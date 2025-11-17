using System;
using Vge;
using Vge.Games;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;
using Vge.World.Gen;

namespace Mvk2.World.Gen
{
    /// <summary>
    /// Объект генерации чанка для мира, генерация Остров
    /// </summary>
    public class ChunkProviderGenerateIsland : IChunkProviderGenerate
    {
        /// <summary>
        /// Количество секций в чанке. Максимально 32
        /// </summary>
        public readonly byte NumberChunkSections;

        /// <summary>
        /// Вспомогательный рандом
        /// </summary>
        public Rand Rnd { get; private set; }
        /// <summary>
        /// Зерно генерации случайных чисел игры
        /// </summary>
        public long Seed { get; private set; }

        /// <summary>
        /// Шум нижнего слоя
        /// </summary>
        public NoiseGeneratorPerlin NoiseDown { get; private set; }

        /// <summary>
        /// Чанк для заполнения данных
        /// </summary>
        private readonly ChunkPrimerIsland _chunkPrimer;

        public ChunkProviderGenerateIsland(byte numberChunkSections)
        {
            NumberChunkSections = numberChunkSections;
            _chunkPrimer = new ChunkPrimerIsland(numberChunkSections);
        }

        public void InitLoading(GameServer server, WorldServer worldServer)
        {
            Seed = server.Settings.Seed;
            Rnd = new Rand(Seed);

            NoiseDown = new NoiseGeneratorPerlin(new Rand(Seed), 1);
        }

        public void GenerateChunk(ChunkBase chunk)
        {
            _chunkPrimer.Clear();

            int x, y, z;
            int h = chunk.NumberSections == 8 ? 47 : 95;
            // Временно льём тест

            ushort Stone, Cobblestone, Limestone, Granite, Glass, GlassRed, GlassGreen,
                GlassBlue, GlassPurple, FlowerClover, Water, Lava, Brol;
            Stone = Cobblestone = Limestone = Granite = Glass = GlassRed = GlassGreen =
                GlassBlue = GlassPurple = FlowerClover = Water = Lava = Brol = 0;


            for (ushort i = 0; i < Ce.Blocks.BlockAlias.Length; i++)
            {
                switch (Ce.Blocks.BlockAlias[i])
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
            //Water = Lava;
            //GlassRed = GlassGreen = GlassBlue = GlassPurple = Glass;

            for (x = 0; x < 16; x++)
            {
                for (z = 0; z < 16; z++)
                {
                    for (y = 0; y < h; y++)
                    {
                        chunk.SetBlockStateD(x, y, z, new BlockState(Stone));
                    }
                }
            }

            if (chunk.X == 0 && chunk.Y == 0)
            {
                for (y = h - 16; y < h - 4; y++)
                {
                    chunk.SetBlockStateD(0, y, 0, new BlockState(0));
                    chunk.SetBlockStateD(1, y, 0, new BlockState(0));
                }
                for (y = h + 16; y < h + 20; y++)
                {
                    chunk.SetBlockStateD(15, y, 0, new BlockState(Water));
                }
            }
            if (chunk.X == 0 && chunk.Y == -1)
            {
                for (y = h - 16; y < h - 4; y++)
                {
                    chunk.SetBlockStateD(1, y, 15, new BlockState(Water));
                }
            }
            if (chunk.X == 0 && chunk.Y == 0)
            {
                for (y = h - 16; y < h - 4; y++)
                {
                    chunk.SetBlockStateD(1, y, 1, new BlockState(Water));
                }
            }

            for (x = 2; x < 8; x++)
            {
                for (z = 0; z < 16; z++)
                {
                    //for (int y = h - 42; y < h; y++)
                    for (y = h - 2; y < h; y++)
                    {
                        //SetBlockState(x, y, z, new BlockState(GlassBlue));
                        chunk.SetBlockStateD(x, y, z, new BlockState(0));
                        // SetBlockStateD(x, y, z, new BlockState(Water));
                    }


                }
            }
            for (x = 2; x < 8; x++)
            {
                for (z = 0; z < 16; z++)
                {
                    chunk.SetBlockStateD(x, h - 3, z, new BlockState(Water));
                }
            }

            for (x = 9; x < 16; x++)
            {
                for (z = 0; z < 16; z++)
                {
                    for (y = h - 12; y < h - 2; y++)
                    {
                        chunk.SetBlockStateD(x, y, z, new BlockState(0));
                    }
                }
            }

            if (chunk.X > -2 && chunk.X < 2 && chunk.Y > -2 && chunk.Y < 2)
            {

                chunk.SetBlockStateD(5, h, 5, new BlockState(GlassRed));
                chunk.SetBlockStateD(5, h - 1, 4, new BlockState(GlassRed));
                chunk.SetBlockStateD(5, h - 2, 3, new BlockState(GlassRed));

                chunk.SetBlockStateD(4, h, 7, new BlockState(Water));

                chunk.SetBlockStateD(1, h + 2, 0, new BlockState(Water));
                chunk.SetBlockStateD(1, h + 1, 0, new BlockState(Water, 13));
                chunk.SetBlockStateD(1, h, 0, new BlockState(Water, 11));
                chunk.SetBlockStateD(2, h, 0, new BlockState(Water, 9));

                chunk.SetBlockStateD(8, h - 1, 15, new BlockState(Water));
                chunk.SetBlockStateD(8, h - 1, 0, new BlockState(Water));
                for (x = 0; x < 7; x++)
                {
                    chunk.SetBlockStateD(x + 9, h - 1, 15, new BlockState(Water, (byte)(13 - x * 2)));
                    if (x < 6) chunk.SetBlockStateD(x + 9, h - 1, 0, new BlockState(Water, (byte)(13 - x * 2)));

                    if (x < 6) chunk.SetBlockStateD(x + 8, h - 1, 14, new BlockState(Lava, (byte)(13 - x * 3)));
                }
                chunk.SetBlockStateD(14, h - 1, 1, new BlockState(Water, 1));
                chunk.SetBlockStateD(13, h - 1, 14, new BlockState(Water, 2));
                chunk.SetBlockStateD(13, h - 1, 13, new BlockState(Water, 1));

                chunk.SetBlockStateD(7, h - 1, 14, new BlockState(Lava));
                chunk.SetBlockStateD(7, h, 15, new BlockState(Lava));
                chunk.SetBlockStateD(7, h + 1, 15, new BlockState(Lava));
                chunk.SetBlockStateD(6, h + 16, 5, new BlockState(Lava));

                for (x = 1; x < 8; x++)
                {
                    for (z = 6; z < 11; z++)
                    {
                        chunk.SetBlockStateD(x, h, z, new BlockState(Granite));
                        //SetBlockState(x, h - 1, z, new BlockState(Lava));
                        chunk.SetBlockStateD(x, h + 9, z, new BlockState(Limestone));
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


                for (y = h; y < h + 32; y++)
                {
                    chunk.SetBlockStateD(7, y, 5, new BlockState(Cobblestone));
                    if (y > h + 16)
                    {
                        chunk.SetBlockStateD(6, y, 5, new BlockState(Water));
                    }

                }

                chunk.SetBlockStateD(0, h, 0, new BlockState(Cobblestone));
                chunk.SetBlockStateD(0, h + 1, 0, new BlockState(Cobblestone));
                chunk.SetBlockStateD(0, h + 2, 0, new BlockState(Cobblestone));

                chunk.SetBlockStateD(10, h, 10, new BlockState(FlowerClover));
                chunk.SetBlockStateD(12, h, 12, new BlockState(FlowerClover));
                chunk.SetBlockStateD(15, h, 10, new BlockState(FlowerClover));
                chunk.SetBlockStateD(15, h, 12, new BlockState(FlowerClover));
                chunk.SetBlockStateD(0, h, 15, new BlockState(FlowerClover));
                chunk.SetBlockStateD(1, h, 15, new BlockState(FlowerClover));





                chunk.SetBlockStateD(15, h, 15, new BlockState(Limestone));
                chunk.SetBlockStateD(15, h + 1, 15, new BlockState(Limestone));

                chunk.SetBlockStateD(8, h, 5, new BlockState(Cobblestone));
                chunk.SetBlockStateD(8, h, 6, new BlockState(Granite));
                chunk.SetBlockStateD(8, h + 3, 7, new BlockState(Cobblestone));
                chunk.SetBlockStateD(8, h + 4, 7, new BlockState(Limestone));


                for (y = h + 5; y < h + 10; y++)
                {
                    chunk.SetBlockStateD(8, y, 3, new BlockState(Granite));

                    chunk.SetBlockStateD(11, y, 5, new BlockState(Glass));
                    chunk.SetBlockStateD(8, y, 5, new BlockState(GlassRed));
                    chunk.SetBlockStateD(9, y, 12, new BlockState(GlassGreen));
                    chunk.SetBlockStateD(10, y, 13, new BlockState(GlassBlue));
                    chunk.SetBlockStateD(11, y, 15, new BlockState(GlassPurple));
                }

                chunk.SetBlockStateD(12, h + 5, 5, new BlockState(GlassRed));
                chunk.SetBlockStateD(12, h + 6, 5, new BlockState(GlassGreen));

                chunk.SetBlockStateD(11, h - 1, 4, new BlockState(1));
                chunk.SetBlockStateD(11, h - 1, 3, new BlockState(1));
                chunk.SetBlockStateD(12, h - 1, 4, new BlockState(1));
                chunk.SetBlockStateD(12, h - 1, 3, new BlockState(1));
                chunk.SetBlockStateD(11, h - 2, 4, new BlockState(0));
                chunk.SetBlockStateD(11, h - 2, 3, new BlockState(0));
                chunk.SetBlockStateD(12, h - 2, 4, new BlockState(0));
                chunk.SetBlockStateD(12, h - 2, 3, new BlockState(0));

                chunk.SetBlockStateD(13, h, 8, new BlockState(1));
                chunk.SetBlockStateD(12, h, 7, new BlockState(1, 1));
                chunk.SetBlockStateD(11, h, 7, new BlockState(1, 2));
                chunk.SetBlockStateD(11, h, 6, new BlockState(1, 3));
                chunk.SetBlockStateD(12, h, 6, new BlockState(1, 3));
                chunk.SetBlockStateD(10, h, 6, new BlockState(Granite));


                chunk.SetBlockStateD(12, h + 1, 9, new BlockState(1));
                chunk.SetBlockStateD(12, h + 2, 9, new BlockState(1, 0));
                chunk.SetBlockStateD(12, h + 3, 9, new BlockState(1, 1));
                chunk.SetBlockStateD(12, h + 4, 9, new BlockState(1, 1));
                chunk.SetBlockStateD(12, h + 5, 9, new BlockState(1, 1));
                chunk.SetBlockStateD(12, h + 6, 9, new BlockState(1, 2));
                chunk.SetBlockStateD(12, h + 7, 9, new BlockState(1, 2));
                chunk.SetBlockStateD(12, h + 8, 9, new BlockState(1, 2));
                chunk.SetBlockStateD(12, h + 9, 9, new BlockState(1, 3));
                chunk.SetBlockStateD(12, h + 10, 9, new BlockState(1, 3));
                chunk.SetBlockStateD(12, h + 11, 9, new BlockState(1, 3));
                chunk.SetBlockStateD(12, h, 9, new BlockState(1));
                chunk.SetBlockStateD(11, h, 9, new BlockState(1));
                chunk.SetBlockStateD(11, h, 10, new BlockState(1));
                chunk.SetBlockStateD(12, h, 10, new BlockState(1));

            }
            //if (X > 4 || X < -4) return;
            //if (Y > 4 || Y < -4) return;

            //if (Y > 5 || Y < -5) return;
            //if (Y > 3 || Y < -5) return;


            // Debug.Burden(.6f);

            ChunkStorage chunkStorage;
           
            int yc, ycb, y0;
            ushort id;
            for (yc = 0; yc < NumberChunkSections; yc++)
            {
                ycb = yc << 4;
                chunkStorage = chunk.StorageArrays[yc];
                for (y = 0; y < 16; y++)
                {
                    y0 = ycb | y;
                    if (y0 <= chunk.Settings.NumberMaxBlock)
                    {
                        for (x = 0; x < 16; x++)
                        {
                            for (z = 0; z < 16; z++)
                            {
                                //  chunkStorage.SetData(y << 8 | z << 4 | x, _chunkPrimer.Id[x << 12 | z << 8 | y0]);
                                id = _chunkPrimer.Id[y0 << 8 | z << 4 | x];
                                if (id != 0)
                                {
                                    chunkStorage.SetData(y << 8 | z << 4 | x, id);
                                }
                            }
                        }
                    }
                }
            }

            //World.Filer.EndSectionLog(); // 0.3 мс
            //World.Filer.StartSection("GHM " + CurrentChunkX + "," + CurrentChunkY);
            //Light.SetLightBlocks(chunkPrimer.arrayLightBlocks.ToArray());
            chunk.Light.GenerateHeightMap(); // 0.02 мс
            //InitHeightMapGen();
            //World.Filer.EndSectionLog();
        }



        public void Populate(ChunkBase chunk)
        {
            Debug.Burden(1.5f);
        }
    }
}
