using Mvk2.World.Biome;
using Mvk2.World.Block;
using Mvk2.World.Gen.Layer;
using Vge.Games;
using Vge.Util;
using Vge.World;
using Vge.World.Chunk;
using Vge.World.Gen;
using Vge.World.Gen.Layer;
using WinGL.Util;

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

        /// <summary>
        /// Объект генерации слоёв биомов
        /// </summary>
        private readonly GenLayer _genLayerBiome;
        /// <summary>
        /// Объект генерации слоёв высот от биомов
        /// </summary>
        private readonly GenLayer _genLayerHeight;

        // Шум речных пещер, из двух частей
        private readonly NoiseGeneratorPerlin noiseCave1;
        private readonly NoiseGeneratorPerlin noiseCaveHeight1;
        private readonly NoiseGeneratorPerlin noiseCave3;
        private readonly NoiseGeneratorPerlin noiseCaveHeight3;

        private readonly float[] caveRiversNoise = new float[256];
        private readonly float[] caveHeightNoise = new float[256];
        private readonly float[] caveNoise3 = new float[256];
        private readonly float[] caveHeightNoise3 = new float[256];

        public ChunkProviderGenerateIsland(byte numberChunkSections)
        {
            NumberChunkSections = numberChunkSections;
            _chunkPrimer = new ChunkPrimerIsland(numberChunkSections);

            GenLayer[] gens = GenLayerIsland.BeginLayerBiome(Seed);
            _genLayerBiome = gens[0];
            _genLayerHeight = gens[1];

            noiseCave1 = new NoiseGeneratorPerlin(new Rand(Seed + 7), 4);
            noiseCaveHeight1 = new NoiseGeneratorPerlin(new Rand(Seed + 5), 4);
            noiseCave3 = new NoiseGeneratorPerlin(new Rand(Seed + 12), 4);
            noiseCaveHeight3 = new NoiseGeneratorPerlin(new Rand(Seed + 13), 4);
        }

        public void InitLoading(GameServer server, WorldServer worldServer)
        {
            Seed = server.Settings.Seed;
            Rnd = new Rand(Seed);

            NoiseDown = new NoiseGeneratorPerlin(new Rand(Seed), 1);
        }

        private readonly float[] downNoise = new float[256];

        public void GenerateChunk(ChunkBase chunk)
        {
            _chunkPrimer.Clear();

            int xbc = chunk.CurrentChunkX << 4;
            int zbc = chunk.CurrentChunkY << 4;

            float scale = 1.0f;

            NoiseDown.GenerateNoise2d(downNoise, xbc, zbc, 16, 16, scale, scale);

            //  шумы речных пещер
            noiseCave1.GenerateNoise2d(caveRiversNoise, xbc, zbc, 16, 16, .05f, .05f);
            noiseCaveHeight1.GenerateNoise2d(caveHeightNoise, xbc, zbc, 16, 16, .025f, .025f);
            noiseCave3.GenerateNoise2d(caveNoise3, xbc, zbc, 16, 16, .05f, .05f);
            noiseCaveHeight3.GenerateNoise2d(caveHeightNoise3, xbc, zbc, 16, 16, .025f, .025f);

            EnumBiomeIsland enumBiome = EnumBiomeIsland.Plain;
            int x, y, z, idx;
            int count = 0;

            // Низ бедрок
            for (x = 0; x < 16; x++)
            {
                for (z = 0; z < 16; z++)
                {
                    float n = downNoise[count];
                    count++;
                    _chunkPrimer.SetBlockState(x, 0, z, 2);
                    _chunkPrimer.SetBlockState(x, 1, z, (ushort)(n < .1 ? 2 : 3));
                    _chunkPrimer.SetBlockState(x, 2, z, (ushort)(n < -.1 ? 2 : 3));
                }
            }

            int[] arHeight = _genLayerHeight.GetInts(xbc, zbc, 16, 16);
            int[] arBiome = _genLayerBiome.GetInts(xbc, zbc, 16, 16);
            count = 0;
            int h, b;
            for (x = 0; x < 16; x++)
            {
                for (z = 0; z < 16; z++)
                {
                    idx = z << 4 | x;
                    h = arHeight[idx];
                    if (h > 2)
                    {
                        for (y = 3; y < h; y++)
                        {
                            _chunkPrimer.SetBlockState(x, y, z, 4);
                        }
                        b = arBiome[idx];
                        _chunkPrimer.SetBlockState(x, h, z, (ushort)(b + 1));
                    }

                    // Пещенры 2д ввиде рек
                   // if ((enumBiome == EnumBiome.Mountains || enumBiome == EnumBiome.MountainsDesert && level > 58))
                    {
                        // В горах и пустыных горах могут быть пещеры
                        _ColumnCave2d(caveRiversNoise[count] / 8f, caveHeightNoise[count] / 8f, x, z, enumBiome,
                        .12f, .28f, 12.5f, 6f, 80f, 56); //  5f, 64f, 72);
                    }
                  //  if (enumBiome == EnumBiome.Mountains)
                    {
                        // В горах внизу может быть лава
                        _ColumnCave2d(caveNoise3[count] / 8f, caveHeightNoise3[count] / 8f, x, z, enumBiome,
                           .10f, .30f, 10f, 18f, 12f, 18); // 12f, 12f, 16);
                    }
                    count++;
                    //_chunkPrimer.SetBlockState(x, 3, z, (ushort)(h + 1));
                }
            }

            //int h = chunk.NumberSections == 8 ? 47 : 95;
            //// Временно льём тест

            //ushort Stone = BlocksRegMvk.Stone.IndexBlock;
            //ushort Cobblestone = BlocksRegMvk.Cobblestone.IndexBlock;
            //ushort Limestone = BlocksRegMvk.Limestone.IndexBlock;
            //ushort Granite = BlocksRegMvk.Granite.IndexBlock;
            //ushort Glass = BlocksRegMvk.Glass.IndexBlock;
            //ushort GlassRed = BlocksRegMvk.GlassRed.IndexBlock;
            //ushort GlassBlue = BlocksRegMvk.GlassBlue.IndexBlock;
            //ushort GlassPurple = BlocksRegMvk.GlassPurple.IndexBlock;
            //ushort GlassGreen = BlocksRegMvk.GlassGreen.IndexBlock;
            //ushort FlowerClover = BlocksRegMvk.FlowerClover.IndexBlock;
            //ushort Water = BlocksRegMvk.Water.IndexBlock;
            //ushort Lava = BlocksRegMvk.Lava.IndexBlock;
            ////ushort Brol = BlocksRegMvk.Brol.IndexBlock;

            //for (x = 0; x < 16; x++)
            //{
            //    for (z = 0; z < 16; z++)
            //    {
            //        for (y = 0; y < h; y++)
            //        {
            //            _chunkPrimer.SetBlockState(x, y, z, Stone);
            //        }
            //    }
            //}

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
            chunk.Light.SetLightBlocks(_chunkPrimer.ArrayLightBlocks.ToArray());
            chunk.Light.GenerateHeightMap(); // 0.02 мс
            //InitHeightMapGen();
            //World.Filer.EndSectionLog();
        }





        public void Populate(ChunkBase chunk)
        {
           // Debug.Burden(1.5f);
        }


        /// <summary>
        /// Столбец речных шумов
        /// </summary>
        /// <param name="cr">шум реки</param>
        /// <param name="ch">шум высоты</param>
        /// <param name="x">координата столбца X</param>
        /// <param name="z">координата столбца Z</param>
        /// <param name="enumBiome">биом столбца</param>
        /// <param name="min">минимальный коэф для ширины реки</param>
        /// <param name="max">максимальны коэф для ширины реки</param>
        /// <param name="size">размер для разницы коэф, чтоб значение было 2, пример: min=0.1 и max=0.3 size = 2 / (max-min)</param>
        /// <param name="heightCave">Высота пещеры</param>
        /// <param name="heightLevel">Уровень амплитуды пещер по миру</param>
        /// <param name="level">Центр амплитуды Y</param>
        private void _ColumnCave2d(float cr, float ch, int x, int z, EnumBiomeIsland enumBiome,
            float min, float max, float size, float heightCave, float heightLevel, int level)
        {
            // Пещенры 2д ввиде рек

            int heightMax = 127;
            int heightWater = 47;

            if ((cr >= min && cr <= max) || (cr <= -min && cr >= -max))
            {
                float h = (enumBiome == EnumBiomeIsland.River || enumBiome == EnumBiomeIsland.Sea || enumBiome == EnumBiomeIsland.Swamp)
                    ? _chunkPrimer.HeightMap[x << 4 | z] : heightMax;
                h -= 4;
                if (h > heightWater) h = heightMax;

                if (cr < 0) cr = -cr;
                cr = (cr - min) * size;
                if (cr > 1f) cr = 2f - cr;
                cr = 1f - cr;
                cr = cr * cr;
                cr = 1f - cr;
                int ych = (int)(cr * heightCave) + 3;
                ych = (ych / 2);

                int ych2 = level + (int)(ch * heightLevel);
                int cy1 = ych2 - ych;
                if (cy1 < 3) cy1 = 3;
                int cy2 = ych2 + ych;
                if (cy2 > heightMax) cy2 = heightMax;
                int index, id;

                // Высота пещерных рек 4 .. ~120
                for (int y = cy1; y <= cy2; y++)
                {
                    if (y < h)
                    {
                        //index = y << 8 | z << 4 | x;
                        //id = _chunkPrimer.Id[index];
                        //if (id == 3 || id == 9 || id == 10 || id == 7
                        //    || (id == 8 && _chunkPrimer.Id[index + 1] != 13))
                        //{
                        //    if (y < 12)
                        //    {
                        //        _chunkPrimer.SetBlockState(x, y, z, 0); // 15 лава
                        //        //_chunkPrimer.ArrayLightBlocks.Add(new vec3i(x, y, z));
                        //    }
                        //    else _chunkPrimer.SetBlockState(x, y, z, 0); // воздух
                        //}

                        if (y < 12)
                        {
                            _chunkPrimer.SetBlockState(x, y, z, BlocksRegMvk.Lava.IndexBlock); // лава
                            _chunkPrimer.ArrayLightBlocks.Add((uint)(y << 8 | z << 4 | x));
                        }
                        else _chunkPrimer.SetBlockState(x, y, z, 0); // воздух
                    }
                }
            }
        }
    }
}
