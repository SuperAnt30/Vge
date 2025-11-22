using Mvk2.World.Biome;
using System;
using Vge.Util;
using Vge.World.Gen.Layer;

namespace Mvk2.World.Gen.Layer
{
    /// <summary>
    /// Объект генерации острова слоями
    /// </summary>
    public class GenLayerIsland : GenLayer
    {
        /// <summary>
        /// массив стартового шаблона мира
        /// </summary>
        private readonly byte[] _arPattern = new byte[] {
             // 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3, 4, 5, 6, 7,
/*  0 */        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
/*  1 */        0, 6, 6, 9, 6, 6, 9, 9, 2, 0, 2, 0, 2, 0, 2, 2, 0, 0, 0, 2, 0, 0, 0, 0, 0, 2, 2, 0, 2, 5, 5, 0,
/*  2 */        0, 6, 9, 9, 9, 9, 9, 9, 0, 2, 0, 2, 0, 2, 0, 0, 2, 0, 2, 0, 2, 0, 0, 0, 0, 2, 2, 2, 5, 5, 5, 0,
/*  3 */        0, 6, 6, 9, 9, 9, 0, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 7, 0, 2, 0, 0, 2, 2, 2, 2, 2, 5, 5, 0,
/*  4 */        0, 0, 6, 9, 9, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2, 0, 2, 0, 0, 2, 2, 2, 2, 2, 2, 0,
/*  5 */        0, 0, 0, 6, 9, 9, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2, 0, 2, 0, 0, 2, 2, 0, 0,
/*  6 */        0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 6, 6, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0,
/*  7 */        0, 0, 0, 0, 0, 0, 0, 2, 0, 2, 2, 2, 9, 9, 9, 9, 6, 6, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0,
/*  8 */        0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 9, 9, 9, 9, 9, 2, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0,
/*  9 */        0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 9, 9, 9, 9, 9, 9, 2, 6, 6, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0,
/* 10 */        0, 0, 0, 0, 0, 0, 0, 2, 2, 7, 2, 2, 5, 9, 9, 9, 9, 2, 9, 6, 6, 6, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0,
/* 11 */        0, 0, 0, 0, 0, 0, 0, 2, 7, 7, 7, 7, 5, 5, 2, 9, 9, 6, 2, 6, 6, 6, 2, 0, 0, 0, 0, 0, 2, 2, 2, 0,
/* 12 */        0, 0, 0, 0, 0, 0, 2, 2, 7, 7, 7, 7, 2, 2, 2, 2, 6, 6, 6, 2, 6, 2, 2, 2, 0, 0, 0, 0, 0, 2, 2, 0,
/* 13 */        0, 0, 0, 0, 0, 0, 0, 2, 2, 7, 7, 7, 7, 5, 2, 2, 6, 6, 6, 6, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 2, 0,
/* 14 */        0, 0, 0, 0, 0, 0, 2, 0, 2, 2, 7, 7, 7, 7, 5, 2, 6, 6, 6, 6, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0,
/* 15 */        0, 0, 0, 0, 0, 2, 0, 0, 2, 2, 5, 7, 5, 5, 5,10, 2, 2, 6, 5, 5, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0,
/*  0 */        0, 0, 0, 0, 0, 0, 2, 0, 2, 2, 2, 5, 5, 2,10,10,10,10, 2, 5, 2, 5, 5, 5, 2, 0, 0, 0, 0, 0, 0, 0,
/*  1 */        0, 0, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2,10,10,10,10,10,10, 5, 5, 5, 5, 2, 2, 2, 0, 0, 0, 0, 0, 0,
/*  2 */        0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 2, 2, 2, 2,10,10,10,10,10,10,10, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0,
/*  3 */        0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 2, 2, 2, 2,10, 2,10,10,10,10,10, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0,
/*  4 */        0, 2, 2, 2,10, 2, 0, 0, 0, 0, 0, 2, 2, 2,10, 2, 2,10,10,10,10, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
/*  5 */        0, 2,10,10,10,10, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0,10,10,10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
/*  6 */        0, 2,10,10,10,10, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
/*  7 */        0, 2, 0, 0,10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3, 0, 0, 0, 0, 0, 0, 0,
/*  8 */        0, 0, 2,10,10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 3,11,11,11, 3, 0, 3, 3, 3, 0, 0, 0, 0, 0, 0,
/*  9 */        0, 2, 0, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 8, 3, 3,11,11,11, 3, 3, 3, 3, 0, 0, 0, 0, 0, 0,
/* 10 */        0, 0, 2, 0, 0, 0, 3, 3, 3, 0, 0, 3, 0, 0, 3, 3, 8, 8, 3, 3,11, 3, 3, 3, 3, 0, 3, 0, 0, 0, 0, 0,
/* 11 */        0, 3, 0, 0, 0, 3, 3, 3, 3, 3, 3, 0, 0, 3, 0, 3, 8, 8, 8, 8, 3, 3, 3, 3, 3, 3, 0, 0, 0, 0, 0, 0,
/* 12 */        0, 3, 3, 0, 3,11,11, 3, 3, 3, 0, 0, 0, 0, 0, 0, 8, 8, 8, 8, 3, 3, 3, 0, 0, 0, 0, 3, 0, 0, 8, 0,
/* 13 */        0, 0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 0, 0, 0, 0, 0, 8, 8, 8, 3, 3, 3, 0, 0, 0, 0, 0, 3, 0, 8, 8, 0,
/* 14 */        0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 0, 3, 0, 0, 0, 0, 0, 8, 3, 3, 3, 0, 0, 0, 0, 0, 0, 0, 8, 8, 8, 0,
/* 15 */        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };

        public GenLayerIsland(long worldSeed)
        {
        }

        public override int[] GetInts(int areaX, int areaZ, int width, int height)
        {
            int[] ar = new int[height * width];
            int x, z, x2, z2;

            try
            {
                for (z = 0; z < height; z++)
                {
                    for (x = 0; x < width; x++)
                    {
                        x2 = areaX + x;
                        z2 = areaZ + z;
                        if (x2 > -15 && x2 < 16 && z2 > -15 && z2 < 16)
                        {
                            ar[z * width + x] = _arPattern[(z2 + 15) << 5 | (x2 + 15)];
                        }
                    }
                }

                return ar;
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
                return new int[0];
            }
        }

        public static GenLayer[] BeginLayerBiome(long worldSeed)
        {
            GenLayer layerBiomeOne;
            GenLayer layerBiomeParam1;
            GenLayer layerBiomeParam2;
            GenLayer layerBiome;
            GenLayer layerHeight;

            layerBiomeOne = new GenLayerIsland(worldSeed);
            layerBiomeOne = new GenLayerZoom(layerBiomeOne);
            // Этот слой если увеличить биомы в 2 раза
            layerBiomeOne = new GenLayerZoomRandom(71, layerBiomeOne);
            // Река и пляж
            layerBiomeOne = new GenLayerShore(new GenLayerRiver(layerBiomeOne));

            int idRiver = (int)EnumBiomeIsland.River;
            int idSea = (int)EnumBiomeIsland.Sea;

            // Biome
            layerBiome = new GenLayerZoomRandom(21, layerBiomeOne);
            // Расширяем биом реки, для бесконечного источника воды
            layerBiome = new GenLayerExpand(layerBiome, idRiver);
            layerBiome = new GenLayerExpand(layerBiome, idSea);
            layerBiome = new GenLayerZoomRandom(24, layerBiome);
            // Расширяем биом реки, для бесконечного источника воды
            layerBiome = new GenLayerExpand(layerBiome, idRiver);
            layerBiome = new GenLayerExpand(layerBiome, idSea);
            layerBiome = new GenLayerSmooth(15, layerBiome);
            layerBiome = layerBiomeParam1 = new GenLayerZoomRandom(15, layerBiome);
            layerBiome = new GenLayerZoomRandom(25, layerBiome);
            layerBiome = new GenLayerSmooth(7, layerBiome);
            // Расширяем биом реки, для бесконечного источника воды
            layerBiome = new GenLayerExpand(layerBiome, idRiver);
            layerBiome = layerBiomeParam2 = new GenLayerSmooth(17, layerBiome);
            // Расширяем биом реки, для бесконечного источника воды
            layerBiome = new GenLayerExpand(layerBiome, idRiver);
            layerBiome = new GenLayerZoomRandom(23, layerBiome);
            layerBiome = new GenLayerSmooth(2, layerBiome);
            // EndBiome

            //// Height
            //layerHeight = new GenLayerBiomeHeight(layerBiomeOne);
            //layerHeight = new GenLayerHeightAddBegin(2, layerHeight);
            //layerHeight = new GenLayerSmoothMix(new GenLayerZoomRandom(21, layerHeight));
            //// Объект по добавлении высот (неровности вверх)
            //layerHeight = new GenLayerHeightAddUp(2, layerHeight);
            //layerHeight = new GenLayerSmoothMix(new GenLayerZoomRandom(24, layerHeight));
            //// добавлении высот (ущелены в море и не большие неровности)
            //layerHeight = new GenLayerHeightAddSea(100, layerHeight);
            //layerHeight = new GenLayerSmoothMix(new GenLayerZoomRandom(15, new GenLayerSmooth(15, layerHeight)));
            //// Добавляем на гора, болоте и лесу неровности
            //layerHeight = new GenLayerHeightAddBiome(200, layerHeight, layerBiomeParam1, true);
            //layerHeight = new GenLayerSmoothMix(new GenLayerZoomRandom(25, layerHeight));
            //// Добавляем на гора, болоте неровности, чтоб более острее
            //layerHeight = new GenLayerHeightAddBiome(300, layerHeight, layerBiomeParam2, false);
            //layerHeight = new GenLayerSmooth(7, layerHeight);
            //layerHeight = new GenLayerSmooth(17, layerHeight);
            //layerHeight = new GenLayerSmoothMix(new GenLayerZoomRandom(23, layerHeight));
            //layerHeight = new GenLayerSmooth(2, layerHeight);
            //// EndHeight

            //layerBiome.InitWorldGenSeed(worldSeed);
            //layerHeight.InitWorldGenSeed(worldSeed);

            //return new GenLayer[] { layerBiome, layerHeight };
            return new GenLayer[] { layerBiome, layerBiomeOne };
            
        }
    }
}
