using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Entity;
using Vge.NBT;
using Vge.Util;
using Vge.World.Block;
using Vge.World.BlockEntity;

namespace Vge.World.Chunk
{
    /// <summary>
    /// Объект чанка для сервера
    /// </summary>
    public class ChunkServer : ChunkBase
    {
        /// <summary>
        /// Карта высот по чанку, рельефа при генерации z << 4 | x
        /// </summary>
        public readonly ushort[] HeightMapGen = new ushort[256];

        /// <summary>
        /// Объект серверного мира
        /// </summary>
        public readonly WorldServer WorldServ;

        /// <summary>
        /// Совокупное количество тиков, которые якори провели в этом чанке 
        /// </summary>
        public uint InhabitedTick { get; private set; }

        /// <summary>
        /// Список BlockTick блоков которые должны мгновенно тикать
        /// </summary>
        private readonly ListMessy<BlockTick> _tickBlocks = new ListMessy<BlockTick>();
        /// <summary>
        /// Кешовы Список BlockTick блоков которые должны мгновенно тикать,
        /// нужен чтоб непересоздавать его в каждом чанке в каждом тике.
        /// Один на весь мир
        /// </summary>
        private readonly ListMessy<BlockTick> _tickBlocksCache;

        /// <summary>
        /// Карта сущностей блока y << 8 | z << 4 | x
        /// </summary>
        private readonly Dictionary<int, BlockEntityBase> _mapBlocksEntity = new Dictionary<int, BlockEntityBase>();

        /// <summary>
        /// Установите значение true, если чанк был изменен и нуждается в внутреннем обновлении. Для сохранения
        /// </summary>
        private bool _isModified;

        public ChunkServer(WorldServer worldServer, ChunkSettings settings, int chunkPosX, int chunkPosY)
            : base(worldServer, settings, chunkPosX, chunkPosY)
        {
            WorldServ = worldServer;
            _tickBlocksCache = WorldServ.TickBlocksCache;
        }
        
        /// <summary>
        /// Задать совокупное количество тактов, которые якоря провели в этом чанке 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetInhabitedTick(uint tick) => InhabitedTick = tick;

        #region Кольца 1-4

        /// <summary>
        /// Готова начальная генерация или загрузка, приступаем к следующему этапу Populate
        /// </summary>
        public void ChunkPresent()
        {
            IsChunkPresent = true;

            if (!World.IsRemote && World is WorldServer worldServer)
            {
                int x, y;
                ChunkServer chunk;
                for (x = -1; x <= 1; x++)
                {
                    for (y = -1; y <= 1; y++)
                    {
                        chunk = worldServer.ChunkPrServ.GetChunkPlus(CurrentChunkX + x, CurrentChunkY + y);
                        if (chunk != null && chunk.IsChunkPresent)
                        {
                            chunk._Decoration(worldServer.ChunkPrServ);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// #2 3*3 Заполнение чанка декорацией
        /// </summary>
        private void _Decoration(ChunkProviderServer provider)
        {
            if (!IsDecorated)
            {
                int x, y;
                // Если его в чанке нет проверяем чтоб у всех чанков близлежащих была генерация
                ChunkServer chunk;
                for (x = -1; x <= 1; x++)
                {
                    for (y = -1; y <= 1; y++)
                    {
                        chunk = provider.GetChunkPlus(CurrentChunkX + x, CurrentChunkY + y);
                        if (chunk == null || !chunk.IsChunkPresent)
                        {
                            return;
                        }
                    }
                }

                // Decoration
                //World.Filer.StartSection("GenDec " + CurrentChunkX + "," + CurrentChunkY);
                provider.ChunkGenerate.Decoration(provider, this);
                //World.Filer.EndSectionLog();
                IsDecorated = true;
                for (x = -1; x <= 1; x++)
                {
                    for (y = -1; y <= 1; y++)
                    {
                        chunk = provider.GetChunkPlus(CurrentChunkX + x, CurrentChunkY + y);
                        if (/*chunk != null && */chunk.IsDecorated)
                        {
                            chunk._HeightMapSky(provider);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// #3 5*5 Карта высот с вертикальным небесным освещением
        /// </summary>
        private void _HeightMapSky(ChunkProviderServer provider)
        {
            if (!IsHeightMapSky)
            {
                int x, y;
                ChunkServer chunk;
                for (x = -1; x <= 1; x++)
                {
                    for (y = -1; y <= 1; y++)
                    {
                        chunk = provider.GetChunkPlus(CurrentChunkX + x, CurrentChunkY + y);
                        if (chunk == null || !chunk.IsDecorated)
                        {
                            return;
                        }
                    }
                }


                // Пробуем загрузить с файла
                //World.Filer.StartSection("Hms " + CurrentChunkX + "," + CurrentChunkY);
                if (World.Settings.HasNoSky)
                {
                    Light.GenerateHeightMap();
                }
                else
                {
                    Light.GenerateHeightMapSky(); // 0.09 - 0.13 мс
                }
                //World.Filer.EndSectionLog();
                IsHeightMapSky = true;
                for (x = -1; x <= 1; x++)
                {
                    for (y = -1; y <= 1; y++)
                    {
                        chunk = provider.GetChunkPlus(CurrentChunkX + x, CurrentChunkY + y);
                        if (/*chunk != null && */chunk.IsHeightMapSky)
                        {
                            chunk._SideLightSky(provider);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// #4 7*7 Боковое небесное освещение и блочное освещение
        /// </summary>
        private void _SideLightSky(ChunkProviderServer provider)
        {
            if (!IsSideLightSky)
            {
                int x, y;
                ChunkServer chunk;
                for (x = -1; x <= 1; x++)
                {
                    for (y = -1; y <= 1; y++)
                    {
                        chunk = provider.GetChunkPlus(CurrentChunkX + x, CurrentChunkY + y);
                        if (chunk == null || !chunk.IsHeightMapSky)
                        {
                            return;
                        }
                    }
                }

                // Боковое небесное освещение и блочное освещение
                //World.Filer.StartSection("Sls " + CurrentChunkX + "," + CurrentChunkY);
                Light.StartRecheckGaps(World.Settings.HasNoSky); // 0.12 - 0.2 мс
                //World.Filer.EndSectionLog();
                IsSideLightSky = true;

                for (x = -1; x <= 1; x++)
                {
                    for (y = -1; y <= 1; y++)
                    {
                        chunk = provider.GetChunkPlus(CurrentChunkX + x, CurrentChunkY + y);
                        if (/*chunk != null && */chunk.IsSideLightSky)
                        {
                            chunk._SendChunk(provider);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// #5 9*9 Возможность отправлять чанк клиентам
        /// </summary>
        private void _SendChunk(ChunkProviderServer provider)
        {
            if (!IsSendChunk)
            {
                int x, y;
                ChunkServer chunk;
                for (x = -1; x <= 1; x++)
                {
                    for (y = -1; y <= 1; y++)
                    {
                        chunk = provider.GetChunkPlus(CurrentChunkX + x, CurrentChunkY + y);
                        if (chunk == null || !chunk.IsSideLightSky)
                        {
                            return;
                        }
                    }
                }
                IsSendChunk = true;
            }
        }

        #endregion

        /// <summary>
        /// Перегенерация всего чанка, кроме сущностейи бокового небесного освещения, пока!
        /// </summary>
        public void Regen()
        {
            // Сначало надо удалить все тайлы
            _mapBlocksEntity.Clear();
            for (int index = 0; index < NumberSections; index++)
            {
                StorageArrays[index] = new ChunkStorage(KeyCash, index);
            }
            Light.Clear();

            WorldServ.ChunkPrServ.ChunkGenerate.Relief(this);
            IsDecorated = false;
            IsHeightMapSky = false;
            IsSideLightSky = false;
            IsSendChunk = false;
            IsLoaded = false;
            ChunkPresent();

            Modified();
            for (int index = 0; index < NumberSections; index++)
            {
                WorldServ.Fragment.FlagChunkForUpdate(X, index, Y);
            }

            // Проверка рядом освещения
            for (int i = 0; i < 3; i++) // Не всегода с первого раза всё чистит по свету
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        WorldServ.Light.FixChunkLightBlock(X + x, Y + y);
                    }
                }
            }
        }

        /// <summary>
        /// Сгенерировать копию высот для популяции
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitHeightMapGen()
            => Buffer.BlockCopy(Light.HeightMap, 0, HeightMapGen, 0, 512);


        #region Block

        /// <summary>
        /// Количество тикающих блоков в чанке
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetTickBlockCount() => _tickBlocks.Count;

        /// <summary>
        /// Задать тик блока с локальной позицие и время через сколько тактов надо тикнуть
        /// </summary>
        public void SetBlockTick(int x, int y, int z, bool liquid, uint timeTick, bool priority = false)
        {
            BlockTick tickBlock;
            bool empty = true;
            for (int i = 0; i < _tickBlocks.Count; i++)
            {
                tickBlock = _tickBlocks[i];
                if (tickBlock.X == x && tickBlock.Y == y && tickBlock.Z == z && tickBlock.Liquid == liquid)
                {
                    _tickBlocks[i].Set(timeTick + WorldServ.TickCounter, priority);
                    empty = false;
                    break;
                }
            }
            if (empty)
            {
                _tickBlocks.Add(new BlockTick(x, y, z, liquid, timeTick + WorldServ.TickCounter, priority));
            }
        }

        /// <summary>
        /// Отменить мгновенный тик блока
        /// </summary>
        protected override void _RemoveBlockTick(int x, int y, int z, bool liquid)
        {
            BlockTick tickBlock;
            int index = -1;
            for (int i = 0; i < _tickBlocks.Count; i++)
            {
                tickBlock = _tickBlocks[i];
                if (tickBlock.X == x && tickBlock.Y == y && tickBlock.Z == z && tickBlock.Liquid == liquid)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1) _tickBlocks.RemoveAt(index);
        }

        #endregion

        #region Update

        /// <summary>
        /// Обновление в такте активных чанков, только на сервере
        /// </summary>
        public void UpdateServer()
        {
            int count = _tickBlocks.Count;
            if (count > 0)
            {
                BlockTick tickBlock;
                _tickBlocksCache.Clear();
                uint time = WorldServ.TickCounter;

                // Пробегаемся по всем тикам блоков и собираем которые надо выполнять
                for (int i = 0; i < count; i++)
                {
                    tickBlock = _tickBlocks[i];
                    if (tickBlock.ScheduledTick <= time 
                        && (_tickBlocksCache.Count < Ce.CountTickBlockChunk || tickBlock.Priority))
                    {
                        tickBlock.Index = i;
                        _tickBlocksCache.Add(tickBlock);
                    }
                }
                count = _tickBlocksCache.Count;
                if (count > 0)
                {
                    BlockState blockState;
                    count--;
                    // Удаляем которые надо выполнять и выполняем их
                    for (int i = count; i >= 0; i--)
                    {
                        tickBlock = _tickBlocksCache[i];
                        _tickBlocks.RemoveAt(tickBlock.Index);

                        blockState = GetBlockStateNotCheck(tickBlock.X, tickBlock.Y, tickBlock.Z);
                        BlockBase block = tickBlock.Liquid ? Ce.Blocks.GetAddLiquid(blockState.Met)
                            : blockState.GetBlock();
                        block.UpdateTick(WorldServ, this,
                            new BlockPos(BlockX | tickBlock.X, tickBlock.Y, BlockZ | tickBlock.Z),
                            blockState, World.Rnd);
                    }
                }
            }
        }

        #endregion

        #region BlockEntity

        /// <summary>
        /// Добавить в список все блок сущности этого чанка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRangeBlockEntity(List<BlockEntityBase> blocksEntity)
            => blocksEntity.AddRange(_mapBlocksEntity.Values);

        /// <summary>
        /// Количество блоков сущности в чанке
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBlockEntityCount() => _mapBlocksEntity.Count;

        /// <summary>
        /// Получить блок сущности по глобальным координатам блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockEntityBase GetBlockEntity(BlockPos pos)
            => GetBlockEntity(pos.X & 15, pos.Y, pos.Z & 15);

        /// <summary>
        /// Получить блок сущности по локальным координатам xz 0..15 y 0..127
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockEntityBase GetBlockEntity(int x, int y, int z)
        {
            int key = y << 8 | z << 4 | x;
            return _mapBlocksEntity.ContainsKey(key) ? _mapBlocksEntity[key] : null;
        }

        /// <summary>
        /// Добавить блок сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlockEntity(BlockEntityBase blockEntity)
        {
            BlockPos pos = blockEntity.Position;
            int key = pos.Y << 8 | (pos.Z & 15) << 4 | (pos.X & 15);
            if (_mapBlocksEntity.ContainsKey(key))
            {
                _mapBlocksEntity[key] = blockEntity;
            }
            else
            {
                _mapBlocksEntity.Add(key, blockEntity);
            }
        }

        /// <summary>
        /// Удалить блок сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveBlockEntity(BlockPos pos)
            => RemoveBlockEntity(pos.X & 15, pos.Y, pos.Z & 15);
        /// <summary>
        /// Удалить блок сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveBlockEntity(int x, int y, int z) 
            => _mapBlocksEntity.Remove(y << 8 | z << 4 | x);

        #endregion

        #region Save

        /// <summary>
        /// Пометка что чанк надо будет перезаписать
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Modified() => _isModified = true;

        /// <summary>
        /// Сохранить чанк в файл региона
        /// </summary>
        public bool SaveFileChunk()
        {
            if (IsSendChunk && (_hasEntities || _isModified))
            {
                TagCompound nbt = new TagCompound();
                _WriteChunkToNBT(nbt);
                WorldServ.Regions.Get(X, Y).WriteChunk(nbt, X, Y);
                _isModified = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Прочесть чанк в файл региона
        /// </summary>
        public void LoadFileChunk()
        {
            RegionFile region = WorldServ.Regions.Get(X, Y);
            if (region != null)
            {
                TagCompound nbt = region.ReadChunk(X, Y);
                if (nbt != null)
                {
                    try
                    {
                        _ReadChunkFromNBT(nbt);
                        IsChunkPresent = true;
                        IsDecorated = true;
                        IsHeightMapSky = true;
                        IsSideLightSky = true;
                        IsSendChunk = true;
                        IsLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        WorldServ.Server.Log.Error(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Сохранить чанк
        /// </summary>
        private void _WriteChunkToNBT(TagCompound nbt)
        {
            int ys, i;
            uint tick = WorldServ.TickCounter;
            nbt.SetShort("Version", 1);
            nbt.SetInt("X", X);
            nbt.SetInt("Z", Y);
            nbt.SetLong("LastUpdate", tick);
            nbt.SetLong("InhabitedTick", InhabitedTick);
            nbt.SetShort("HeightMapMax", (short)Light.HeightMapMax);
            nbt.SetByteArray("HeightMap", TagByteArray.ConvToByte(Light.HeightMap));
            nbt.SetByteArray("HeightMapGen", TagByteArray.ConvToByte(HeightMapGen));
            nbt.SetByteArray("Biomes", Biome);

            // ---Sections

            TagList tagListSections = new TagList();
            for (ys = 0; ys < NumberSections; ys++)
            {
                StorageArrays[ys].WriteDataToNBT(tagListSections);
            }
            nbt.SetTag("Sections", tagListSections);

            // ---BlockTicks

            if (_tickBlocks.Count > 0)
            {
                TagList tagListTickBlocks = new TagList();
                BlockTick tickBlock;
                for (i = 0; i < _tickBlocks.Count; i++)
                {
                    tickBlock = _tickBlocks[i];
                    TagCompound tagCompound = new TagCompound();
                    tagCompound.SetByte("X", tickBlock.X);
                    tagCompound.SetShort("Y", tickBlock.Y);
                    tagCompound.SetByte("Z", tickBlock.Z);
                    tagCompound.SetBool("Liquid", tickBlock.Liquid);
                    tagCompound.SetInt("Tick", (int)(tickBlock.ScheduledTick - tick));
                    tagCompound.SetBool("P", tickBlock.Priority);
                    tagListTickBlocks.AppendTag(tagCompound);
                }
                nbt.SetTag("BlockTicks", tagListTickBlocks);
            }

            // ---BlocksEntity

            if (_mapBlocksEntity.Count > 0)
            {
                TagList tagListBlocksEntity = new TagList();
                foreach (KeyValuePair<int, BlockEntityBase> keyTileEntity in _mapBlocksEntity)
                {
                    TagCompound tagCompound = new TagCompound();
                    keyTileEntity.Value.WriteToNBT(tagCompound);
                    tagListBlocksEntity.AppendTag(tagCompound);
                }
                if (tagListBlocksEntity.TagCount() > 0)
                {
                    nbt.SetTag("BlocksEntity", tagListBlocksEntity);
                }
            }

            // ---Entities

            // проверяю есть ли сущность, если есть то true;
            _hasEntities = false;
            int count = ListEntities.Count;
            if (count > 0)
            {
                TagList tagListEntities = new TagList();
                for (i = 0; i < count; i++)
                {
                    TagCompound tagCompound = new TagCompound();
                    if (ListEntities.GetAt(i).WriteToNBT(tagCompound)) 
                    {
                        _hasEntities = true;
                        tagListEntities.AppendTag(tagCompound);
                    }
                }
                if (tagListEntities.TagCount() > 0)
                {
                    nbt.SetTag("Entities", tagListEntities);
                }
            }
        }

        /// <summary>
        /// Прочесть чанка
        /// </summary>
        private void _ReadChunkFromNBT(TagCompound nbt)
        {
            uint tick = WorldServ.TickCounter;
            uint timeDelta = tick - (uint)nbt.GetLong("LastUpdate");
            InhabitedTick = (uint)nbt.GetLong("InhabitedTick");
            Light.HeightMapMax = nbt.GetShort("HeightMapMax");
            TagByteArray.ConvToUShort(nbt.GetByteArray("HeightMap"), Light.HeightMap);
            TagByteArray.ConvToUShort(nbt.GetByteArray("HeightMapGen"), HeightMapGen);
            Buffer.BlockCopy(Biome, 0, nbt.GetByteArray("Biomes"), 0, Biome.Length);

            // ---Sections

            TagList tagListSections = nbt.GetTagList("Sections", 10);
            int count = tagListSections.TagCount();
            if (tagListSections.GetTagType() == 10)
            {
                int y;
                bool[] flag = new bool[NumberSections];
                for (int i = 0; i < count; i++)
                {
                    TagCompound tagCompound = tagListSections.Get(i) as TagCompound;
                    y = tagCompound.GetByte("Y");
                    flag[y] = true;
                    StorageArrays[y].ReadDataFromNBT(tagCompound);
                }
                if (count < NumberSections)
                {
                    TagCompound tagCompound = new TagCompound();
                    for (int i = 0; i < NumberSections; i++)
                    {
                        if (!flag[i])
                        {
                            StorageArrays[i].ReadDataFromNBT(tagCompound);
                        }
                    }
                }
            }

            // ---BlockTicks

            _tickBlocks.Clear();
            TagList tagListTickBlocks = nbt.GetTagList("BlockTicks", 10);
            count = tagListTickBlocks.TagCount();
            if (tagListTickBlocks.GetTagType() == 10)
            {
                for (int i = 0; i < count; i++)
                {
                    TagCompound tagCompound = tagListTickBlocks.Get(i) as TagCompound;
                    _tickBlocks.Add(new BlockTick(
                        tagCompound.GetByte("X"), tagCompound.GetShort("Y"), tagCompound.GetByte("Z"),
                        tagCompound.GetBool("Liquid"), (uint)tagCompound.GetInt("Tick") + tick,
                        tagCompound.GetBool("P")));
                }
            }

            // ---BlocksEntity

            _mapBlocksEntity.Clear();
            TagList tagListTileEntities = nbt.GetTagList("BlocksEntity", 10);
            count = tagListTileEntities.TagCount();
            if (tagListTileEntities.GetTagType() == 10)
            {
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        TagCompound tagCompound = tagListTileEntities.Get(i) as TagCompound;
                        BlockEntityBase blockEntity = Ce.BlocksEntity.CreateEntityServer((ushort)tagCompound.GetShort("Id"));
                        blockEntity.ReadFromNBT(tagCompound, this);
                        SetBlockEntity(blockEntity);
                    }
                    catch (Exception ex)
                    {
                        WorldServ.Server.Log.Error(ex);
                    }
                }
            }

            // ---Entities

            // Скорее всего будет добавление сущности AddEntity, а там hasEntities = true;
            TagList tagListEntities = nbt.GetTagList("Entities", 10);
            count = tagListEntities.TagCount();
            if (tagListEntities.GetTagType() == 10)
            {
                _hasEntities = true;
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        TagCompound tagCompound = tagListEntities.Get(i) as TagCompound;
                        EntityBase entity = Ce.Entities.CreateEntityServer((ushort)tagCompound.GetShort("Id"), WorldServ);
                        entity.ReadFromNBT(tagCompound);
                        World.SpawnEntityInWorld(entity);
                    }
                    catch (Exception ex)
                    {
                        WorldServ.Server.Log.Error(ex);
                    }
                }
            }
        }

        #endregion
    }
}
