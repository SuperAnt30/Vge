using Mvk2.World.Biome;
using Vge.World.Gen.Layer;

namespace Mvk2.World.Gen.Layer
{
    /// <summary>
    /// По добавляем дополнительные высоты в определённых биомах
    /// </summary>
    public class GenLayerHeightAddBiome : GenLayer
    {
        private readonly int _idBirchForest = (int)EnumBiomeIsland.BirchForest;
        private readonly int _idMountains = (int)EnumBiomeIsland.Mountains;
        private readonly int _idSwamp = (int)EnumBiomeIsland.Swamp;
        private readonly bool _isForest = false;

        private readonly GenLayer _layerBiome;

        public GenLayerHeightAddBiome(long baseSeed, GenLayer parent, GenLayer layerBiome, bool isForest) 
            : base(baseSeed)
        {
            _parent = parent;
            _layerBiome = layerBiome;
            _isForest = isForest;
        }

        public override int[] GetInts(int areaX, int areaZ, int width, int height)
        {
            int[] arBiome = _layerBiome.GetInts(areaX, areaZ, width, height);
            int[] arParent = _parent.GetInts(areaX, areaZ, width, height);
            int count = width * height;
            int[] ar = new int[count];
            int x, z, idx, c, b;

            for (z = 0; z < height; z++)
            {
                for (x = 0; x < width; x++)
                {
                    idx = x + z * width;
                    c = arParent[idx];
                    b = arBiome[idx];

                    if (b == _idSwamp)
                    {
                        InitChunkSeed(x + areaX, z + areaZ);
                        c += _NextInt(3) - 1;
                    }
                    else if (b == _idMountains)
                    {
                        InitChunkSeed(x + areaX, z + areaZ);
                        //c += _NextInt(9) - 4; //Old version
                        c += _NextInt(5) - 2;
                    }
                    else if (_isForest && b == _idBirchForest)
                    {
                        InitChunkSeed(x + areaX, z + areaZ);
                        c += _NextInt(5) - 2;
                    }
                    ar[idx] = c;
                }
            }
            return ar;
        }
    }
}
