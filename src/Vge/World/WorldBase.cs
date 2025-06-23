using System.Runtime.CompilerServices;
using Vge.Entity;
using Vge.Entity.Player;
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
        /// Задать блок неба, с флагом по умолчанию 14 (уведомление соседей, modifyRender, modifySave)
        /// </summary>
        /// <param name="blockPos">позици блока</param>
        /// <param name="flag">флаг, 1 частички старого блока, 2 уведомление соседей, 4 modifyRender, 8 modifySave, 16 sound break</param>
        public bool SetBlockToAir(BlockPos blockPos, int flag = 14) 
            => SetBlockState(blockPos, new BlockState(0), flag);

        /// <summary>
        /// Сменить блок
        /// </summary>
        /// <param name="blockPos">позици блока</param>
        /// <param name="blockState">данные блока</param>
        /// <param name="flag">флаг, 1 частички старого блока, 2 уведомление соседей, 4 modifyRender, 8 modifySave, 16 sound break</param>
        /// <returns>true смена была</returns>
        public virtual bool SetBlockState(BlockPos blockPos, BlockState blockState, int flag)
        {
            if (!blockPos.IsValid(ChunkPr.Settings)) return false;

            ChunkBase chunk = ChunkPr.GetChunk(blockPos.GetPositionChunk());
            if (chunk == null) return false;

            BlockState blockStateTrue = chunk.SetBlockState(blockPos, blockState, (flag & 8) != 0, (flag & 4) != 0, (flag & 16) != 0);
            if (blockStateTrue.IsEmpty()) return false;

            //BlockState blockStateTrue = chunk.SetBlockState(pos.X, pos.Y, pos.Z, blockState, (flag & 8) != 0, (flag & 4) != 0, (flag & 16) != 0);
            //if (blockStateTrue.IsEmpty()) return false;

            //if (!IsRemote)
            //{
            //    // Частички блока, только на сервере чтоб всем разослать
            //    if ((flag & 1) != 0) ParticleDiggingBlock(blockStateTrue.GetBlock(), blockPos, 50);
            //}
            //// Уведомление соседей и на сервере и на клиенте
            //if ((flag & 2) != 0) NotifyNeighborsOfStateChange(blockPos, blockState.GetBlock());

            return true;
        }

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
        public virtual void SpawnEntityInWorld(EntityBase entity)
            => _OnEntityAdded(entity);

        /// <summary>
        /// Удаление сущности в текущем мире
        /// </summary>
        public virtual void RemoveEntityInWorld(EntityBase entity)
        {
            entity.SetDead();
            _OnEntityRemoved(entity);
        }

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

            //profiler.EndStartSection("EntityTick");


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

            //profiler.EndStartSection("EntityRemove");

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
                    // Для отладки
                    _FlagDebugChunkProviderServer();
                }
                LoadedEntityList.Remove(entity.Id, entity);
                _OnEntityRemoved(entity);
            }

            //profiler.EndSection();
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
