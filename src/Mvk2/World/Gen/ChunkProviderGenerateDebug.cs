using Mvk2.World.Block;
using Vge;
using Vge.Games;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;
using Vge.World.Gen;

namespace Mvk2.World.Gen
{
    public class ChunkProviderGenerateDebug : IChunkProviderGenerate
    {
        /// <summary>
        /// Количество секций в чанке. Максимально 32
        /// </summary>
        public readonly byte NumberChunkSections;
        /// <summary>
        /// Чанк для заполнения данных
        /// </summary>
        private readonly ChunkPrimerIsland _chunkPrimer;

        public ChunkProviderGenerateDebug(byte numberChunkSections)
        {
            NumberChunkSections = numberChunkSections;
            _chunkPrimer = new ChunkPrimerIsland(numberChunkSections);
        }

        /// <summary>
        /// Генерация рельефа чанка, соседние чанки не требуются
        /// </summary>
        public void Relief(ChunkBase chunk)
        {
            _chunkPrimer.Clear();

            int x, y, z;
            int h = chunk.NumberSections == 8 ? 47 : 95;
            // Временно льём тест

            ushort Stone = BlocksRegMvk.Stone.IndexBlock;
            ushort Cobblestone = BlocksRegMvk.Cobblestone.IndexBlock;
            ushort Limestone = BlocksRegMvk.Limestone.IndexBlock;
            ushort Granite = BlocksRegMvk.Granite.IndexBlock;
            ushort Glass = BlocksRegMvk.Glass.IndexBlock;
            ushort GlassRed = BlocksRegMvk.GlassRed.IndexBlock;
            ushort GlassBlue = BlocksRegMvk.GlassBlue.IndexBlock;
            ushort GlassPurple = BlocksRegMvk.GlassPurple.IndexBlock;
            ushort GlassGreen = BlocksRegMvk.GlassGreen.IndexBlock;
            ushort FlowerClover = BlocksRegMvk.FlowerClover.IndexBlock;
            ushort Water = BlocksRegMvk.Water.IndexBlock;
            ushort Lava = BlocksRegMvk.Lava.IndexBlock;
            ushort Turf = BlocksRegMvk.Turf.IndexBlock;
            //ushort Brol = BlocksRegMvk.Brol.IndexBlock;

            for (x = 0; x < 16; x++)
            {
                for (z = 0; z < 16; z++)
                {
                    for (y = 0; y < h - 1; y++)
                    {
                        _chunkPrimer.SetBlockState(x, y, z, Stone);
                    }
                    _chunkPrimer.SetBlockState(x, h - 1, z, Turf);
                    
                }
            }

            if (chunk.X == 0 && chunk.Y == 0)
            {
                for (y = h - 16; y < h - 4; y++)
                {
                    _chunkPrimer.SetBlockState(0, y, 0, 0);
                    _chunkPrimer.SetBlockState(1, y, 0, 0);
                }
                for (y = h + 16; y < h + 20; y++)
                {
                    _chunkPrimer.SetBlockState(15, y, 0, Water);
                }
            }
            if (chunk.X == 0 && chunk.Y == -1)
            {
                for (y = h - 16; y < h - 4; y++)
                {
                    _chunkPrimer.SetBlockState(1, y, 15, Water);
                }
            }
            if (chunk.X == 0 && chunk.Y == 0)
            {
                for (y = h - 16; y < h - 4; y++)
                {
                    _chunkPrimer.SetBlockState(1, y, 1, Water);
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
                        _chunkPrimer.SetBlockState(x, y, z, 0);
                        // SetBlockStateD(x, y, z, new BlockState(Water));
                    }


                }
            }
            for (x = 2; x < 8; x++)
            {
                for (z = 0; z < 16; z++)
                {
                    _chunkPrimer.SetBlockState(x, h - 3, z, Water);
                }
            }

            for (x = 9; x < 16; x++)
            {
                for (z = 0; z < 16; z++)
                {
                    for (y = h - 12; y < h - 2; y++)
                    {
                        _chunkPrimer.SetBlockState(x, y, z, 0);
                    }
                }
            }

            if (chunk.X > -2 && chunk.X < 2 && chunk.Y > -2 && chunk.Y < 2)
            {

                // Biome
                chunk.Biome[0 << 4 | 11] = 9;
                chunk.Biome[0 << 4 | 12] = 9;
                chunk.Biome[0 << 4 | 13] = 9;
                chunk.Biome[1 << 4 | 11] = 9;
                chunk.Biome[1 << 4 | 12] = 9;
                chunk.Biome[1 << 4 | 13] = 9;
                chunk.Biome[2 << 4 | 11] = 9;
                chunk.Biome[2 << 4 | 12] = 9;
                chunk.Biome[2 << 4 | 13] = 9;
                chunk.Biome[3 << 4 | 11] = 9;
                chunk.Biome[3 << 4 | 12] = 9;
                chunk.Biome[3 << 4 | 13] = 9;


                _chunkPrimer.SetBlockState(5, h, 5, GlassRed);
                _chunkPrimer.SetBlockState(5, h - 1, 4, GlassRed);
                _chunkPrimer.SetBlockState(5, h - 2, 3, GlassRed);

                _chunkPrimer.SetBlockState(4, h, 7, Water);

                _chunkPrimer.SetBlockState(1, h + 2, 0, Water);
                _chunkPrimer.SetBlockState(1, h + 1, 0, Water, 13);
                _chunkPrimer.SetBlockState(1, h, 0, Water, 11);
                _chunkPrimer.SetBlockState(2, h, 0, Water, 9);

                _chunkPrimer.SetBlockState(8, h - 1, 15, Water);
                _chunkPrimer.SetBlockState(8, h - 1, 0, Water);
                for (x = 0; x < 7; x++)
                {
                    _chunkPrimer.SetBlockState(x + 9, h - 1, 15, Water, (byte)(13 - x * 2));
                    if (x < 6) _chunkPrimer.SetBlockState(x + 9, h - 1, 0, Water, (byte)(13 - x * 2));

                    if (x < 6) _chunkPrimer.SetBlockState(x + 8, h - 1, 14, Lava, (byte)(13 - x * 3));
                }
                _chunkPrimer.SetBlockState(14, h - 1, 1, Water, 1);
                _chunkPrimer.SetBlockState(13, h - 1, 14, Water, 2);
                _chunkPrimer.SetBlockState(13, h - 1, 13, Water, 1);

                _chunkPrimer.SetBlockState(7, h - 1, 14, Lava);
                _chunkPrimer.SetBlockState(7, h, 15, Lava);
                _chunkPrimer.SetBlockState(7, h + 1, 15, Lava);
                _chunkPrimer.SetBlockState(6, h + 16, 5, Lava);

                for (x = 1; x < 8; x++)
                {
                    for (z = 6; z < 11; z++)
                    {
                        _chunkPrimer.SetBlockState(x, h, z, Granite);
                        //SetBlockState(x, h - 1, z, Lava));
                        _chunkPrimer.SetBlockState(x, h + 9, z, Limestone);
                    }
                }

                //SetBlockStateD(4, h + 1, 8, Brol));
                //Light.SetLightBlock(4, h + 1, 8);

                //for (int x = 0; x < 8; x++)
                //{
                //    for (int z = 2; z < 11; z++)
                //    {
                //      //  SetBlockStateD(x, h + 6, z, Limestone));
                //        SetBlockStateD(x, h + 16, z, Lava));
                //        SetBlockStateD(x, h + 17, z, Lava));
                //        SetBlockStateD(x, h + 18, z, Lava));
                //        SetBlockStateD(x, h + 19, z, Lava));
                //    }
                //}


                for (y = h; y < h + 32; y++)
                {
                    _chunkPrimer.SetBlockState(7, y, 5, Cobblestone);
                    if (y > h + 16)
                    {
                        _chunkPrimer.SetBlockState(6, y, 5, Water);
                    }

                }

                _chunkPrimer.SetBlockState(0, h, 0, Cobblestone);
                _chunkPrimer.SetBlockState(0, h + 1, 0, Cobblestone);
                _chunkPrimer.SetBlockState(0, h + 2, 0, Cobblestone);

                _chunkPrimer.SetBlockState(10, h, 10, FlowerClover);
                _chunkPrimer.SetBlockState(12, h, 12, FlowerClover);
                _chunkPrimer.SetBlockState(15, h, 10, FlowerClover);
                _chunkPrimer.SetBlockState(15, h, 12, FlowerClover);
                _chunkPrimer.SetBlockState(0, h, 15, FlowerClover);
                _chunkPrimer.SetBlockState(1, h, 15, FlowerClover);





                _chunkPrimer.SetBlockState(15, h, 15, Limestone);
                _chunkPrimer.SetBlockState(15, h + 1, 15, Limestone);

                _chunkPrimer.SetBlockState(8, h, 5, Cobblestone);
                _chunkPrimer.SetBlockState(8, h, 6, Granite);
                _chunkPrimer.SetBlockState(8, h + 3, 7, Cobblestone);
                _chunkPrimer.SetBlockState(8, h + 4, 7, Limestone);


                for (y = h + 5; y < h + 10; y++)
                {
                    _chunkPrimer.SetBlockState(8, y, 3, Granite);

                    _chunkPrimer.SetBlockState(11, y, 5, Glass);
                    _chunkPrimer.SetBlockState(8, y, 5, GlassRed);
                    _chunkPrimer.SetBlockState(9, y, 12, GlassGreen);
                    _chunkPrimer.SetBlockState(10, y, 13, GlassBlue);
                    _chunkPrimer.SetBlockState(11, y, 15, GlassPurple);
                }

                _chunkPrimer.SetBlockState(12, h + 5, 5, GlassRed);
                _chunkPrimer.SetBlockState(12, h + 6, 5, GlassGreen);

                _chunkPrimer.SetBlockState(11, h - 1, 4, 1);
                _chunkPrimer.SetBlockState(11, h - 1, 3, 1);
                _chunkPrimer.SetBlockState(12, h - 1, 4, 1);
                _chunkPrimer.SetBlockState(12, h - 1, 3, 1);
                _chunkPrimer.SetBlockState(11, h - 2, 4, 0);
                _chunkPrimer.SetBlockState(11, h - 2, 3, 0);
                _chunkPrimer.SetBlockState(12, h - 2, 4, 0);
                _chunkPrimer.SetBlockState(12, h - 2, 3, 0);

                _chunkPrimer.SetBlockState(13, h, 8, 1);
                _chunkPrimer.SetBlockState(12, h, 7, 1, 1);
                _chunkPrimer.SetBlockState(11, h, 7, 1, 2);
                _chunkPrimer.SetBlockState(11, h, 6, 1, 3);
                _chunkPrimer.SetBlockState(12, h, 6, 1, 3);
                _chunkPrimer.SetBlockState(10, h, 6, Granite);


                _chunkPrimer.SetBlockState(12, h + 1, 9, 1);
                _chunkPrimer.SetBlockState(12, h + 2, 9, 1, 0);
                _chunkPrimer.SetBlockState(12, h + 3, 9, 1, 1);
                _chunkPrimer.SetBlockState(12, h + 4, 9, 1, 1);
                _chunkPrimer.SetBlockState(12, h + 5, 9, 1, 1);
                _chunkPrimer.SetBlockState(12, h + 6, 9, 1, 2);
                _chunkPrimer.SetBlockState(12, h + 7, 9, 1, 2);
                _chunkPrimer.SetBlockState(12, h + 8, 9, 1, 2);
                _chunkPrimer.SetBlockState(12, h + 9, 9, 1, 3);
                _chunkPrimer.SetBlockState(12, h + 10, 9, 1, 3);
                _chunkPrimer.SetBlockState(12, h + 11, 9, 1, 3);
                _chunkPrimer.SetBlockState(12, h, 9, 1);
                _chunkPrimer.SetBlockState(11, h, 9, 1);
                _chunkPrimer.SetBlockState(11, h, 10, 1);
                _chunkPrimer.SetBlockState(12, h, 10, 1);

            }
            //if (X > 4 || X < -4) return;
            //if (Y > 4 || Y < -4) return;

            //if (Y > 5 || Y < -5) return;
            //if (Y > 3 || Y < -5) return;


            // Debug.Burden(.6f);

            ChunkStorage chunkStorage;

            int yc, ycb, y0;
            ushort id;
            int index, indexY, indexYZ;
            for (yc = 0; yc < NumberChunkSections; yc++)
            {
                ycb = yc << 4;
                chunkStorage = chunk.StorageArrays[yc];
                for (y = 0; y < 16; y++)
                {
                    y0 = ycb | y;
                    if (y0 <= chunk.Settings.NumberMaxBlock)
                    {
                        indexY = y0 << 8;
                        for (z = 0; z < 16; z++)
                        {
                            indexYZ = indexY | z << 4;
                            for (x = 0; x < 16; x++)
                            {
                                index = indexYZ | x;
                                id = _chunkPrimer.Id[index];
                                if (id != 0)
                                {
                                    chunkStorage.SetData(y << 8 | z << 4 | x, id, _chunkPrimer.Met[index]);
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

        /// <summary>
        /// Декорация чанков, требуются соседние чанки (3*3)
        /// </summary>
        public void Decoration(ChunkProviderServer provider, ChunkBase chunk)
        {
            Debug.Burden(1.5f);
        }
    }
}
