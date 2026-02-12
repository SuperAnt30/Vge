using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using Vge.Entity;
using Vge.Util;
using Vge.World.Block;
using Vge.World.Light;
using WinGL.Util;

namespace Vge.World.Chunk
{
    /// <summary>
    /// Базовый объект чанка
    /// </summary>
    public abstract class ChunkBase : IChunkPosition
    {
        /// <summary>
        /// Исходящий буфер памяти для Zip
        /// </summary>
        private readonly static MemoryStream _bigStreamOut = new MemoryStream();

        /// <summary>
        /// Дополнительные данные покуда для отладки
        /// </summary>
        public object Tag
        {
            get
            {
                return ListEntities.Count;
            }
        }

        /// <summary>
        /// Опции высот чанка
        /// </summary>
        public readonly ChunkSettings Settings;

        /// <summary>
        /// Позиция X текущего чанка
        /// </summary>
        public readonly int X;
        /// <summary>
        /// Позиция Y текущего чанка
        /// </summary>
        public readonly int Y;
        /// <summary>
        /// Блочная позиция X текущего чанка
        /// </summary>
        public readonly int BlockX;
        /// <summary>
        /// Блочная позиция Z текущего чанка
        /// </summary>
        public readonly int BlockZ;
        /// <summary>
        /// Позиция X текущего чанка
        /// </summary>
        public int CurrentChunkX { get; private set; }
        /// <summary>
        /// Позиция Y текущего чанка
        /// </summary>
        public int CurrentChunkY { get; private set; }
        /// <summary>
        /// Ключ кэш координат чанка (ulong)(uint)x  32) | ((uint)y
        /// </summary>
        public readonly ulong KeyCash;
        /// <summary>
        /// Сылка на объект мира
        /// </summary>
        public readonly WorldBase World;
        /// <summary>
        /// Данные чанка
        /// </summary>
        public readonly ChunkStorage[] StorageArrays;
        /// <summary>
        /// Количество секций в чанке (old COUNT_HEIGHT)
        /// </summary>
        public readonly byte NumberSections;
        /// <summary>
        /// Объект работы с освещением
        /// </summary>
        public readonly ChunkLight Light;

        /// <summary>
        /// Биомы в индексах
        /// z << 4 | x;
        /// </summary>
        public readonly byte[] Biome = new byte[256];
        /// <summary>
        /// Список сущностей в каждом псевдочанке
        /// </summary>
        public readonly MapEntity<EntityBase>[] ListEntitiesSection;
        /// <summary>
        /// Список всех сущностей в текущем чанке
        /// </summary>
        public readonly MapEntity<EntityBase> ListEntities = new MapEntity<EntityBase>();

        /// <summary>
        /// Имеет ли этот фрагмент какие-либо сущности и, следовательно, требует сохранения на каждом тике
        /// </summary>
        protected bool _hasEntities;

        #region Кольца 1-4

        /// <summary>
        /// Присутствует, этап загрузки или начальная генерация #1 1*1
        /// </summary>
        public bool IsChunkPresent { get; protected set; }
        /// <summary>
        /// Было ли декорация чанка #2 3*3
        /// </summary>
        public bool IsDecorated { get; protected set; }
        /// <summary>
        /// Было ли карта высот с небесным освещением #3 5*5
        /// </summary>
        public bool IsHeightMapSky { get; protected set; }
        /// <summary>
        /// Было ли боковое небесное освещение и блочное освещение #4 7*7
        /// </summary>
        public bool IsSideLightSky { get; protected set; }
        /// <summary>
        /// Готов ли чанк для отправки клиентам #5 9*9
        /// </summary>
        public bool IsSendChunk { get; protected set; }
        /// <summary>
        /// Загружен ли чанк, false если была генерация, для дополнительной правки освещения.
        /// Для проверки не корректного освещения.
        /// </summary>
        public bool IsLoaded { get; private set; }

        #endregion

        public ChunkBase(WorldBase world, ChunkSettings settings, int chunkPosX, int chunkPosY)
        {
            World = world;
            X = CurrentChunkX = chunkPosX;
            Y = CurrentChunkY = chunkPosY;
            BlockX = X << 4;
            BlockZ = Y << 4;
            KeyCash = Conv.ChunkXyToIndex(X, Y);
            Settings = settings;
            NumberSections = Settings.NumberSections;
            StorageArrays = new ChunkStorage[NumberSections];
            ListEntitiesSection = new MapEntity<EntityBase>[NumberSections];
            for (int index = 0; index < NumberSections; index++)
            {
                StorageArrays[index] = new ChunkStorage(KeyCash, index);
                ListEntitiesSection[index] = new MapEntity<EntityBase>();
            }
            Light = new ChunkLight(this);
        }

        /// <summary>
        /// Выгрузили чанк
        /// </summary>
        public virtual void OnChunkUnload()
        {
            IsChunkPresent = false;
        }

        /// <summary>
        /// Возвращает самый верхний экземпляр ChunkStorage для этого фрагмента, который содержит блок.
        /// </summary>
        public int GetTopFilledSegment()
        {
            for (int y = StorageArrays.Length - 1; y >= 0; y--)
            {
                if (!StorageArrays[y].IsEmptyData())
                {
                    return StorageArrays[y].YBase;
                }
            }
            return 0;
        }

        #region Block

        /// <summary>
        /// Получить блок данных, по глобальным координатам
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockState GetBlockState(BlockPos blockPos)
            => GetBlockState(blockPos.X & 15, blockPos.Y, blockPos.Z & 15);
        /// <summary>
        /// Получить блок данных, XZ 0..15, Y 0..255
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockState GetBlockState(int x, int y, int z)
        {
            if (x >> 4 == 0 && z >> 4 == 0) return GetBlockStateNotCheck(x, y, z);
            return new BlockState().Empty();
        }

        /// <summary>
        /// Получить блок данных, по глобальным координатам без проверки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockState GetBlockStateNotCheck(BlockPos blockPos)
            => GetBlockStateNotCheck(blockPos.X & 15, blockPos.Y, blockPos.Z & 15);
        /// <summary>
        /// Получить блок данных, XZ 0..15, Y 0..255 без проверки
        /// </summary>
        public BlockState GetBlockStateNotCheck(int x, int y, int z)
        {
            ChunkStorage chunkStorage = StorageArrays[y >> 4];
            if (chunkStorage.CountBlock != 0)
            {
                return chunkStorage.GetBlockState(x, y & 15, z);
            }
            else
            {
                return new BlockState(0, chunkStorage.Light[(y & 15) << 8 | z << 4 | x]);
            }
        }

        /// <summary>
        /// Получить блок данных, по глобальным координатам без проверки и без света
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockState GetBlockStateNotCheckLight(BlockPos blockPos)
            => GetBlockStateNotCheckLight(blockPos.X & 15, blockPos.Y, blockPos.Z & 15);
        /// <summary>
        /// Получить блок данных, XZ 0..15, Y 0..255 без проверки и без света
        /// </summary>
        public BlockState GetBlockStateNotCheckLight(int xz, int y)
        {
            ChunkStorage chunkStorage = StorageArrays[y >> 4];
            return chunkStorage.CountBlock != 0
                ? chunkStorage.GetBlockStateNotLight(xz, y & 15)
                : new BlockState();
        }

        /// <summary>
        /// Получить блок данных, XZ 0..15, Y 0..255 без проверки и без света
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockState GetBlockStateNotCheckLight(int x, int y, int z)
            => GetBlockStateNotCheckLight(z << 4 | x, y);

        /// <summary>
        /// Изменить метданные блока, без изменения для клиента
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlockStateMet(BlockPos blockPos, int met)
        {
            if (blockPos.IsValid(Settings))
            {
                int yc = blockPos.Y >> 4;
                int index = (blockPos.Y & 15) << 8 | (blockPos.Z & 15) << 4 | (blockPos.X & 15);
                StorageArrays[yc].NewMetBlock(index, met);
            }
        }

        /// <summary>
        /// Задать новые данные блока, с перерасчётом освещения если надо и прочего, возвращает прошлые данные блока
        /// </summary>
        /// <param name="blockPos">Позици блока</param>
        /// <param name="blockState">Данные нового блока</param>
        /// <param name="flag">флаг, 4 modifyRender, 8 modifySave, 16 sound break, 
        /// 32 проверка доп жидкости, 64 OnBreakBlock отключить!</param>
        public BlockState SetBlockState(BlockPos blockPos, BlockState blockState, int flag)
        {
            // Пометка надо сохранение чанка
            bool isModify = (flag & 8) != 0;
            // Пометка надо обновить рендер чанка
            bool isModifyRender = (flag & 4) != 0;
            // Звуковое сопровождение сломанного блока
            bool isSound = (flag & 16) != 0;
            // Проверка доп жидкости
            bool isCheckLiquid = (flag & 32) != 0;

            int bx = blockPos.X & 15;
            int by = blockPos.Y;
            int bz = blockPos.Z & 15;

            // Получаем прошлый блок
            BlockState blockStateOld = GetBlockState(bx, by, bz);

            // Если блоки одинаковые, то возвращаем пустоту, ктороая проигнорирует смену
            if (blockState.Equals(blockStateOld)) return new BlockState().Empty();

            BlockBase block = blockState.GetBlock();
            ChunkStorage storage = StorageArrays[by >> 4];
            // Если в секторе блока нет и мы пробуем поставить блок воздуха, выплёвываем игнор
            if (storage.CountBlock == 0 && block.IsAir) return new BlockState().Empty();
            BlockBase blockOld = blockStateOld.GetBlock();

            if (isCheckLiquid)
            {
                // Проверка жидкости, Как мы работаем.
                // Если прилетает блок жидкости, а у нас не жидкость мы пробуем проверить и склеить.
                // Если прилетает не жидкость, мы проверяем была ли доп жидкость и корректируем с ней

                if (block.IsAir)
                {
                    if (blockState.Met == 0)
                    {
                        // Убираем блок
                        if (blockOld.IsAddLiquid(blockStateOld.Met))
                        {
                            // Если прошлый есть доп жыдкость делаем её основной
                            blockState.Id = Ce.Blocks.GetAddLiquidIndex(blockStateOld.Met);
                            blockState.Met = blockOld.GetAddLiquidMet(blockStateOld.Met);
                            block = Ce.Blocks.GetAddLiquid(blockStateOld.Met);
                        }
                    }
                    else
                    {
                        // Убираем жидкость
                        if (blockOld.IsAddLiquid(blockStateOld.Met))
                        {
                            blockState.Id = blockStateOld.Id;
                            blockState.Met = blockStateOld.Met & 0xFFF;
                            block = blockOld;
                        }
                        else
                        {
                            blockState.Met = 0;
                        }
                    }
                }
                else if (block.Liquid)
                {
                    if (!blockOld.Liquid && blockOld.CanAddLiquid())
                    {
                        // Добавляем жидкость в старый блок
                        blockState.Id = blockStateOld.Id;
                        blockState.Met = (block.IndexLiquid << 16)
                            | (blockState.Met << 12)
                            | (blockStateOld.Met & 0xFFF);
                        block = blockOld;
                    }
                }
                else if (blockOld.Liquid)
                {
                    if (!block.Liquid && block.CanAddLiquid())
                    {
                        // Добавляем блок в доп жидкость
                        blockState.Met = (blockOld.IndexLiquid << 16)
                            | (blockStateOld.Met << 12)
                            | (blockState.Met & 0xFFF);
                    }
                }
            }
            else if (block.IsAir && blockState.Met != 0)
            {
                // Для того чтоб убрать блок жидкости
                blockState.Met = 0;
            }

            int index = (by & 15) << 8 | bz << 4 | bx;
            storage.SetData(index, blockState.Id, blockState.Met);

            if (blockOld != block) // Блоки разные
            {
                // Отмена тик блока
                _RemoveBlockTick(bx, by, bz, false);

                bool differenceOpacity = block.LightOpacity != blockOld.LightOpacity;
                if (differenceOpacity || block.LightValue != blockOld.LightValue)
                {
                    World.Light.ActionChunk(this);
                    if (differenceOpacity && World.Settings.HasNoSky)
                    {
                        // Отключаем проверку небесного освещения
                        differenceOpacity = false;
                    }
                    World.Light.CheckLightFor(blockPos, differenceOpacity, isModify, isModifyRender);//, replaceAir);
                }
                else
                {
                    // Проверка высоты
                    Light.CheckHeightMap(blockPos, block.LightOpacity);
                    if (World.IsRemote)
                    {
                        World.Light.ClearDebugString();
                    }
                    //if (isModify) Modified();
                }

                if (isModifyRender)
                {
                    World.MarkBlockForUpdate(blockPos.X, blockPos.Y, blockPos.Z);
                    // Пробуждение сущности
                    _AwakenEntitiesTouching(blockPos);
                }

                if (World.IsRemote)
                {
                    World.DebugString(World.Light.ToDebugString());
                }
                else if ((flag & 64) == 0) // проверку сломаного
                {
                    // Действие блока после его удаления
                    blockOld.OnBreakBlock((WorldServer)World, (ChunkServer)this, blockPos, blockStateOld, blockState);
                }
            }
            else if (blockState.Met != blockStateOld.Met) // Метданные разные
            {
                if (isModifyRender)
                {
                    World.MarkBlockForUpdate(blockPos.X, blockPos.Y, blockPos.Z);
                    // Пробуждение сущности
                    _AwakenEntitiesTouching(blockPos);
                }
            }

            //if (!World.IsRemote && blockOld != block)
            //{
            //    block.OnBlockAdded(World, blockPos, blockState);
            //}

            // Звуковое сопровождение сломанного блока
            if (isSound && !World.IsRemote && World is WorldServer worldServer)
            {
                //vec3 pos = blockPos.ToVec3();
                //worldServer.Tracker.SendToAllEntityDistance(pos, 32f,
                //    new PacketS29SoundEffect(blockOld.SampleBreak(worldServer), pos, 1f, blockOld.SampleBreakPitch(worldServer.Rnd)));
            }

            // Эта строка не понятна мне, зачем это я делал?! 2024-12-04
            if (storage.CountBlock != 0 && storage.Data[index] != block.IndexBlock) return new BlockState();

            return blockStateOld;
        }

        /// <summary>
        /// Отменить мгновенный тик блока
        /// </summary>
        protected virtual void _RemoveBlockTick(int x, int y, int z, bool liquid) { }

        #endregion

        #region Entity

        /// <summary>
        /// Добавить сущность в чанк
        /// </summary>
        public void AddEntity(EntityBase entity, int cx, int cy, int cz)
        {
            _hasEntities = true;
            if (cy < 0) cy = 0; else if (cy >= NumberSections) cy = NumberSections - 1;
            entity.SetPositionChunk(cx, cy, cz);
            ListEntitiesSection[cy].Add(entity.Id, entity);
            ListEntities.Add(entity.Id, entity);
        }

        /// <summary>
        /// Удаляет сущность из конкретного псевдочанка
        /// </summary>
        /// <param name="entity">сущность</param>
        /// <param name="cy">уровень псевдочанка</param>
        public void RemoveEntityAtIndex(EntityBase entity, int cy)
        {
            if (cy < 0) cy = 0; else if (cy >= NumberSections) cy = NumberSections - 1;
            ListEntitiesSection[cy].Remove(entity.Id, entity);
            ListEntities.Remove(entity.Id, entity);
        }

        /// <summary>
        /// Удаляет сущность, используя его координату y в качестве индекса
        /// </summary>
        /// <param name="entity">сущность</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveEntity(EntityBase entity) => RemoveEntityAtIndex(entity, entity.ChunkPositionY);

        /// <summary>
        /// Пробудить все сущности данного чанка
        /// </summary>
        public void AwakenAllEntities()
        {
            for (int i = 0; i < ListEntities.Count; i++)
            {
                ListEntities.GetAt(i).AwakenPhysicSleep();
            }
        }

        /// <summary>
        /// Заполнить список сущностей, которые могут находится в секторах чанка
        /// </summary>
        /// <param name="id">исключение ID сущности</param>
        public void FillInEntityBoundingBoxesFromSector(ListFast<EntityBase> list,
            AxisAlignedBB aabb, int minY, int maxY, int id)
        {
            EntityBase entity;
            MapEntity <EntityBase> entityMap;
            int count;
            for (int cy = minY; cy <= maxY; cy++)
            {
                entityMap = ListEntitiesSection[cy];
                count = entityMap.Count;
                for (int i = 0; i < count; i++)
                {
                    entity = entityMap.GetAt(i);
                    // TODO::2025-06-09 !entity.IsDead в колизии, продумать, когда сущность умерла, что с колизией делать?
                    if (entity.Id != id)// && !entity.IsDead)
                    {
                        if (entity.Size.IntersectsWith(aabb))
                        {
                            // Если пересекается вносим в список
                            list.Add(entity);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Пробудить сущности соприкасающих с блоком и проверить 
        /// соседние чанки если блок коснулся xz = 0 || 15
        /// </summary>
        private void _AwakenEntitiesTouching(BlockPos blockPos)
        {
            int x = blockPos.X;
            int y = blockPos.Y + 1;
            int z = blockPos.Z;
            int cy = y >> 4;

            if (cy >= NumberSections) cy = NumberSections - 1;

            EntityBase entity;
            for (int i = 0; i < ListEntitiesSection[cy].Count; i++)
            {
                entity = ListEntitiesSection[cy].GetAt(i);
                if (!entity.IsDead && entity.IsPhysicSleep())
                {
                    if (entity.Size.IntersectsWithBlock(x, y, z))
                    {
                        entity.AwakenPhysicSleep();
                    }
                }
            }

            int bx = x & 15;
            int bz = z & 15;

            // Нужен соседний чанк
            if (bx == 0)
            {
                World.GetChunk(CurrentChunkX - 1, CurrentChunkY)?.AwakenEntitiesTouching(x - 1, y, z);
                if (bz == 0)
                {
                    World.GetChunk(CurrentChunkX - 1, CurrentChunkY - 1)?.AwakenEntitiesTouching(x - 1, y, z - 1);
                }
                else if (bz == 15)
                {
                    World.GetChunk(CurrentChunkX - 1, CurrentChunkY + 1)?.AwakenEntitiesTouching(x - 1, y, z + 1);
                }
            }
            else if (bx == 15)
            {
                World.GetChunk(CurrentChunkX + 1, CurrentChunkY)?.AwakenEntitiesTouching(x + 1, y, z);
                if (bz == 0)
                {
                    World.GetChunk(CurrentChunkX + 1, CurrentChunkY - 1)?.AwakenEntitiesTouching(x + 1, y, z - 1);
                }
                else if (bz == 15)
                {
                    World.GetChunk(CurrentChunkX + 1, CurrentChunkY + 1)?.AwakenEntitiesTouching(x + 1, y, z + 1);
                }
            }
            else if (bz == 0)
            {
                World.GetChunk(CurrentChunkX, CurrentChunkY - 1)?.AwakenEntitiesTouching(x, y, z - 1);
            }
            else if (bz == 15)
            {
                World.GetChunk(CurrentChunkX, CurrentChunkY + 1)?.AwakenEntitiesTouching(x, y, z + 1);
            }
        }

        /// <summary>
        /// Пробудить сущности соприкасающих с блоком
        /// </summary>
        public void AwakenEntitiesTouching(int x, int y, int z)
        {
            int cy = y >> 4;
            if (cy >= NumberSections) cy = NumberSections - 1;
            int count = ListEntitiesSection[cy].Count;
            EntityBase entity;
            for (int i = 0; i < count; i++)
            {
                entity = ListEntitiesSection[cy].GetAt(i);
                if (!entity.IsDead && entity.IsPhysicSleep())
                {
                    if (entity.Size.IntersectsWithBlock(x, y, z))
                    {
                        entity.AwakenPhysicSleep();
                    }
                }
            }
        }

        #endregion

        #region Binary

        /// <summary>
        /// Внести данные в zip буфере
        /// </summary>
        public void SetBinaryZip(byte[] bufferIn, bool biom, uint flagsYAreas)
        {
            using (MemoryStream inStream = new MemoryStream(bufferIn))
            using (GZipStream bigStream = new GZipStream(inStream, CompressionMode.Decompress))
            {
                _bigStreamOut.Position = 0;
                bigStream.CopyTo(_bigStreamOut);
                _bigStreamOut.Position = 0;
                for (int sy = 0; sy < NumberSections; sy++)
                {
                    if ((flagsYAreas & 1 << sy) != 0)
                    {
                        ChunkStorage storage = StorageArrays[sy];
                        _bigStreamOut.Read(storage.Light, 0, 4096);

                        if (_bigStreamOut.ReadByte() == 0)
                        {
                            storage.ClearNotLight();
                        }
                        else
                        {
                            storage.Data = new int[4096];
                            byte[] b = new byte[16384];
                            _bigStreamOut.Read(b, 0, 16384);
                            Buffer.BlockCopy(b, 0, storage.Data, 0, 16384);
                            storage.UpCountBlock();
                        }
                    }
                }
                // биом
                if (biom)
                {
                    _bigStreamOut.Read(Biome, 0, 256);
                }
            }
            IsChunkPresent = true;
        }

        #endregion

        /// <summary>
        /// Пометить что надо перерендерить сетку чанка для клиента
        /// </summary>
        public virtual void ModifiedToRender(int y) { }

        public override string ToString() => CurrentChunkX + " : " + CurrentChunkY;

        /// <summary>
        /// Получить вектор позиции чанка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2i ToPosition() => new Vector2i(CurrentChunkX, CurrentChunkY);

        /// <summary>
        /// Получить вектор позиции региона
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2i ToRegion() => new Vector2i(CurrentChunkX >> 5, CurrentChunkY >> 5);
    }
}
