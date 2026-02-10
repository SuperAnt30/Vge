using System;
using System.Runtime.CompilerServices;
using Vge.Entity;
using Vge.Entity.Player;
using Vge.Entity.Sizes;
using Vge.Util;
using Vge.World.Block;
using Vge.World.Chunk;
using Vge.World.Light;
using WinGL.Util;

namespace Vge.World
{
    /// <summary>
    /// Абстрактный объект мира
    /// </summary>
    public abstract class WorldBase
    {
        /// <summary>
        /// Это значение true для клиентских миров и false для серверных миров.
        /// </summary>
        public bool IsRemote { get; protected set; }
        /// <summary>
        /// Объект генератора случайных чисел
        /// </summary>
        public Rand Rnd { get; protected set; }
        /// <summary>
        /// Объект сыщик
        /// </summary>
        public Profiler Filer { get; protected set; }
        /// <summary>
        /// Посредник чанков
        /// </summary>
        public ChunkProvider ChunkPr { get; protected set; }
        /// <summary>
        /// Настройки мира
        /// </summary>
        public WorldSettings Settings { get; protected set; }

        /// <summary>
        /// Список всех сущностей во всех загруженных в данный момент чанках 
        /// EntityBase
        /// </summary>
        public MapEntity<EntityBase> LoadedEntityList { get; protected set; } = new MapEntity<EntityBase>();
        /// <summary>
        /// Список сущностей которые надо выгрузить
        /// </summary>
        public ListMessy<EntityBase> UnloadedEntityList { get; protected set; } = new ListMessy<EntityBase>();
        /// <summary>
        /// Список игроков в мире
        /// </summary>
        public ListMessy<PlayerBase> PlayerEntities { get; protected set; } = new ListMessy<PlayerBase>();

        /// <summary>
        /// Объект обработки освещения для мира
        /// </summary>
        public readonly WorldLight Light;
        /// <summary>
        /// Объект проверки коллизии
        /// </summary>
        public readonly CollisionBase Collision;

        public WorldBase(int numberBlocksToLight)
        {
            Light = new WorldLight(this, numberBlocksToLight);
            Collision = new CollisionBase(this);
        }

        /// <summary>
        /// Внести лог
        /// </summary>
        public virtual void SetLog(string logMessage, params object[] args) { }

        /// <summary>
        /// Получить время в тактах объекта Stopwatch с момента запуска проекта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual long ElapsedTicks() => 0;

        #region Chunk

        /// <summary>
        /// Получить чанк по координатам чанка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChunkBase GetChunk(int chunkPosX, int chunkPosY) 
            => ChunkPr.GetChunk(chunkPosX, chunkPosY);
        /// <summary>
        /// Получить чанк по координатам чанка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChunkBase GetChunk(Vector2i chunkPos)
            => ChunkPr.GetChunk(chunkPos);

        #endregion

        #region Block

        /// <summary>
        /// Получить блок данных
        /// </summary>
        public BlockState GetBlockState(BlockPos blockPos)
        {
            if (blockPos.IsValid(ChunkPr.Settings))
            {
                ChunkBase chunk = GetChunk(blockPos.GetPositionChunk());
                if (chunk != null)
                {
                    return chunk.GetBlockStateNotCheck(blockPos.X & 15, blockPos.Y, blockPos.Z & 15);
                }
            }
            return new BlockState().Empty();
        }

        /// <summary>
        /// Изменить метданные блока
        /// </summary>
        public void SetBlockStateMet(BlockPos blockPos, int met, bool isMarkUpdate = true)
        {
            if (blockPos.IsValid(ChunkPr.Settings))
            {
                ChunkBase chunk = ChunkPr.GetChunk(blockPos.GetPositionChunk());
                if (chunk == null) return;
                int yc = blockPos.Y >> 4;
                int index = (blockPos.Y & 15) << 8 | (blockPos.Z & 15) << 4 | (blockPos.X & 15);
                chunk.StorageArrays[yc].NewMetBlock(index, met);
                if (isMarkUpdate) MarkBlockForUpdate(blockPos.X, blockPos.Y, blockPos.Z);
            }
        }

        /// <summary>
        /// Задать блок неба, с флагом по умолчанию 46 (уведомление соседей, modifyRender, modifySave, isCheckLiquid)
        /// флаг, 1 частички старого блока, 2 уведомление соседей, 
        /// 4 modifyRender, 8 modifySave, 16 sound break, 32 проверка доп жидкости,
        /// 64 OnBreakBlock отключить!
        /// </summary>
        /// <param name="blockPos">позици блока</param>
        /// <param name="flag">флаг, 1 частички старого блока, 2 уведомление соседей, 4 modifyRender, 
        /// 8 modifySave, 16 sound break, 32 проверка доп жидкости</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetBlockToAir(BlockPos blockPos, int flag = 46) 
            => SetBlockState(blockPos, new BlockState(0), flag);

        /// <summary>
        /// Задать блок неба доп жидкости или жидкости если она есть, с флагом по умолчанию 46 (уведомление соседей, modifyRender, modifySave, isCheckLiquid)
        /// флаг, 1 частички старого блока, 2 уведомление соседей, 
        /// 4 modifyRender, 8 modifySave, 16 sound break, 32 проверка доп жидкости,
        /// 64 OnBreakBlock отключить!
        /// </summary>
        /// <param name="blockPos">позици блока</param>
        /// <param name="flag">флаг, 1 частички старого блока, 2 уведомление соседей, 4 modifyRender, 
        /// 8 modifySave, 16 sound break, 32 проверка доп жидкости</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetLiquidBlockToAir(BlockPos blockPos, int flag = 46)
            => SetBlockState(blockPos, new BlockState(0 | (2 << 12)), flag);

        /// <summary>
        /// Сменить блока без проверки доп жидкости, как есть!
        /// </summary>
        /// <param name="blockPos">позици блока</param>
        /// <param name="blockState">данные блока</param>
        /// <param name="flag">флаг, 1 частички старого блока, 2 уведомление соседей, 
        /// 4 modifyRender, 8 modifySave, 16 sound break, 32 проверка доп жидкости,
        /// 64 OnBreakBlock отключить!</param>
        /// <returns>true смена была</returns>
        public bool SetBlockState(BlockPos blockPos, BlockState blockState, int flag)
        {
            if (!blockPos.IsValid(ChunkPr.Settings)) return false;

            ChunkBase chunk = ChunkPr.GetChunk(blockPos.GetPositionChunk());
            if (chunk == null) return false;

            BlockState blockStateTrue = chunk.SetBlockState(blockPos, blockState, flag);
            if (blockStateTrue.IsEmpty()) return false;

            if (!IsRemote)
            {
                //    // Частички блока, только на сервере чтоб всем разослать
                //    if ((flag & 1) != 0) ParticleDiggingBlock(blockStateTrue.GetBlock(), blockPos, 50);
                // Уведомление соседей только на сервере!
                if ((flag & 2) != 0) _NotifyNeighborsOfStateChange(blockPos, blockState.GetBlock());
            }

            return true;
        }

        /// <summary>
        /// Уведомить соседей об изменении состояния
        /// </summary>
        protected virtual void _NotifyNeighborsOfStateChange(BlockPos pos, BlockBase block) { }

        #endregion

        #region Mark

        /// <summary>
        /// Отметить блок для обновления
        /// </summary>
        public virtual void MarkBlockForUpdate(int x, int y, int z) { }
        /// <summary>
        /// Отметить блоки для обновления
        /// </summary>
        public virtual void MarkBlockRangeForUpdate(int x0, int y0, int z0, int x1, int y1, int z1) { }
        /// <summary>
        /// Отметить блоки для изминения
        /// </summary>
        public virtual void MarkBlockRangeForModified(int x0, int z0, int x1, int z1) { }

        #endregion

        #region Entity

        /// <summary>
        /// Вызывается, когда объект появляется в мире. Это включает в себя игроков
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SpawnEntityInWorld(EntityBase entity)
            => _OnEntityAdded(entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _OnEntityAdded(EntityBase entity)
        {
            if (entity is PlayerBase player)
            {
                PlayerEntities.Add(player);
                //TODO::2023-07-07 Флаг сна всех игроков
                //UpdateAllPlayersSleepingFlag();
            }
            LoadedEntityList.Add(entity.Id, entity);
        }

        /// <summary>
        /// Вызывается для всех World, когда сущность выгружается или уничтожается. 
        /// В клиентских мирах освобождает любые загруженные текстуры.
        /// В серверных мирах удаляет сущность из трекера сущностей.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _OnEntityRemoved(EntityBase entity)
        {
            if (entity is PlayerBase player)
            {
                PlayerEntities.Remove(player);
                //TODO::2023-07-07 Флаг сна всех игроков
                //UpdateAllPlayersSleepingFlag();
            }
        }

        /// <summary>
        /// Обновляет (и очищает) объекты и объекты чанка 
        /// </summary>
        protected virtual void _UpdateEntities()
        {
            // колекция для удаления
            //  MapListEntity entityRemove = new MapListEntity();

            //profiler.StartSection("EntitiesUnloadedList");

            //// Выгружаем сущности которые имеются в списке выгрузки
            //LoadedEntityList.RemoveRange(UnloadedEntityList);
            //entityRemove.AddRange(UnloadedEntityList);
            //UnloadedEntityList.Clear();

            Filer.EndStartSection("EntityTick");


            // Пробегаем по всем сущностям и обрабатываеи их такт
            for (int i = 0; i < LoadedEntityList.Count; i++)
            {
                EntityBase entity = LoadedEntityList.GetAt(i);

                if (entity != null)
                {
                    if (!entity.IsDead)
                    {
                        _UpdateEntity(entity);
                    }
                    else
                    {
                        UnloadedEntityList.Add(entity);
                    }
                }
            }

            Filer.EndStartSection("EntityRemove");

            // Удаляем 
            while (UnloadedEntityList.Count > 0)
            {
                EntityBase entity = UnloadedEntityList[UnloadedEntityList.Count - 1];
                UnloadedEntityList.RemoveLast();
                if (entity.AddedToChunk)
                {
                    ChunkBase chunk = GetChunk(entity.ChunkPositionX, entity.ChunkPositionZ);
                    if (chunk != null)
                    {
                        chunk.RemoveEntity(entity);
                    }
                    // Для пробуждения сущностей сверху
                    if (entity.Size is ISizeEntityBox size)
                    {
                        Collision.EntityBoundingBoxesFromSector(size.GetBoundingBox().Up(.2f), entity.Id);
                        int count = Collision.ListEntity.Count;
                        for (int i = 0; i < count; i++)
                        {
                            Collision.ListEntity[i].AwakenPhysicSleep();
                        }
                    }
                    // Для отладки
                    _FlagDebugChunkProviderServer();
                }
                LoadedEntityList.Remove(entity.Id, entity);
                _OnEntityRemoved(entity);
            }

            Filer.EndSection();
        }

        /// <summary>
        /// Изменение в каждом такте сущности
        /// </summary>
        protected virtual void _UpdateEntity(EntityBase entity)
        {
            // Добавление сущности в чанк или выгрузка
            int cx = entity.ChunkPositionX;
            int cy = entity.ChunkPositionY;
            int cz = entity.ChunkPositionZ;
            bool check = !entity.AddedToChunk || cx != entity.ChunkPositionPrevX || cz != entity.ChunkPositionPrevZ;
            if (check || cy != entity.ChunkPositionPrevY)
            {
                ChunkBase chunk = null;
                if (entity.AddedToChunk)
                {
                    chunk = GetChunk(entity.ChunkPositionPrevX, entity.ChunkPositionPrevZ);
                    // Удаляем из старого псевдо чанка
                    if (chunk != null)
                    {
                        chunk.RemoveEntityAtIndex(entity, entity.ChunkPositionPrevY);
                    }
                }
                if (check)
                {
                    // Если была смена чанка
                    chunk = GetChunk(cx, cz);
                }
                if (chunk != null)
                {
                    // Добавляем в новый псевдочанк
                    entity.AddedToChunk = true;
                    chunk.AddEntity(entity, cx, cy, cz);
                }
                else
                {
                    // Если нет чанка помечаем что сущность без чанка
                    entity.AddedToChunk = false;
                }
                // Для отладки
                _FlagDebugChunkProviderServer();
            }
        }

        /// <summary>
        /// Флаг для отладки сервера
        /// </summary>
        protected virtual void _FlagDebugChunkProviderServer() { }

        #endregion

        /// <summary>
        /// Для отладки
        /// </summary>
        public virtual void DebugString(string logMessage, params object[] args) { }
    }
}
