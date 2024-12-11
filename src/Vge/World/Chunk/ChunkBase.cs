using System;
using System.IO;
using System.IO.Compression;
using Vge.Util;
using Vge.World.Block;
using Vge.World.Light;

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
        /// Количество секций в чанке (old COUNT_HEIGHT)
        /// </summary>
        public readonly byte NumberSections;
        /// <summary>
        /// Объект работы с освещением
        /// </summary>
        public readonly ChunkLight Light;

        /// <summary>
        /// Установите значение true, если чанк был изменен и нуждается в внутреннем обновлении. Для сохранения
        /// </summary>
        private bool _isModified;

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
            KeyCash = Conv.ChunkXyToIndex(X, Y);
            Settings = settings;
            NumberSections = Settings.NumberSections;
            StorageArrays = new ChunkStorage[NumberSections];
            for (int index = 0; index < NumberSections; index++)
            {
                StorageArrays[index] = new ChunkStorage(KeyCash, index);
            }
            Light = new ChunkLight(this);
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
                //int h = NumberSections == 8 ? 63 : 95;
                int h = NumberSections == 8 ? 47 : 95;
                // Временно льём тест

                ushort Stone, Cobblestone, Limestone, Granite, Glass, GlassRed, GlassGreen, 
                    GlassBlue, GlassPurple, FlowerClover, Water, Lava, Brol;
                Stone = Cobblestone = Limestone = Granite = Glass = GlassRed = GlassGreen = 
                    GlassBlue = GlassPurple = FlowerClover = Water = Lava = Brol = 0;


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
                        case "Water": Water = i; break;
                        case "Lava": Lava = i; break;
                        case "Brol": Brol = i; break;
                    }
                }

                for (int x = 0; x < 16; x++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        for (int y = 0; y < h; y++)
                        {
                            SetBlockStateD(x, y, z, new BlockState(Stone));
                        }
                    }
                }

                if (X == 0 && Y == 0)
                {
                    for (int y = h - 16; y < h - 4; y++)
                    {
                        SetBlockStateD(0, y, 0, new BlockState(0));
                        SetBlockStateD(1, y, 0, new BlockState(0));
                    }
                    for (int y = h + 16; y < h + 20; y++)
                    {
                        SetBlockStateD(15, y, 0, new BlockState(Water));
                    }
                }
                if (X == 0 && Y == -1)
                {
                    for (int y = h - 16; y < h - 4; y++)
                    {
                        SetBlockStateD(1, y, 15, new BlockState(Water));
                    }
                }
                if (X == 0 && Y == 0)
                {
                    for (int y = h - 16; y < h - 4; y++)
                    {
                        SetBlockStateD(1, y, 1, new BlockState(Water));
                    }
                }

                for (int x = 2; x < 8; x++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        //for (int y = h - 42; y < h; y++)
                        for (int y = h - 12; y < h; y++)
                        {
                            //SetBlockState(x, y, z, new BlockState(GlassBlue));
                            //SetBlockState(x, y, z, new BlockState(0));
                            SetBlockStateD(x, y, z, new BlockState(Water));
                        }

                        
                    }
                }

                for (int x = 9; x < 16; x++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        for (int y = h - 12; y < h - 2; y++)
                        {
                            SetBlockStateD(x, y, z, new BlockState(0));
                        }
                    }
                }

                SetBlockStateD(5, h, 5, new BlockState(GlassRed));
                SetBlockStateD(5, h - 1, 4, new BlockState(GlassRed));
                SetBlockStateD(5, h - 2, 3, new BlockState(GlassRed));

                SetBlockStateD(4, h, 7, new BlockState(Water));

                SetBlockStateD(1, h + 2, 0, new BlockState(Water));
                SetBlockStateD(1, h + 1, 0, new BlockState(Water, 13));
                SetBlockStateD(1, h, 0, new BlockState(Water, 11));
                SetBlockStateD(2, h, 0, new BlockState(Water, 9));

                SetBlockStateD(8, h - 1, 15, new BlockState(Water));
                SetBlockStateD(8, h - 1, 0, new BlockState(Water));
                for (int x = 0; x < 7; x++)
                {
                    SetBlockStateD(x + 9, h - 1, 15, new BlockState(Water, (byte)(13 - x * 2)));
                    if (x < 6) SetBlockStateD(x + 9, h - 1, 0, new BlockState(Water, (byte)(13 - x * 2)));

                    if (x < 6) SetBlockStateD(x + 8, h - 1, 14, new BlockState(Lava, (byte)(13 - x * 3)));
                }
                SetBlockStateD(14, h - 1, 1, new BlockState(Water, 1));
                SetBlockStateD(13, h - 1, 14, new BlockState(Water, 2));
                SetBlockStateD(13, h - 1, 13, new BlockState(Water, 1));

                SetBlockStateD(7, h - 1, 14, new BlockState(Lava));
                SetBlockStateD(7, h, 15, new BlockState(Lava));
                SetBlockStateD(7, h + 1, 15, new BlockState(Lava));
                SetBlockStateD(6, h + 16, 5, new BlockState(Lava));

                for (int x = 1; x < 8; x++)
                {
                    for (int z = 6; z < 11; z++)
                    {
                        SetBlockStateD(x, h, z, new BlockState(Granite));
                        //SetBlockState(x, h - 1, z, new BlockState(Lava));
                        SetBlockStateD(x, h + 9, z, new BlockState(Limestone));
                    }
                }

                //SetBlockStateD(4, h + 1, 8, new BlockState(Brol));
                //Light.SetLightBlock(4, h + 1, 8);

                //for (int x = 0; x < 8; x++)
                //{
                //    for (int z = 2; z < 11; z++)
                //    {
                //      //  SetBlockStateD(x, h + 6, z, new BlockState(Limestone));
                //        SetBlockStateD(x, h + 16, z, new BlockState(Lava));
                //        SetBlockStateD(x, h + 17, z, new BlockState(Lava));
                //        SetBlockStateD(x, h + 18, z, new BlockState(Lava));
                //        SetBlockStateD(x, h + 19, z, new BlockState(Lava));
                //    }
                //}


                for (int y = h; y < h + 32; y++)
                {
                    SetBlockStateD(7, y, 5, new BlockState(Cobblestone));
                    if (y > h + 16)
                    {
                        SetBlockStateD(6, y, 5, new BlockState(Water));
                    }

                }

                SetBlockStateD(0, h, 0, new BlockState(Cobblestone));
                SetBlockStateD(0, h + 1, 0, new BlockState(Cobblestone));
                SetBlockStateD(0, h + 2, 0, new BlockState(Cobblestone));

                SetBlockStateD(10, h, 10, new BlockState(FlowerClover));
                SetBlockStateD(12, h, 12, new BlockState(FlowerClover));
                SetBlockStateD(15, h, 10, new BlockState(FlowerClover));
                SetBlockStateD(15, h, 12, new BlockState(FlowerClover));
                SetBlockStateD(0, h, 15, new BlockState(FlowerClover));
                SetBlockStateD(1, h, 15, new BlockState(FlowerClover));



                

                SetBlockStateD(15, h, 15, new BlockState(Limestone));
                SetBlockStateD(15, h + 1, 15, new BlockState(Limestone));

                SetBlockStateD(8, h, 5, new BlockState(Cobblestone));
                SetBlockStateD(8, h, 6, new BlockState(Granite));
                SetBlockStateD(8, h + 3, 7, new BlockState(Cobblestone));
                SetBlockStateD(8, h + 4, 7, new BlockState(Limestone));


                for (int y = h + 5; y < h + 10; y++)
                {
                    SetBlockStateD(8, y, 3, new BlockState(Granite));

                    SetBlockStateD(11, y, 5, new BlockState(Glass));
                    SetBlockStateD(8, y, 5, new BlockState(GlassRed));
                    SetBlockStateD(9, y, 12, new BlockState(GlassGreen));
                    SetBlockStateD(10, y, 13, new BlockState(GlassBlue));
                    SetBlockStateD(11, y, 15, new BlockState(GlassPurple));
                }

                SetBlockStateD(12, h + 5, 5, new BlockState(GlassRed));
                SetBlockStateD(12, h + 6, 5, new BlockState(GlassGreen));

                SetBlockStateD(11, h - 1, 4, new BlockState(1));
                SetBlockStateD(11, h - 1, 3, new BlockState(1));
                SetBlockStateD(12, h - 1, 4, new BlockState(1));
                SetBlockStateD(12, h - 1, 3, new BlockState(1));
                SetBlockStateD(11, h - 2, 4, new BlockState(0));
                SetBlockStateD(11, h - 2, 3, new BlockState(0));
                SetBlockStateD(12, h - 2, 4, new BlockState(0));
                SetBlockStateD(12, h - 2, 3, new BlockState(0));

                SetBlockStateD(13, h, 8, new BlockState(1));
                SetBlockStateD(12, h, 7, new BlockState(1, 1));
                SetBlockStateD(11, h, 7, new BlockState(1, 2));
                SetBlockStateD(11, h, 6, new BlockState(1, 3));
                SetBlockStateD(12, h, 6, new BlockState(1, 3));
                SetBlockStateD(10, h, 6, new BlockState(Granite));
                

                SetBlockStateD(12, h + 1, 9, new BlockState(1));
                SetBlockStateD(12, h + 2, 9, new BlockState(1, 0));
                SetBlockStateD(12, h + 3, 9, new BlockState(1, 1));
                SetBlockStateD(12, h + 4, 9, new BlockState(1, 1));
                SetBlockStateD(12, h + 5, 9, new BlockState(1, 1));
                SetBlockStateD(12, h + 6, 9, new BlockState(1, 2));
                SetBlockStateD(12, h + 7, 9, new BlockState(1, 2));
                SetBlockStateD(12, h + 8, 9, new BlockState(1, 2));
                SetBlockStateD(12, h + 9, 9, new BlockState(1, 3));
                SetBlockStateD(12, h + 10, 9, new BlockState(1, 3));
                SetBlockStateD(12, h + 11, 9, new BlockState(1, 3));
                SetBlockStateD(12, h, 9, new BlockState(1));
                SetBlockStateD(11, h, 9, new BlockState(1));
                SetBlockStateD(11, h, 10, new BlockState(1));
                SetBlockStateD(12, h, 10, new BlockState(1));


                //if (X > 4 || X < -4) return;
                //if (Y > 4 || Y < -4) return;

                //if (Y > 5 || Y < -5) return;
                //if (Y > 3 || Y < -5) return;


                // Debug.Burden(.6f);

                //World.Filer.EndSectionLog(); // 0.3 мс
                //World.Filer.StartSection("GHM " + CurrentChunkX + "," + CurrentChunkY);
                //Light.SetLightBlocks(chunkPrimer.arrayLightBlocks.ToArray());
                Light.GenerateHeightMap(); // 0.02 мс
                //InitHeightMapGen();
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
        /// Получить блок данных, XZ 0..15, Y 0..255
        /// </summary>
        public BlockState GetBlockState(int x, int y, int z)
        {
            if (x >> 4 == 0 && z >> 4 == 0) return GetBlockStateNotCheck(x, y, z);
            return new BlockState().Empty();
        }

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
                int index = (y & 15) << 8 | z << 4 | x;
                return new BlockState(0, 0, (byte)(chunkStorage.Light[index] >> 4), (byte)(chunkStorage.Light[index] & 15));
            }
        }

        /// <summary>
        /// Задать новые данные блока, с перерасчётом освещения если надо и прочего, возвращает прошлые данные блока
        /// </summary>
        /// <param name="blockPos">Позици блока</param>
        /// <param name="blockState">Данные нового блока</param>
        /// <param name="isModify">Пометка надо сохранение чанка</param>
        /// <param name="isModifyRender">Пометка надо обновить рендер чанка</param>
        /// <param name="isSound">Звуковое сопровождение сломанного блока</param>
        public BlockState SetBlockState(BlockPos blockPos, BlockState blockState,
            bool isModify, bool isModifyRender, bool isSound)
        {
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

            int index = (by & 15) << 8 | bz << 4 | bx;
            storage.SetData(index, blockState.Id, blockState.Met);

            if (blockOld != block) // Блоки разные
            {
                // Отмена тик блока
                //RemoveBlockTick(bx, by, bz);

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
                }

                if (World.IsRemote)
                {
                    World.DebugString(World.Light.ToDebugString());
                }
                else
                {
                    // Действие блока после его удаления
                 //   blockOld.OnBreakBlock(World, blockPos, blockStateOld);
                }
            }
            else if (blockState.Met != blockStateOld.Met) // Метданные разные
            {
                if (isModifyRender) World.MarkBlockForUpdate(blockPos.X, blockPos.Y, blockPos.Z);
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
            if (storage.CountBlock != 0 && storage.Data[index] != block.Id) return new BlockState();

            return blockStateOld;
        }

        /// <summary>
        /// Ставим блок в своём чанке, xz 0-15, y 0-max
        /// Временно, для отладки. 2024-12-03
        /// </summary>
        public void SetBlockStateD(int x, int y, int z, BlockState blockState)
        {
            int index = (y & 15) << 8 | z << 4 | x;
            ChunkStorage storage = StorageArrays[y >> 4];
            storage.SetData(index, blockState.Id, blockState.Met);
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
                        _bigStreamOut.Read(storage.Light, 0, 4096);

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
            }
            IsChunkPresent = true;
        }

        #endregion

        /// <summary>
        /// Пометить что надо перерендерить сетку чанка для клиента
        /// </summary>
        public virtual void ModifiedToRender(int y) { }

        /// <summary>
        /// Пометка что чанк надо будет перезаписать
        /// </summary>
        public void Modified() => _isModified = true;

        public override string ToString() => CurrentChunkX + " : " + CurrentChunkY;
    }
}
