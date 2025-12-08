using Mvk2.World.Biome;
using Mvk2.World.Block;
using Mvk2.World.Gen.Layer;
using System;
using Vge;
using Vge.Util;
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
        public readonly Rand Rnd;
        /// <summary>
        /// Зерно генерации случайных чисел игры
        /// </summary>
        public readonly long Seed;

        /// <summary>
        /// Количество блокуоыв в высоту
        /// </summary>
        private readonly int _countHeightBlock;

        /// <summary>
        /// Чанк для заполнения данных
        /// </summary>
        public readonly ChunkPrimerIsland ChunkPrimer;

        /// <summary>
        /// Объект генерации слоёв биомов
        /// </summary>
        private readonly GenLayer _genLayerBiome;
        /// <summary>
        /// Объект генерации слоёв высот от биомов
        /// </summary>
        private readonly GenLayer _genLayerHeight;

        #region Шум

        /// <summary>
        /// Шум нижнего слоя, берок
        /// </summary>
        private readonly NoiseGeneratorPerlin _noiseDown;
        /// <summary>
        /// Массив шума нижнего слоя, берок
        /// </summary>
        public readonly float[] DownNoise = new float[256];

        /// <summary>
        /// Шум для дополнительных областей, для корректировки рельефа
        /// </summary>
        private readonly NoiseGeneratorPerlin _noiseArea;
        /// <summary>
        /// Массив, шума для дополнительных областей, для корректировки рельефа
        /// </summary>
        public readonly float[] AreaNoise = new float[256];

        /// <summary>
        /// Шумы речных пещер, из двух частей
        /// </summary>
        private readonly NoiseGeneratorPerlin _noiseCave1;
        private readonly NoiseGeneratorPerlin _noiseCaveHeight1;
        private readonly NoiseGeneratorPerlin _noiseCave2;
        private readonly NoiseGeneratorPerlin _noiseCaveHeight2;

        /// <summary>
        /// Массив шумов речных пещер, из двух частей
        /// </summary>
        public readonly float[] CaveRiversNoise = new float[256];
        public readonly float[] CaveHeightNoise = new float[256];
        public readonly float[] CaveRiversNoise2 = new float[256];
        public readonly float[] CaveHeightNoise2 = new float[256];

        public readonly float[] Level2Noise = new float[256];
        public readonly float[] Level3Noise = new float[256];
        public readonly float[] Level4Noise = new float[256];
        public readonly float[] Level5Noise = new float[256];

        #endregion

        private readonly BiomeIsland[] _biomes;
        /// <summary>
        /// ID блока воды
        /// </summary>
        private readonly ushort _blockIdWater;

        public ChunkProviderGenerateIsland(byte numberChunkSections, long seed)
        {
            Seed = seed;
            Rnd = new Rand(Seed);
            NumberChunkSections = numberChunkSections;
            _countHeightBlock = numberChunkSections * 16 - 1;
            ChunkPrimer = new ChunkPrimerIsland(NumberChunkSections);

            GenLayer[] gens = GenLayerIsland.BeginLayerBiome(Seed);
            _genLayerBiome = gens[0];
            _genLayerHeight = gens[1];

            _noiseDown = new NoiseGeneratorPerlin(new Rand(Seed), 1);
            _noiseArea = new NoiseGeneratorPerlin(new Rand(Seed + 2), 4);
            _noiseCave1 = new NoiseGeneratorPerlin(new Rand(Seed + 7), 4);
            _noiseCaveHeight1 = new NoiseGeneratorPerlin(new Rand(Seed + 5), 4);
            _noiseCave2 = new NoiseGeneratorPerlin(new Rand(Seed + 12), 4);
            _noiseCaveHeight2 = new NoiseGeneratorPerlin(new Rand(Seed + 13), 4);

            _blockIdWater = BlocksRegMvk.Water.IndexBlock;

            _biomes = new BiomeIsland[]
            {
                new BiomeSea(this),
                new BiomeRiver(this),
                new BiomePlane(this),
                new BiomeDesert(this),
                new BiomeBeach(this),
                new BiomeMixedForest(this),
                new BiomeConiferousForest(this),
                new BiomeBirchForest(this),
                new BiomeIsland(this),
                new BiomeSwamp(this),
                new BiomeMountains(this),
                new BiomeIsland(this)
            };
        }

        private int iMin = int.MaxValue;
        private int iMax = int.MinValue;

        /// <summary>
        /// Генерация рельефа чанка, соседние чанки не требуются
        /// </summary>
        public void Relief(ChunkBase chunk)
        {
            try
            {
               // chunk.World.Filer.StartSection("ReliefChunk");

                int xbc = chunk.CurrentChunkX << 4;
                int zbc = chunk.CurrentChunkY << 4;

                ChunkPrimer.Clear();

                // Шум для бедрока
                _noiseDown.GenerateNoise2d(DownNoise, xbc, zbc, 16, 16, 1, 1);
                // Доп шумы
                _noiseArea.GenerateNoise2d(AreaNoise, xbc, zbc, 16, 16, .4f, .4f);
                // Шумы речных пещер
                _noiseCave1.GenerateNoise2d(CaveRiversNoise, xbc, zbc, 16, 16, .05f, .05f);
                _noiseCaveHeight1.GenerateNoise2d(CaveHeightNoise, xbc, zbc, 16, 16, .025f, .025f);
                _noiseCave2.GenerateNoise2d(CaveRiversNoise2, xbc, zbc, 16, 16, .05f, .05f);
                _noiseCaveHeight2.GenerateNoise2d(CaveHeightNoise2, xbc, zbc, 16, 16, .025f, .025f);
                // Шумф для рельефа
                _noiseArea.GenerateNoise2d(Level3Noise, xbc, zbc, 16, 16, .1f, .1f);
                _noiseCave1.GenerateNoise2d(Level2Noise, xbc, zbc, 16, 16, .1f, .1f);
                _noiseCaveHeight1.GenerateNoise2d(Level5Noise, xbc, zbc, 16, 16, .1f, .1f);
                _noiseCave2.GenerateNoise2d(Level4Noise, xbc, zbc, 16, 16, .1f, .1f);

                EnumBiomeIsland enumBiome;
                BiomeIsland biome;
                byte idBiome;
                int xz, level;

                int[] arHeight = _genLayerHeight.GetInts(xbc, zbc, 16, 16);
                int[] arBiome = _genLayerBiome.GetInts(xbc, zbc, 16, 16);

                for (xz = 0; xz < 256; xz++)
                {
                    // TODO::2025-11-29 временна чанки не грузим по X 0 и 1
                    if (chunk.CurrentChunkY != 0 && chunk.CurrentChunkY != 1
                        && chunk.CurrentChunkY != -4 && chunk.CurrentChunkY != -3
                        && chunk.CurrentChunkY != -8 && chunk.CurrentChunkY != -7)
                    //if (chunk.CurrentChunkY == 1 && chunk.CurrentChunkX == -17)
                    {
                        //ChunkPrimer.SetBlockState(xz, 0, BlocksRegMvk.Bedrock.IndexBlock);
                        //  ChunkPrimer.SetBlockState(xz, 1, (ushort)(dn < .1 ? 2 : 3));
                        //   ChunkPrimer.SetBlockState(xz, 2, (ushort)(dn < -.1 ? 2 : 3));

                        // Биомы
                        idBiome = (byte)arBiome[xz];
                        enumBiome = (EnumBiomeIsland)idBiome;
                        chunk.Biome[xz] = idBiome;
                        ChunkPrimer.Biome[xz] = enumBiome;
                        biome = _biomes[idBiome];
                        biome.Init(xbc, zbc);
                        level = biome.ReliefColumn(xz, arHeight[xz]);

                        //Provider.DownNoise[xz] * 5f) +1
                        int levelDebug = (int)(AreaNoise[xz] * .4f); // ~ 0 .. 3
                        if (levelDebug < iMin) iMin = levelDebug;
                        if (levelDebug > iMax) iMax = levelDebug;
                        Debug.Text = string.Format("{0:0.0} {1:0.0}", iMin, iMax);

                        
                        // Пещенры 2д ввиде рек
                        if (enumBiome == EnumBiomeIsland.Mountains
                            || (enumBiome == EnumBiomeIsland.MountainsDesert && level > 58))
                        {
                            // В горах и пустыных горах могут быть пещеры
                            _ColumnCave2d(CaveRiversNoise[xz] / 8f, CaveHeightNoise[xz] / 8f, xz, enumBiome,
                            .12f, .28f, 12.5f, 6f, 80f, 56);
                        }
                        if (enumBiome == EnumBiomeIsland.Mountains)
                        {
                            // В горах внизу может быть лава
                            _ColumnCave2d(CaveRiversNoise2[xz] / 8f, CaveHeightNoise2[xz] / 8f, xz, enumBiome,
                                .10f, .30f, 10f, 18f, 12f, 18);
                        }
                        
                    }
                }

                _ExportChuck(chunk);

              // chunk.World.Filer.EndSectionLog(); // 0.3 мс
                //World.Filer.StartSection("GHM " + CurrentChunkX + "," + CurrentChunkY);
                chunk.Light.SetLightBlocks(ChunkPrimer.ArrayLightBlocks.ToArray());
                chunk.Light.GenerateHeightMap(); // 0.02 мс
                chunk.InitHeightMapGen();
                //World.Filer.EndSectionLog();
            }
            catch (Exception ex)
            {
                Logger.Crash(ex, "GenerateChunk xc:{0} yc:{1}", chunk.CurrentChunkX, chunk.CurrentChunkY);
            }
        }

        /// <summary>
        /// Декорация чанков, требуются соседние чанки (3*3)
        /// </summary>
        public void Decoration(ChunkProviderServer provider, ChunkBase chunk)
        {
          //  chunk.World.Filer.StartSection("DecorationChunk");
            // Debug.Burden(1.5f);
            ChunkPrimer.Clear();

            BiomeIsland biome = _biomes[chunk.Biome[136]];
            biome.DecorationsColumn(chunk);
            ChunkBase chunkSpawn;

            int i;
            Vector2i[] array = Ce.AreaOne9priority[(chunk.CurrentChunkX % 2 == 0 ? 0 : 1) 
                + (chunk.CurrentChunkY % 2 == 0 ? 0 : 2)];
            // Декорация областей которые могу выйти за 1 чанк
            for (i = 0; i < 9; i++)
            {
                //chunk.ToPositionBias(array[i])
                chunkSpawn = provider.GetChunkPlus(chunk.CurrentChunkX + array[i].X, chunk.CurrentChunkY + array[i].Y);
                biome = _biomes[chunkSpawn.Biome[136]];
                biome.DecorationsArea(chunk, chunkSpawn);
            }

            biome.DecorationsColumnAfter(chunk);
            //biome.Decorator.GenDecorations(World, this, chunk);
            _ExportChuck(chunk);
           // chunk.World.Filer.EndSectionLog(); // 1.5
        }

        /// <summary>
        /// Экспортировать данные чанка с chunkPrimer в ChunkBase
        /// </summary>
        private void _ExportChuck(ChunkBase chunk)
        {
            ChunkStorage chunkStorage;
            int x, y, z, yc, ycb, y0, y8, yz;
            ushort id, idOld;
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
                        y8 = y << 8;
                        for (z = 0; z < 16; z++)
                        {
                            indexYZ = indexY | z << 4;
                            yz = y8 | z << 4;
                            for (x = 0; x < 16; x++)
                            {
                                index = indexYZ | x;
                                id = ChunkPrimer.Id[index];
                                if (id != 0)
                                {
                                    if (ChunkPrimer.Flag[index] == 1)
                                    {
                                        if (chunkStorage.CountBlock > 0)
                                        {
                                            idOld = chunkStorage.Data[yz | x];
                                            if (idOld != 0 && idOld != _blockIdWater)
                                            {
                                                chunkStorage.SetData(yz | x, id, ChunkPrimer.Met[index]);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        chunkStorage.SetData(yz | x, id, ChunkPrimer.Met[index]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Столбец речных шумов
        /// </summary>
        /// <param name="cr">шум реки</param>
        /// <param name="ch">шум высоты</param>
        /// <param name="xz">z << 4 | x</param>
        /// <param name="z">координата столбца Z</param>
        /// <param name="enumBiome">биом столбца</param>
        /// <param name="min">минимальный коэф для ширины реки</param>
        /// <param name="max">максимальны коэф для ширины реки</param>
        /// <param name="size">размер для разницы коэф, чтоб значение было 2, пример: min=0.1 и max=0.3 size = 2 / (max-min)</param>
        /// <param name="heightCave">Высота пещеры</param>
        /// <param name="heightLevel">Уровень амплитуды пещер по миру</param>
        /// <param name="level">Центр амплитуды Y</param>
        private void _ColumnCave2d(float cr, float ch, int xz, EnumBiomeIsland enumBiome,
            float min, float max, float size, float heightCave, float heightLevel, int level)
        {
            // Пещенры 2д ввиде рек
            if ((cr >= min && cr <= max) || (cr <= -min && cr >= -max))
            {
                float h = (enumBiome == EnumBiomeIsland.River || enumBiome == EnumBiomeIsland.Sea || enumBiome == EnumBiomeIsland.Swamp)
                    ? ChunkPrimer.HeightMap[xz] : _countHeightBlock;
                h -= 4;
                if (h > BiomeIsland.HeightWater) h = _countHeightBlock;

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
                if (cy2 > _countHeightBlock) cy2 = _countHeightBlock;

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
                            ChunkPrimer.SetBlockState(xz, y, BlocksRegMvk.Lava.IndexBlock); // лава
                            ChunkPrimer.ArrayLightBlocks.Add((uint)(y << 8 | xz));
                        }
                        else ChunkPrimer.SetBlockState(xz, y, 0); // воздух
                    }
                }
            }
        }
    }
}
