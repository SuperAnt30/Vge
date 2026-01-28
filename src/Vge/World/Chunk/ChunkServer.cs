using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Entity;
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
        private readonly WorldServer _worldServer;
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
            _worldServer = worldServer;
            _tickBlocksCache = _worldServer.TickBlocksCache;
        }

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
                    _tickBlocks[i].Set(timeTick + _worldServer.TickCounter, priority);
                    empty = false;
                    break;
                }
            }
            if (empty)
            {
                _tickBlocks.Add(new BlockTick(x, y, z, liquid, timeTick + _worldServer.TickCounter, priority));
            }
        }

        /// <summary>
        /// Отменить мгновенный тик блока
        /// </summary>
        protected override void _RemoveBlockTick(int x, int y, int z)
        {
            BlockTick tickBlock;
            int index = -1;
            for (int i = 0; i < _tickBlocks.Count; i++)
            {
                tickBlock = _tickBlocks[i];
                if (tickBlock.X == x && tickBlock.Y == y && tickBlock.Z == z)
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
                uint time = _worldServer.TickCounter;

                // Пробегаемся по всем тикам блоков и собираем которые надо выполнять
                for (int i = 0; i < count; i++)
                {
                    tickBlock = _tickBlocks[i];
                    if (tickBlock.ScheduledTick <= time && (_tickBlocksCache.Count < 8 || tickBlock.Priority))
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
                        block.UpdateTick(_worldServer, this,
                            new BlockPos(BlockX | tickBlock.X, tickBlock.Y, BlockZ | tickBlock.Z),
                            blockState, World.Rnd);
                    }
                }
            }
        }

        #endregion

        #region BlockEntity

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

        /// <summary>
        /// Пометка что чанк надо будет перезаписать
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Modified() => _isModified = true;
    }
}
