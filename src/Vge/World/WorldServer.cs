using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Entity;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Management;
using Vge.Network.Packets.Server;
using Vge.Util;
using Vge.World.Block;
using Vge.World.BlockEntity;
using Vge.World.Chunk;
using Vge.World.Gen;
using WinGL.Util;

namespace Vge.World
{
    /// <summary>
    /// Серверный объект мира
    /// </summary>
    public class WorldServer : WorldBase
    {
        /// <summary>
        /// ID мира
        /// </summary>
        public readonly byte IdWorld;
        /// <summary>
        /// Имя пути к папке
        /// </summary>
        public readonly string PathWorld;
        /// <summary>
        /// Основной сервер
        /// </summary>
        public readonly GameServer Server;
        /// <summary>
        /// Объект управляет всеми чанками которые надо загрузить или выгрузить
        /// </summary>
        public readonly FragmentManager Fragment;
        /// <summary>
        /// Трекер сущностей
        /// </summary>
        public readonly EntityTrackerManager Tracker;
        /// <summary>
        /// Посредник серверного чанка
        /// </summary>
        public readonly ChunkProviderServer ChunkPrServ;
        /// <summary>
        /// Ждать обработчик
        /// </summary>
        public readonly WaitHandler Wait;
        /// <summary>
        /// Запущен ли мир
        /// </summary>
        public bool IsRuning { get; private set; } = true;
        /// <summary>
        /// Увеличивается каждый такт конкретного мира
        /// </summary>
        public uint TickCounter { get; private set; }

        /// <summary>
        /// Кешовы Список BlockTick блоков которые должны мгновенно тикать,
        /// нужен чтоб непересоздавать его в каждом чанке в каждом тике
        /// </summary>
        public readonly ListMessy<BlockTick> TickBlocksCache = new ListMessy<BlockTick>();
        
        /// <summary>
        /// Время затраченое за такт
        /// </summary>
        private short _timeTick = 0;
        /// <summary>
        /// Содержит текущее начальное число линейного конгруэнтного генератора для обновлений блоков. 
        /// Используется со значением A, равным 3, и значением C, равным 0x3c6ef35f, 
        /// создавая очень плоский ряд значений, плохо подходящих для выбора случайных блоков в поле 16x128x16.
        /// </summary>
        private int _updateLCG;

        //private readonly TestAnchor _testAnchor;

        public WorldServer(GameServer server, byte idWorld, WorldSettings worldSettings) 
            : base(worldSettings.NumberChunkSections * 16)
        {
            Server = server;
            IdWorld = idWorld;
            PathWorld = server.Settings.GetPathWorld(IdWorld);
            Settings = worldSettings;
            Rnd = new Rand(server.Settings.Seed);
            ChunkPr = ChunkPrServ = new ChunkProviderServer(this);
            Collision.Init();
            Filer = new Profiler(server.Log, "[Server] ");
            Fragment = new FragmentManager(this);
            Tracker = new EntityTrackerManager(this);

            if (idWorld == 0)
            {
                //_testAnchor = new TestAnchor(this);
                //Fragment.AddAnchor(_testAnchor);
            }

            if (idWorld != 0)
            {
                // Все кроме основного мира, использую дополнительный поток
                Wait = new WaitHandler("World" + IdWorld);
                Wait.DoInFlow += (sender, e) => Update();
                Wait.Run();
            }
        }

        /// <summary>
        /// Внести лог
        /// </summary>
        public override void SetLog(string logMessage, params object[] args) 
            => Filer.Log.Server(logMessage, args);

        #region BlockCaches

        /// <summary>
        /// Экспортировать в мир временные блоки из кэша блоков обнолений
        /// </summary>
        public void ExportBlockCaches()
        {
            int count = Settings.BlockCaches.Count;
            if (count > 0)
            {
                BlockCache blockCache;
                for (int i = 0; i < count; i++)
                {
                    blockCache = Settings.BlockCaches[i];
                    SetBlockState(blockCache.Position, blockCache.GetBlockState(), 46);
                }
            }
            Settings.BlockCaches.Clear();
        }

        #endregion

        /// <summary>
        /// Такт выполнения
        /// </summary>
        public void Update()
        {
            long timeBegin = Server.Time();
            Settings.Calendar.UpdateServer();

            // Обработка фрагментов в начале такта
            _FragmentBegin();
            _FragmentEnd();
            // Тут начинается все действия с блоками АИ мобов и всё такое....
            if (IdWorld == 0)
            {
              //  _testAnchor.Update();
            }

            // Тикание блоков и чанков
            _TickBlocks();

            // TODO::2026-02-06!!! Поток генерации
            // Обработка фрагментов в конце такта
            //_FragmentEnd();

            Filer.StartSection("Entities");
            _UpdateEntities();
            Filer.EndStartSection("Tracker");
            Tracker.Update();
            Filer.EndSection();

            _timeTick = (short)((_timeTick * 3 + (Server.Time() - timeBegin)) / 4);
        }

        /// <summary>
        /// Останавливаем мир
        /// </summary>
        public void Stoping()
        {
            IsRuning = false;
            ChunkPrServ.Wait.Stoping();
            Wait?.Stoping();
            _WriteToFile();
        }

        /// <summary>
        /// Внести изменение по мировому времени
        /// </summary>
        public void SetTickCounter(uint tickCounter)
        {
            Settings.Calendar.SetTickCounter(tickCounter);
            Tracker.SendToAll(new PacketS04TickUpdate(tickCounter));
        }

        #region Fragments (Chunks)

        /// <summary>
        /// Отметить блок для обновления 
        /// </summary>
        public override void MarkBlockForUpdate(int x, int y, int z)
            => Fragment.FlagBlockForUpdate(x, y, z);

        /// <summary>
        /// Отметить блоки для изминения
        /// </summary>
        public override void MarkBlockRangeForModified(int x0, int z0, int x1, int z1)
        {
            int c0x = (x0) >> 4;
            int c0z = (z0) >> 4;
            int c1x = (x1) >> 4;
            int c1z = (z1) >> 4;
            int x, z;
            for (x = c0x; x <= c1x; x++)
            {
                for (z = c0z; z <= c1z; z++)
                {
                    GetChunkServer(x, z)?.Modified();
                }
            }
        }

        /// <summary>
        /// Отметить псевдочанки на обновления, сервер. Координаты чанков
        /// </summary>
        public void ServerMarkChunkRangeForRenderUpdate(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            int x, y, z;
            for (x = x0; x <= x1; x++)
            {
                for (z = z0; z <= z1; z++)
                {
                    for (y = y0; y <= y1; y++)
                    {
                        Fragment.FlagChunkForUpdate(x, y, z);
                    }
                }
            }
        }

        /// <summary>
        /// Обработка фрагментов в начале такта
        /// </summary>
        private void _FragmentBegin()
        {
            TickCounter++;
            if (TickCounter % Ce.Tps == 0)
            {
                // Прошла секунда
                ChunkPrServ.ClearCounter();
            }
            // Запускаем фрагмент, тут определение какие чанки выгрузить, какие загрузить, 
            // определение активных чанков.
            Filer.StartSection("Fragment");
            Fragment.Update();
            // Выгрузка ненужных чанков из очереди
            Filer.StartSection("UnloadingUnnecessaryChunksFromQueue");
            ChunkPrServ.UnloadingUnnecessaryChunksFromQueue();
            Filer.EndSection();

            // Этап запуска чанков в отдельном потоке
            ChunkPrServ.Wait.RunInFlow();
        }

        /// <summary>
        /// Обработка фрагментов в конце такта
        /// </summary>
        private void _FragmentEnd()
        {
            // Дожидаемся загрузки чанков
            ChunkPrServ.Wait.Waiting();

            // Выгрузка требуемых чанков из очереди которые отработали в отдельном потоке
            Filer.EndStartSection("UnloadingRequiredChunksFromQueue");
            ChunkPrServ.UnloadingRequiredChunksFromQueue();
            Filer.EndSection();
        }

        #endregion

        #region Chunk

        /// <summary>
        /// Получить чанк по координатам чанка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChunkServer GetChunkServer(int chunkPosX, int chunkPosY) 
            => ChunkPr.GetChunk(chunkPosX, chunkPosY) as ChunkServer;
        /// <summary>
        /// Получить чанк по координатам чанка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChunkServer GetChunkServer(Vector2i chunkPos)
            => ChunkPr.GetChunk(chunkPos) as ChunkServer;
        
        /// <summary>
        /// Получить чанк по координатам блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChunkServer GetChunkServer(BlockPos blockPos)
            => ChunkPr.GetChunk(blockPos.GetPositionChunkX(), blockPos.GetPositionChunkZ()) as ChunkServer;

        #endregion

        #region Entity

        /// <summary>
        /// Сущность бросатет другую сущность в мире.
        /// </summary>
        /// <param name="entity">Сущность кторая кидает</param>
        /// <param name="entityThrown">Сущность которую кунули</param>
        /// <param name="inFrontOf">Флаг перед собой</param>
        /// <param name="longAway">Далеко бросить от себя</param>
        public virtual void EntityDropsEntityInWorld(EntityLiving entity, EntityBase entityThrown, bool inFrontOf, bool longAway)
        {
            if (entityThrown.Physics != null)
            {
                if (inFrontOf)
                {
                    if (longAway)
                    {
                        float pitchxz = Glm.Cos(entity.RotationPitch);
                        entityThrown.Physics.MotionX = Glm.Sin(entity.RotationYaw) * pitchxz * 1.4f;
                        entityThrown.Physics.MotionY = Glm.Sin(entity.RotationPitch) * 1.4f;
                        entityThrown.Physics.MotionZ = -Glm.Cos(entity.RotationYaw) * pitchxz * 1.4f;
                        float f1 = Rnd.NextFloat() * .02f;
                        float f2 = Rnd.NextFloat() * Glm.Pi360;
                        entityThrown.Physics.MotionX += Glm.Cos(f2) * f1;
                        entityThrown.Physics.MotionZ += Glm.Sin(f2) * f1;
                    }
                    else
                    {
                        float pitchxz = Glm.Cos(entity.RotationPitch);
                        entityThrown.Physics.MotionX = Glm.Sin(entity.RotationYaw) * pitchxz * .3f;
                        entityThrown.Physics.MotionY = Glm.Sin(entity.RotationPitch) * .3f + .1f;
                        entityThrown.Physics.MotionZ = -Glm.Cos(entity.RotationYaw) * pitchxz * .3f;
                        float f1 = Rnd.NextFloat() * .02f;
                        float f2 = Rnd.NextFloat() * Glm.Pi360;
                        entityThrown.Physics.MotionX += Glm.Cos(f2) * f1;
                        entityThrown.Physics.MotionY += (Rnd.NextFloat() - Rnd.NextFloat()) * .1f;
                        entityThrown.Physics.MotionZ += Glm.Sin(f2) * f1;
                    }
                }
                else
                {
                    float f1 = Rnd.NextFloat() * .5f;
                    float f2 = Rnd.NextFloat() * Glm.Pi360;
                    entityThrown.Physics.MotionX = -Glm.Sin(f2) * f1;
                    entityThrown.Physics.MotionY = .2f;
                    entityThrown.Physics.MotionZ = Glm.Cos(f2) * f1;
                }

                entityThrown.PosX = entity.PosX;
                entityThrown.PosY = entity.PosY;
                entityThrown.PosZ = entity.PosZ;

                if (longAway)
                {
                    entityThrown.PosX += Glm.Cos(entity.RotationYaw) * .32f;
                    entityThrown.PosZ += Glm.Sin(entity.RotationYaw) * .32f;
                    entityThrown.PosY += entity.GetEyeHeight() - .2f;
                }
                else
                {
                    entityThrown.PosY += entity.GetEyeHeight() / 2f;
                }

                entityThrown.BeforeDrop();
                SpawnEntityInWorld(entityThrown);
            }
        }

        /// <summary>
        /// Вызывается, когда объект появляется в мире. Это включает в себя игроков
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SpawnEntityInWorld(EntityBase entity)
        {
            entity.SetEntityId(Server.LastEntityId());
            base.SpawnEntityInWorld(entity);
        }

        protected override void _OnEntityAdded(EntityBase entity)
        {
            base._OnEntityAdded(entity);
            Tracker.EntityAdd(entity);
            entity.SpawnServer();
        }

        protected override void _OnEntityRemoved(EntityBase entity)
        {
            base._OnEntityRemoved(entity);
            Tracker.UntrackEntity(entity);
        }

        /// <summary>
        /// Удаление игрока в текущем мире для перехода в другой мир
        /// </summary>
        public void RemovePlayerInWorldForNextWorld(PlayerServer player)
        {
            Fragment.RemoveAnchor(player);
            player.SetDead();
            _OnEntityRemoved(player);
            LoadedEntityList.Remove(player.Id, player);
        }

        /// <summary>
        /// Игрок переходит в другой мир
        /// </summary>
        public void PlayerForNextWorld(PlayerServer player)
        {
            Fragment.AddAnchor(player);
            SpawnEntityInWorld(player);
        }

        /// <summary>
        /// Флаг для отладки сервера
        /// </summary>
        protected override void _FlagDebugChunkProviderServer() 
            => Fragment.flagDebugChunkProviderServer = true;

        /// <summary>
        /// Изменение в каждом такте сущности
        /// </summary>
        protected override void _UpdateEntity(EntityBase entity)
        {
            if (entity.AddedToChunk)
            {
                entity.Update();
            }
            base._UpdateEntity(entity);
        }

        #endregion

        #region BlockEntity

        /// <summary>
        /// Список всех блок сущностей в квадрате 3*3 чанка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<BlockEntityBase> GetBlocksEntity3x3(ChunkServer chunk)
        {
            List<BlockEntityBase> blocksEntity = new List<BlockEntityBase>();
            chunk.AddRangeBlockEntity(blocksEntity);
            for (int i = 0; i < 8; i++)
            {
                GetChunkServer(chunk.X + Ce.AreaOne8X[i], chunk.Y + Ce.AreaOne8Y[i]).AddRangeBlockEntity(blocksEntity);
            }
            return blocksEntity;
        }

        #endregion

        #region Blocks

        /// <summary>
        /// Тикание блоков и чанков
        /// </summary>
        private void _TickBlocks()
        {
            int count = Fragment.ListChunkAction.Count;
            if (count > 0)
            {
                // Случайная скорость тика, для случайных обновлений блока в чанке, параметр из майна 1.8
                int randomTickSpeed = 3;
                ulong index;
                int yc, i, j, x, y, z, k;
                ChunkServer chunk;
                ChunkStorage chunkStorage;
                BlockPos blockPos = new BlockPos();
                BlockState blockState;
                BlockBase block;

                for (i = 0; i < count; i++)
                {
                    index = Fragment.ListChunkAction[i];
                    chunk = GetChunkServer(Conv.IndexToChunkX(index), Conv.IndexToChunkY(index));

                    if (chunk != null && chunk.IsSendChunk)
                    {
                        Filer.StartSection("TickChunk " + chunk.X + ":" + chunk.Y);
                        chunk.UpdateServer();
                        Filer.EndSection();

                        // Тикаем рандом блоки
                        for (yc = 0; yc < chunk.NumberSections; yc++)
                        {
                            chunkStorage = chunk.StorageArrays[yc];
                            if (!chunkStorage.IsEmptyData() && chunkStorage.GetNeedsRandomTick())
                            {
                                for (j = 0; j < randomTickSpeed; j++)
                                {
                                    _updateLCG = _updateLCG * 3 + 1013904223;
                                    k = _updateLCG >> 2;
                                    x = k & 15;
                                    z = k >> 8 & 15;
                                    y = k >> 16 & 15;
                                    blockPos.X = x + chunk.BlockX;
                                    blockPos.Y = y + chunkStorage.YBase;
                                    blockPos.Z = z + chunk.BlockZ;
                                    blockState = chunkStorage.GetBlockState(x, y, z);
                                    block = blockState.GetBlock();
                                    if (block.NeedsRandomTick)
                                    {
                                        block.RandomTick(this, chunk, blockPos, blockState, Rnd);
                                        if (chunkStorage.IsEmptyData() || !chunkStorage.GetNeedsRandomTick())
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Задать тик блока
        /// </summary>
        public void SetBlockTick(BlockPos blockPos, uint timeTick, bool priority = false)
        {
            if (blockPos.IsValid(ChunkPr.Settings))
            {
                GetChunkServer(blockPos.GetPositionChunk())
                    ?.SetBlockTick(blockPos.X & 15, blockPos.Y, blockPos.Z & 15, false, timeTick, priority);
            }
        }

        /// <summary>
        /// Задать тик дополнительного блока жидкости
        /// </summary>
        public void SetBlockAddLiquidTick(BlockPos blockPos, uint timeTick)
        {
            if (blockPos.IsValid(ChunkPr.Settings))
            {
                GetChunkServer(blockPos.GetPositionChunk())
                    ?.SetBlockTick(blockPos.X & 15, blockPos.Y, blockPos.Z & 15, true, timeTick, false);
            }
        }

        /// <summary>
        /// Уведомить соседей об изменении состояния
        /// </summary>
        protected override void _NotifyNeighborsOfStateChange(BlockPos pos, BlockBase block)
        {
            _NotifyBlockOfStateChange(pos.OffsetWest(), block);
            _NotifyBlockOfStateChange(pos.OffsetEast(), block);
            _NotifyBlockOfStateChange(pos.OffsetDown(), block);
            _NotifyBlockOfStateChange(pos.OffsetUp(), block);
            _NotifyBlockOfStateChange(pos.OffsetNorth(), block);
            _NotifyBlockOfStateChange(pos.OffsetSouth(), block);
            //if (block.IsAir)
            //{
            //    // Для дверей диагональ
            //    _NotifyBlockOfStateChange(pos.Offset(1, 0, 1), block);
            //    _NotifyBlockOfStateChange(pos.Offset(-1, 0, 1), block);
            //    _NotifyBlockOfStateChange(pos.Offset(1, 0, -1), block);
            //    _NotifyBlockOfStateChange(pos.Offset(-1, 0, -1), block);
            //}
        }

        /// <summary>
        /// Уведомить о блок об изменения состоянии соседнего блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _NotifyBlockOfStateChange(BlockPos pos, BlockBase block)
        {
            try
            {
                ChunkServer chunk = GetChunkServer(pos);
                if (chunk != null)
                {
                    BlockState blockState = chunk.GetBlockState(pos);
                    blockState.GetBlock().NeighborBlockChange(this, chunk, pos, blockState, block);
                }
            }
            catch (Exception ex)
            {
                Logger.Crash(ex, "NotifyBlockOfStateChange " + pos.ToString());
            }
        }

        #endregion

        #region WriteRead

        /// <summary>
        /// Записать данные мира
        /// </summary>
        private void _WriteToFile()
        {
            GameFile.CheckPath(PathWorld);
        }

        #endregion

        public override string ToString()
        {
            int chBt = 0;
            int chBe = 0;
            if (Server.Players.PlayerOwner != null &&
                Server.Players.PlayerOwner.IdWorld == IdWorld)
            {
                ChunkServer chunk = GetChunkServer(Server.Players.PlayerOwner.GetChunkPosition());
                chBt = chunk.GetTickBlockCount();
                chBe = chunk.GetBlockEntityCount();
            }
            return "World-" + IdWorld
                + " " + _timeTick + "ms " + Fragment.ToString() + " " + ChunkPrServ.ToString()
                + "\r\n" + Tracker + " ChBt: " + chBt + " ChBe: " + chBe;
        }
        
    }
}
