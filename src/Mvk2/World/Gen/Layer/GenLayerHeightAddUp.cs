using Mvk2.World.Biome;
using Vge.World.Gen.Layer;
using WinGL.Util;

namespace Mvk2.World.Gen.Layer
{
    /// <summary>
    /// Объект по добавлении высот (неровности вверх)
    /// </summary>
    public class GenLayerHeightAddUp : GenLayer
    {
        private readonly int _idDesert = (int)EnumBiomeIsland.Desert;

        private readonly GenLayer _layerBiome;

        public GenLayerHeightAddUp(long baseSeed, GenLayer parent, GenLayer layerBiome) : base(baseSeed)
        {
            _parent = parent;
            _layerBiome = layerBiome;
        }

        public override int[] GetInts(int areaX, int areaZ, int width, int height)
        {
            int[] arBiome = _layerBiome.GetInts(areaX, areaZ, width, height);
            int[] arParent = _parent.GetInts(areaX, areaZ, width, height);
            int count = width * height;
            int[] ar = new int[count];
            int x, z, idx, zw, c, b, r;

            for (z = 0; z < height; z++)
            {
                zw = z * width;
                for (x = 0; x < width; x++)
                {
                    idx = x + zw;
                    b = arBiome[idx];
                    c = arParent[idx];
                    InitChunkSeed(x + areaX, z + areaZ);
                    if (c < 45)
                    {
                        // Ниже воды 
                        r = (45 - c) / 2;
                        if (r > 0) c += Mth.Min(_NextInt(r), _NextInt(r));
                    }
                    else if (c > 46)
                    {
                        // Выше воды
                        if (b == _idDesert)
                        {
                            c = 50;
                        }
                        else
                        {
                            r = c - 45;
                            if (r > 0) c += _NextInt(r);
                        }
                    }

                    ar[idx] = c;
                }
            }
            return ar;
        }
    }
}
