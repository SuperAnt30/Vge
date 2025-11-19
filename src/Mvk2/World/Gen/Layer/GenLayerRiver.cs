using Mvk2.World.Biome;
using System.Runtime.CompilerServices;
using Vge.World.Gen.Layer;

namespace Mvk2.World.Gen.Layer
{
    /// <summary>
    /// Создание реки
    /// </summary>
    public class GenLayerRiver : GenLayer
    {
        private readonly int _idRiver = (int)EnumBiomeIsland.River;
        private readonly int _idSwamp = (int)EnumBiomeIsland.Swamp;
        private readonly int _idConiferousForest = (int)EnumBiomeIsland.ConiferousForest;

        public GenLayerRiver(GenLayer parent) => _parent = parent;

        public override int[] GetInts(int areaX, int areaZ, int width, int height)
        {
            int px = areaX - 1;
            int pz = areaZ - 1;
            int pw = width + 2;
            int ph = height + 2;
            int[] arParent = _parent.GetInts(px, pz, pw, ph);
            int[] ar = new int[width * height];
            int x, z, idx;
            bool c11;

            for (z = 0; z < height; z++)
            {
                for (x = 0; x < width; x++)
                {
                    idx = arParent[x + 1 + (z + 1) * pw];
                    c11 = _Check(idx);

                    ar[x + z * width] = (c11 == _Check(arParent[x + 0 + (z + 1) * pw])
                        && c11 == _Check(arParent[x + 1 + (z + 0) * pw])
                        && c11 == _Check(arParent[x + 2 + (z + 1) * pw])
                        && c11 == _Check(arParent[x + 1 + (z + 2) * pw])) ? idx : _idRiver;
                }
            }

            return ar;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _Check(int idx) => idx == _idConiferousForest || idx == _idSwamp;
    }
}
