using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
        /// Исходящий буфер памяти для Zip
        /// </summary>
        private readonly static MemoryStream _bigStreamOut = new MemoryStream();

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
            KeyCash = Conv.ChunkXyToIndex(X, Y);
            Settings = settings;
            NumberSections = Settings.NumberSections;
            StorageArrays = new ChunkStorage[NumberSections];
            for (int index = 0; index < NumberSections; index++)
            {
                StorageArrays[index] = new ChunkStorage(KeyCash, index);
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
                // Пробуем загрузить с файла
                //World.Filer.StartSection("Gen " + CurrentChunkX + "," + CurrentChunkY);
                int h = NumberSections == 8 ? 63 : 127;
                // Временно льём тест

                ushort Stone, Cobblestone, Limestone, Granite, Glass, GlassRed, GlassGreen, GlassBlue, GlassPurple, FlowerClover;
                Stone = Cobblestone = Limestone = Granite = Glass = GlassRed = GlassGreen = GlassBlue = GlassPurple = FlowerClover = 0;


                for (ushort i = 0; i < Ce.Blocks.BlockAlias.Length; i++)
                {
                    switch(Ce.Blocks.BlockAlias[i])
                    {
                        case "Stone": Stone = i; break;
                        case "Cobblestone": Cobblestone = i; break;
                        case "Limestone": Limestone = i; break;
                        case "Granite": Granite = i; break;
                        case "Glass": Glass = i; break;
                        case "GlassRed": GlassRed = i; break;
                        case "GlassGreen": GlassGreen = i; break;
                        case "GlassBlue": GlassBlue = i; break;
                        case "GlassPurple": GlassPurple = i; break;
                        case "FlowerClover": FlowerClover = i; break;
                    }
                }

                for (int x = 0; x < 16; x++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        for (int y = 0; y < h; y++)
                        {
                            SetBlockState(x, y, z, new BlockState(Stone));
                        }
                    }
                }

                for (int y = h; y < h + 32; y++)
                {
                    SetBlockState(7, y, 5, new BlockState(Cobblestone));
                }

                SetBlockState(0, h, 0, new BlockState(Cobblestone));
                SetBlockState(0, h + 1, 0, new BlockState(Cobblestone));
                SetBlockState(0, h + 2, 0, new BlockState(Cobblestone));

                SetBlockState(10, h, 10, new BlockState(FlowerClover));
                SetBlockState(12, h, 12, new BlockState(FlowerClover));
                SetBlockState(15, h, 10, new BlockState(FlowerClover));
                SetBlockState(15, h, 12, new BlockState(FlowerClover));
                SetBlockState(0, h, 15, new BlockState(FlowerClover));
                SetBlockState(1, h, 15, new BlockState(FlowerClover));


                SetBlockState(15, h, 15, new BlockState(Limestone));
                SetBlockState(15, h + 1, 15, new BlockState(Limestone));

                SetBlockState(8, h, 5, new BlockState(Cobblestone));
                SetBlockState(8, h, 6, new BlockState(Granite));
                SetBlockState(8, h + 3, 7, new BlockState(Cobblestone));
                SetBlockState(8, h + 4, 7, new BlockState(Limestone));


                for (int y = h + 5; y < h + 10; y++)
                {
                    SetBlockState(8, y, 3, new BlockState(Granite));

                    SetBlockState(11, y, 5, new BlockState(Glass));
                    SetBlockState(8, y, 5, new BlockState(GlassRed));
                    SetBlockState(9, y, 12, new BlockState(GlassGreen));
                    SetBlockState(10, y, 13, new BlockState(GlassBlue));
                    SetBlockState(11, y, 15, new BlockState(GlassPurple));
                }

                SetBlockState(12, h + 5, 5, new BlockState(GlassRed));
                SetBlockState(12, h + 6, 5, new BlockState(GlassGreen));

                SetBlockState(11, h - 1, 4, new BlockState(0));
                SetBlockState(11, h - 1, 3, new BlockState(0));
                SetBlockState(12, h - 1, 4, new BlockState(0));
                SetBlockState(12, h - 1, 3, new BlockState(0));

                SetBlockState(13, h, 8, new BlockState(1));
                SetBlockState(12, h, 7, new BlockState(1, 1));
                SetBlockState(11, h, 7, new BlockState(1, 2));
                SetBlockState(11, h, 6, new BlockState(1, 3));
                SetBlockState(12, h, 6, new BlockState(1, 3));
                SetBlockState(10, h, 6, new BlockState(Granite));
                

                SetBlockState(12, h + 1, 9, new BlockState(1));
                SetBlockState(12, h + 2, 9, new BlockState(1, 0));
                SetBlockState(12, h + 3, 9, new BlockState(1, 1));
                SetBlockState(12, h + 4, 9, new BlockState(1, 1));
                SetBlockState(12, h + 5, 9, new BlockState(1, 1));
                SetBlockState(12, h + 6, 9, new BlockState(1, 2));
                SetBlockState(12, h + 7, 9, new BlockState(1, 2));
                SetBlockState(12, h + 8, 9, new BlockState(1, 2));
                SetBlockState(12, h + 9, 9, new BlockState(1, 3));
                SetBlockState(12, h + 10, 9, new BlockState(1, 3));
                SetBlockState(12, h + 11, 9, new BlockState(1, 3));
                SetBlockState(12, h, 9, new BlockState(1));
                SetBlockState(11, h, 9, new BlockState(1));
                SetBlockState(11, h, 10, new BlockState(1));
                SetBlockState(12, h, 10, new BlockState(1));


                // Debug.Burden(.6f);
                //World.Filer.EndSectionLog();
                IsChunkPresent = true;

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

                
                // Пробуем загрузить с файла
                //World.Filer.StartSection("Pop " + CurrentChunkX + "," + CurrentChunkY);
                Debug.Burden(1.5f);
                //World.Filer.EndSectionLog();
                IsPopulated = true;
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

                
                // Пробуем загрузить с файла
                //World.Filer.StartSection("Hms " + CurrentChunkX + "," + CurrentChunkY);
                Debug.Burden(.1f);
                //World.Filer.EndSectionLog();
                IsHeightMapSky = true;
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
        /// Внести данные в zip буфере
        /// </summary>
        public void SetBinaryZip(byte[] bufferIn, bool biom, int flagsYAreas)
        {
            using (MemoryStream inStream = new MemoryStream(bufferIn))
            using (GZipStream bigStream = new GZipStream(inStream, CompressionMode.Decompress))
            {
                _bigStreamOut.Position = 0;
                bigStream.CopyTo(_bigStreamOut);
                _bigStreamOut.Position = 0;
                int sy, i;
                ushort countMet;
                int count = 0;
                ushort key;
                uint met;
                for (sy = 0; sy < NumberSections; sy++)
                {
                    if ((flagsYAreas & 1 << sy) != 0)
                    {
                        ChunkStorage storage = StorageArrays[sy];
                        _bigStreamOut.Read(storage.LightBlock, 0, 4096);
                        _bigStreamOut.Read(storage.LightSky, 0, 4096);

                        if (_bigStreamOut.ReadByte() == 0)
                        {
                            storage.ClearNotLight();
                        }
                        else
                        {
                            storage.Data = new ushort[4096];
                            byte[] b = new byte[8192];
                            _bigStreamOut.Read(b, 0, 8192);
                            Buffer.BlockCopy(b, 0, storage.Data, 0, 8192);
                            storage.UpCountBlock();

                            storage.Metadata.Clear();
                            countMet = (ushort)(_bigStreamOut.ReadByte() << 8 | _bigStreamOut.ReadByte());
                            for (i = 0; i < countMet; i++)
                            {
                                key = (ushort)(_bigStreamOut.ReadByte() << 8 | _bigStreamOut.ReadByte());
                                met = (uint)(_bigStreamOut.ReadByte() << 24 | _bigStreamOut.ReadByte() << 16
                                    | _bigStreamOut.ReadByte() << 8 | _bigStreamOut.ReadByte());
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
            }
            IsChunkPresent = true;
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
            ushort id, key;
            uint met;
            for (sy = 0; sy < NumberSections; sy++)
            {
                if ((flagsYAreas & 1 << sy) != 0)
                {
                    ChunkStorage storage = StorageArrays[sy];
                    for (i = 0; i < 4096; i++)
                    {
                        storage.LightBlock[i] = buffer[count++];
                    }
                    for (i = 0; i < 4096; i++)
                    {
                        storage.LightSky[i] = buffer[count++];
                    }

                    if (buffer[count++] == 0)
                    {
                        storage.ClearNotLight();
                    }
                    else
                    {
                        for (i = 0; i < 4096; i++)
                        {
                            value = buffer[count++] | buffer[count++] << 8;
                            id = (ushort)(value & 0xFFF);
                            storage.SetData(i, id, (ushort)(value >> 12));
                            if (!Ce.Blocks.BlocksMetadata[id]) storage.Metadata.Remove((ushort)i);
                        }
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
