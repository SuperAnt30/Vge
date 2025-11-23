using Mvk2.World.Biome;
using Mvk2.World.Block;
using Mvk2.World.Gen.Layer;
using System;
using Vge.Util;
using Vge.World.Chunk;
using Vge.World.Gen;
using Vge.World.Gen.Layer;

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
        private readonly ChunkPrimerIsland _chunkPrimer;

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
        private readonly float[] _downNoise = new float[256];

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
        private readonly float[] _caveRiversNoise = new float[256];
        private readonly float[] _caveHeightNoise = new float[256];
        private readonly float[] _caveRiversNoise2 = new float[256];
        private readonly float[] _caveHeightNoise2 = new float[256];

        #endregion

        public ChunkProviderGenerateIsland(byte numberChunkSections, long seed)
        {
            Seed = seed;
            Rnd = new Rand(Seed);
            NumberChunkSections = numberChunkSections;
            _countHeightBlock = numberChunkSections * 16 - 1;
            _chunkPrimer = new ChunkPrimerIsland(NumberChunkSections);

            GenLayer[] gens = GenLayerIsland.BeginLayerBiome(Seed);
            _genLayerBiome = gens[0];
            _genLayerHeight = gens[1];

            _noiseDown = new NoiseGeneratorPerlin(new Rand(Seed), 1);
            _noiseArea = new NoiseGeneratorPerlin(new Rand(Seed + 2), 4);
            _noiseCave1 = new NoiseGeneratorPerlin(new Rand(Seed + 7), 4);
            _noiseCaveHeight1 = new NoiseGeneratorPerlin(new Rand(Seed + 5), 4);
            _noiseCave2 = new NoiseGeneratorPerlin(new Rand(Seed + 12), 4);
            _noiseCaveHeight2 = new NoiseGeneratorPerlin(new Rand(Seed + 13), 4);
        }

        public void GenerateChunk(ChunkBase chunk)
        {
            try
            {
                //chunk.World.Filer.StartSection("GenerateChunk");

                int xbc = chunk.CurrentChunkX << 4;
                int zbc = chunk.CurrentChunkY << 4;

                _chunkPrimer.Clear();

                // Шум для бедрока
                _noiseDown.GenerateNoise2d(_downNoise, xbc, zbc, 16, 16, 1, 1);
                // Доп шумы
                _noiseArea.GenerateNoise2d(AreaNoise, xbc, zbc, 16, 16, .4f, .4f);
                // Шумы речных пещер
                _noiseCave1.GenerateNoise2d(_caveRiversNoise, xbc, zbc, 16, 16, .05f, .05f);
                _noiseCaveHeight1.GenerateNoise2d(_caveHeightNoise, xbc, zbc, 16, 16, .025f, .025f);
                _noiseCave2.GenerateNoise2d(_caveRiversNoise2, xbc, zbc, 16, 16, .05f, .05f);
                _noiseCaveHeight2.GenerateNoise2d(_caveHeightNoise2, xbc, zbc, 16, 16, .025f, .025f);

                EnumBiomeIsland enumBiome;
                BiomeIsland biome = new BiomeIsland(this);
                byte idBiome;
                int xz, level;
                float dn;

                int[] arHeight = _genLayerHeight.GetInts(xbc, zbc, 16, 16);
                int[] arBiome = _genLayerBiome.GetInts(xbc, zbc, 16, 16);

                for (xz = 0; xz < 256; xz++)
                {
                    // Низ бедрок
                    dn = _downNoise[xz];
                    _chunkPrimer.SetBlockState(xz, 0, 2);
                    _chunkPrimer.SetBlockState(xz, 1, (ushort)(dn < .1 ? 2 : 3));
                    _chunkPrimer.SetBlockState(xz, 2, (ushort)(dn < -.1 ? 2 : 3));

                    // Биомы
                    idBiome = (byte)arBiome[xz];
                    enumBiome = (EnumBiomeIsland)idBiome;
                    chunk.Biome[xz] = idBiome;
                    _chunkPrimer.Biome[xz] = enumBiome;
                    biome.Init(_chunkPrimer, xbc, zbc);
                    level = biome.Column(xz, arHeight[xz]);

                    // Пещенры 2д ввиде рек
                    if (enumBiome == EnumBiomeIsland.Mountains
                        || (enumBiome == EnumBiomeIsland.MountainsDesert && level > 58))
                    {
                        // В горах и пустыных горах могут быть пещеры
                        _ColumnCave2d(_caveRiversNoise[xz] / 8f, _caveHeightNoise[xz] / 8f, xz, enumBiome,
                        .12f, .28f, 12.5f, 6f, 80f, 56);
                    }
                    if (enumBiome == EnumBiomeIsland.Mountains)
                    {
                        // В горах внизу может быть лава
                        _ColumnCave2d(_caveRiversNoise2[xz] / 8f, _caveHeightNoise2[xz] / 8f, xz, enumBiome,
                            .10f, .30f, 10f, 18f, 12f, 18);
                    }
                }

                _ExportChuck(chunk);

                //chunk.World.Filer.EndSectionLog(); // 0.3 мс
                //World.Filer.StartSection("GHM " + CurrentChunkX + "," + CurrentChunkY);
                chunk.Light.SetLightBlocks(_chunkPrimer.ArrayLightBlocks.ToArray());
                chunk.Light.GenerateHeightMap(); // 0.02 мс
                chunk.InitHeightMapGen();
                //World.Filer.EndSectionLog();
            }
            catch (Exception ex)
            {
                Logger.Crash(ex, "GenerateChunk xc:{0} yc:{1}", chunk.CurrentChunkX, chunk.CurrentChunkY);
            }
        }

        public void Populate(ChunkBase chunk)
        {
           // Debug.Burden(1.5f);
        }

        /// <summary>
        /// Экспортировать данные чанка с chunkPrimer в ChunkBase
        /// </summary>
        private void _ExportChuck(ChunkBase chunk)
        {
            ChunkStorage chunkStorage;
            int x, y, z, yc, ycb, y0;
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
                    ? _chunkPrimer.HeightMap[xz] : _countHeightBlock;
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
                            _chunkPrimer.SetBlockState(xz, y, BlocksRegMvk.Lava.IndexBlock); // лава
                            _chunkPrimer.ArrayLightBlocks.Add((uint)(y << 8 | xz));
                        }
                        else _chunkPrimer.SetBlockState(xz, y, 0); // воздух
                    }
                }
            }
        }
    }
}
