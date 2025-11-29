using Mvk2.World.Gen;
using Mvk2.World.Gen.Feature;
using System;
using Vge;
using Vge.Util;
using Vge.World.Gen;
using WinGL.Util;

namespace Mvk2.World.Biome
{
    public class BiomePlane : BiomeIsland
    {

        public BiomePlane(ChunkProviderGenerateIsland chunkProvider)
            : base(chunkProvider)
        {

            _featureColumnsAfter = new IFeatureGeneratorColumn[]
            {
                new FeatureCactus(_chunkPrimer, 5)
            };
        }

        /// <summary>
        /// Возращаем сгенерированный столбец и возвращает фактическую высоту, без воды
        /// </summary>
        /// <param name="xz">z << 4 | x</param>
        /// <param name="height">Высота в блоках, средняя рекомендуемая</param>
        public override int ReliefColumn(int xz, int height)
        {
            int yh = height;
            if (yh < 2) yh = 2;
            int result = _chunkPrimer.HeightMap[xz] = yh;
            int y = 0;
            // Смещение от уровня моря
            int biasWater = yh - HeightWater;

            try
            {
                // Бедрок
                int level1 = (int)(Provider.CaveRiversNoise[xz] / 5f) + 2; // ~ 0 .. 3
                if (level1 > 0)
                {
                    for (y = 0; y < level1; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdBedrock);
                }

                // Известняк
                int level2 = (int)(Provider.CaveRiversNoise[xz] * 1.5f) + 28 + biasWater; // ~ 18 .. 38
                if (level2 > level1)
                {
                   for (y = level1; y < level2; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLimestone);
                }

                // Низ песка
                int level3 = (int)(Provider.SandDownNoise[xz] * 1.25f) + 36 + biasWater; // ~ 27 .. 45
                if (level3 > yh) level3 = yh;
                // Низ суглинка
                int level4 = (int)(Provider.LoamDownNoise[xz] * 1.25f) + 36 + biasWater; // ~ 27 .. 45
                if (level4 > yh) level4 = yh;

                // Глина
                int level = Mth.Min(level3, level4);
                if (level > level2)
                {
                    for (y = level2; y < level; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdClay);
                }

                if (level4 > level3)
                {
                    // Местами прослойки песка между глиной и суглинком
                    for (y = level3; y < level4; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdSand);
                }

                // Вверх песка
                int level5 = (int)Provider.SandUpNoise[xz] + 42 + biasWater; // ~ 34 .. 51
                if (level5 > yh) level5 = yh;
                // Вверх суглинка
                int level6 = (int)Provider.LoamUpNoise[xz] + 48 + biasWater; // ~ 40 .. 57
                if (level6 > yh) level6 = yh;

                if (level6 > level4)
                {
                    // Суглинок
                    for (y = level4; y < level6; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLoam);
                    // может дёрн
                    if (level6 == yh) _chunkPrimer.SetBlockState(xz, y, Block.BlocksRegMvk.Brol.IndexBlock);
                }

                if (level5 > level6)
                {
                    // Местами прослойки песка между вверхном и суглинком
                    for (y = level6; y < level5; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdSand);
                    // Сверху песок
                    if (level5 == yh) _chunkPrimer.SetBlockState(xz, yh, _blockIdSand);
                    level = level5;
                }
                else level = level6;

                if (yh > level)
                {
                    for (y = level; y < yh; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdHumus);
                    _chunkPrimer.SetBlockState(xz, yh, _blockIdUp);
                }

                //int bodyHeight = (int)(level2);
                if (yh < HeightWater)
                {
                    // меньше уровня воды
                    _chunkPrimer.SetBlockState(xz, HeightWater, _blockIdWater);
                    //yh++;
                    //for (y = yh; y < _heightWaterPlus; y++)
                    //{
                    //    // заполняем водой
                    //   // _chunkPrimer.SetBlockState(xz, y, _blockIdWater);
                    //}
                }
            }
            catch (Exception ex)
            {
                Logger.Crash(ex, "Biome.Column yh:{0} y:{1} xz:{2}", yh, y, xz);
            }
            
            return result;
        }

    }
}
