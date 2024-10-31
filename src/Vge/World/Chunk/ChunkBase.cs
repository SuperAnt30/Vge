
using System;
using System.Collections.Generic;
using Vge.Util;
using Vge.World.Block;

namespace Vge.World.Chunk
{
    /// <summary>
    /// Базовый объект чанка
    /// </summary>
    public class ChunkBase : IChunkPosition
    {
        /// <summary>
        /// Статический быстрый буфер для записи
        /// </summary>
        public readonly static ListFast<byte> BufferWrite = new ListFast<byte>(50000);
        /// <summary>
        /// Опции высот чанка
        /// </summary>
        public readonly ChunkSettings Settings = new ChunkSettings();

        /// <summary>
        /// Позиция X текущего чанка
        /// </summary>
        public readonly int X;
        /// <summary>
        /// Позиция Y текущего чанка
        /// </summary>
        public readonly int Y;
        /// <summary>
        /// Позиция X текущего чанка
        /// </summary>
        public int CurrentChunkX { get; private set; }
        /// <summary>
        /// Позиция Y текущего чанка
        /// </summary>
        public int CurrentChunkY { get; private set; }
        /// <summary>
        /// Сылка на объект мира
        /// </summary>
        public readonly WorldBase World;
        /// <summary>
        /// Данные чанка
        /// </summary>
        public readonly ChunkStorage[] StorageArrays;
        /// <summary>
        /// Совокупное количество тиков, которые якори провели в этом чанке 
        /// </summary>
        public uint InhabitedTakt { get; private set; }
        /// <summary>
        /// Количество секций в чанке
        /// </summary>
        public readonly byte NumberSections;

        #region Кольца 1-4

        /// <summary>
        /// Присутствует, этап загрузки или начальная генерация #1 1*1
        /// </summary>
        public bool IsChunkPresent { get; private set; }
        /// <summary>
        /// Было ли декорация чанка #2 3*3
        /// </summary>
        public bool IsPopulated { get; private set; }
        /// <summary>
        /// Было ли карта высот с небесным освещением #3 5*5
        /// </summary>
        public bool IsHeightMapSky { get; private set; }
        /// <summary>
        /// Было ли боковое небесное освещение и блочное освещение #4 7*7
        /// </summary>
        public bool IsSideLightSky { get; private set; }
        /// <summary>
        /// Готов ли чанк для отправки клиентам #5 9*9
        /// </summary>
        public bool IsSendChunk { get; private set; }

        #endregion

        public ChunkBase(WorldBase world, ChunkSettings settings, int chunkPosX, int chunkPosY)
        {
            World = world;
            X = CurrentChunkX = chunkPosX;
            Y = CurrentChunkY = chunkPosY;
            Settings = settings;
            NumberSections = Settings.NumberSections;
            StorageArrays = new ChunkStorage[NumberSections];
            for (int y = 0; y < NumberSections; y++)
            {
                StorageArrays[y] = new ChunkStorage(y << 4);
            }
        }

        /// <summary>
        /// Задать совокупное количество тактов, которые якоря провели в этом чанке 
        /// </summary>
        public void SetInhabitedTime(uint takt) => InhabitedTakt = takt;

        /// <summary>
        /// Выгрузили чанк
        /// </summary>
        public virtual void OnChunkUnload()
        {
            IsChunkPresent = false;
        }

        #region Кольца 1-4

        /// <summary>
        /// #1 1*1 Загрузка или генерация
        /// </summary>
        public void LoadingOrGen()
        {
            if (!IsChunkPresent)
            {
                IsChunkPresent = true;

                // Пробуем загрузить с файла
                //World.Filer.StartSection("Gen " + CurrentChunkX + "," + CurrentChunkY);

                // Временно льём тест
                for (int x = 0; x < 16; x++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        SetBlockState(x, 0, z, new BlockState(5));
                        for (int y = 1; y < 96; y++)
                        {
                            SetBlockState(x, y, z, new BlockState(2));
                        }
                    }
                }

                for (int y = 96; y < 128; y++)
                {
                    SetBlockState(7, y, 5, new BlockState(3));
                }
                
                SetBlockState(8, 96, 5, new BlockState(3));
                SetBlockState(8, 96, 6, new BlockState(5));
                SetBlockState(8, 96, 7, new BlockState(3));
                SetBlockState(8, 97, 7, new BlockState(4));

                SetBlockState(8, 98, 3, new BlockState(5));
                SetBlockState(8, 99, 3, new BlockState(5));
                SetBlockState(8, 100, 3, new BlockState(5));

                // Debug.Burden(.6f);
                //World.Filer.EndSectionLog();

                if (!World.IsRemote && World is WorldServer worldServer)
                {
                    int x, y;
                    ChunkBase chunk;
                    for (x = -1; x <= 1; x++)
                    {
                        for (y = -1; y <= 1; y++)
                        {
                            chunk = worldServer.ChunkPrServ.GetChunkPlus(CurrentChunkX + x, CurrentChunkY + y);
                            if (chunk != null && chunk.IsChunkPresent)
                            {
                                chunk._Populate(worldServer.ChunkPrServ);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// #2 3*3 Заполнение чанка населённостью
        /// </summary>
        private void _Populate(ChunkProviderServer provider)
        {
            if (!IsPopulated)
            {
                int x, y;
                // Если его в чанке нет проверяем чтоб у всех чанков близлежащих была генерация
                ChunkBase chunk;
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

                IsPopulated = true;
                // Пробуем загрузить с файла
                //World.Filer.StartSection("Pop " + CurrentChunkX + "," + CurrentChunkY);
                Debug.Burden(1.5f);
                //World.Filer.EndSectionLog();

                for (x = -1; x <= 1; x++)
                {
                    for (y = -1; y <= 1; y++)
                    {
                        chunk = provider.GetChunkPlus(CurrentChunkX + x, CurrentChunkY + y);
                        if (chunk != null && chunk.IsPopulated)
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
                ChunkBase chunk;
                for (x = -1; x <= 1; x++)
                {
                    for (y = -1; y <= 1; y++)
                    {
                        chunk = provider.GetChunkPlus(CurrentChunkX + x, CurrentChunkY + y);
                        if (chunk == null || !chunk.IsPopulated)
                        {
                            return;
                        }
                    }
                }

                IsHeightMapSky = true;
                // Пробуем загрузить с файла
                //World.Filer.StartSection("Hms " + CurrentChunkX + "," + CurrentChunkY);
                Debug.Burden(.1f);
                //World.Filer.EndSectionLog();

                for (x = -1; x <= 1; x++)
                {
                    for (y = -1; y <= 1; y++)
                    {
                        chunk = provider.GetChunkPlus(CurrentChunkX + x, CurrentChunkY + y);
                        if (chunk != null && chunk.IsHeightMapSky)
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
                ChunkBase chunk;
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

                IsSideLightSky = true;
                // Пробуем загрузить с файла
                //World.Filer.StartSection("Sls " + CurrentChunkX + "," + CurrentChunkY);
                Debug.Burden(.1f);
                //World.Filer.EndSectionLog();

                for (x = -1; x <= 1; x++)
                {
                    for (y = -1; y <= 1; y++)
                    {
                        chunk = provider.GetChunkPlus(CurrentChunkX + x, CurrentChunkY + y);
                        if (chunk != null && chunk.IsSideLightSky)
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
                ChunkBase chunk;
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
        /// Ставим блок в своём чанке, xz 0-15, y 0-max
        /// </summary>
        public void SetBlockState(int x, int y, int z, BlockState blockState)
        {
            int index = (y & 15) << 8 | z << 4 | x;
            ChunkStorage storage = StorageArrays[y >> 4];
            storage.SetData(index, blockState.Id, blockState.Met);
            storage.LightBlock[index] = blockState.LightBlock;
            storage.LightSky[index] = blockState.LightSky;
        }

        #region Binary

        /// <summary>
        /// Сгенерировать буфер байт для отправки чанка игроку
        /// </summary>
        public void GenBinary(bool biom, int flagsYAreas)
        {
            if (flagsYAreas == 0)
            {
                throw new Exception(Sr.GetString(Sr.OutOfRange, flagsYAreas));
            }

            BufferWrite.Clear();

            ushort data;
            int i;
            uint value;
            ChunkStorage chunkStorage;

            for (int y = 0; y < NumberSections; y++)
            {
                if ((flagsYAreas & 1 << y) != 0)
                {
                    chunkStorage = StorageArrays[y];
                    bool emptyData = chunkStorage.IsEmptyData();
                    BufferWrite.Add((byte)(emptyData ? 0 : 1));
                    for (i = 0; i < 4096; i++)
                    {
                        // TODO::2024-10-30 Буфер передавать копией, а не по единичке. Когда будет рендер блоков, чтоб протестить можно было
                        if (!emptyData)
                        {
                            data = chunkStorage.Data[i];
                            BufferWrite.Add((byte)(data & 0xFF));
                            BufferWrite.Add((byte)(data >> 8));
                        }

                        BufferWrite.Add((byte)(chunkStorage.LightBlock[i] << 4 | chunkStorage.LightSky[i] & 0xF));
                    }
                    if (!emptyData)
                    {
                        data = (ushort)chunkStorage.Metadata.Count;
                        BufferWrite.Add((byte)(data & 0xFF));
                        BufferWrite.Add((byte)(data >> 8));

                        foreach (KeyValuePair<ushort, uint> entry in chunkStorage.Metadata)
                        {
                            BufferWrite.Add((byte)(entry.Key >> 8));
                            BufferWrite.Add((byte)(entry.Key & 0xFF));
                            value = entry.Value;
                            BufferWrite.Add((byte)((value & 0xFF000000) >> 24));
                            BufferWrite.Add((byte)((value & 0xFF0000) >> 16));
                            BufferWrite.Add((byte)((value & 0xFF00) >> 8));
                            BufferWrite.Add((byte)(value & 0xFF));
                        }
                    }
                }
            }
            if (biom)
            {
                // добавляем данные биома
                for (i = 0; i < 256; i++)
                {
                    BufferWrite.Add(0);// (byte)chunk.biome[i];
                }
            }
        }

        /// <summary>
        /// Задать чанк байтами
        /// </summary>
        public void SetBinary(byte[] buffer, bool biom, int flagsYAreas)
        {
            if (buffer == null)
            {
                throw new Exception(Sr.EmptyArrayIsNotAllowed);
            }

            int sy, i, value;
            ushort countMet;
            int count = 0;
            byte light;
            ushort id, key;
            uint met;
            bool emptyData;
            for (sy = 0; sy < NumberSections; sy++)
            {
                if ((flagsYAreas & 1 << sy) != 0)
                {
                    ChunkStorage storage = StorageArrays[sy];
                    emptyData = buffer[count++] == 0;
                    if (emptyData)
                    {
                        storage.ClearNotLight();
                    }
                    for (i = 0; i < 4096; i++)
                    {
                        if (!emptyData)
                        {
                            value = buffer[count++] | buffer[count++] << 8;
                            id = (ushort)(value & 0xFFF);
                            storage.SetData(i, id, (ushort)(value >> 12));
                            if (!Blocks.BlocksMetadata[id]) storage.Metadata.Remove((ushort)i);
                        }
                        light = buffer[count++];
                        storage.LightBlock[i] = (byte)(light >> 4);
                        storage.LightSky[i] = (byte)(light & 0xF);
                    }
                    if (!emptyData)
                    {
                        countMet = (ushort)(buffer[count++] | buffer[count++] << 8);
                        for (i = 0; i < countMet; i++)
                        {
                            key = (ushort)(buffer[count++] << 8 | buffer[count++]);
                            met = (uint)(buffer[count++] << 24 | buffer[count++] << 16
                                | buffer[count++] << 8 | buffer[count++]);
                            if (storage.Metadata.ContainsKey(key))
                            {
                                storage.Metadata[key] = met;
                            }
                            else
                            {
                                storage.Metadata.Add(key, met);
                            }
                        }
                    }
                }
            }
            // биом
            if (biom)
            {
                for (i = 0; i < 256; i++)
                {
                    count++;
                    // biome[i] = (EnumBiome)buffer[count++];
                }
            }
            else
            {
                // Не первая закгрузка, помечаем что надо отрендерить весь столб
                for (int y = 0; y < NumberSections; y++)
                {
                    ModifiedToRender(y);
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
    }
}
