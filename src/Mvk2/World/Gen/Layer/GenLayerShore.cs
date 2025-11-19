using Mvk2.World.Biome;
using System.Runtime.CompilerServices;
using Vge.World.Gen.Layer;

namespace Mvk2.World.Gen.Layer
{
    /// <summary>
    /// Создание берега
    /// </summary>
    public class GenLayerShore : GenLayer
    {
        private readonly int _idPlain = (int)EnumBiomeIsland.Plain;
        private readonly int _idDesert = (int)EnumBiomeIsland.Desert;
        private readonly int _idSea = (int)EnumBiomeIsland.Sea;
        private readonly int _idBeach = (int)EnumBiomeIsland.Beach;

        public GenLayerShore(GenLayer parent) => _parent = parent;

        public override int[] GetInts(int areaX, int areaZ, int width, int height)
        {
            int px = areaX - 1;
            int pz = areaZ - 1;
            int pw = width + 2;
            int ph = height + 2;
            int[] arParent = _parent.GetInts(px, pz, pw, ph);
            int[] ar = new int[width * height];
            int x, z, idx;

            for (z = 0; z < height; z++)
            {
                for (x = 0; x < width; x++)
                {
                    idx = arParent[x + 1 + (z + 1) * pw];
                    if (idx == _idPlain || idx == _idDesert)
                    {
                        ar[x + z * width] = arParent[x + 0 + (z + 1) * pw] == _idSea
                            || arParent[x + 2 + (z + 1) * pw] == _idSea
                            || arParent[x + 1 + (z + 0) * pw] == _idSea
                            || arParent[x + 1 + (z + 2) * pw] == _idSea ? _idBeach : idx;
                    }
                    else if (idx == _idSea)
                    {
                        ar[x + z * width] = _Check(arParent[x + 0 + (z + 1) * pw])
                            || _Check(arParent[x + 2 + (z + 1) * pw])
                            || _Check(arParent[x + 1 + (z + 0) * pw])
                            || _Check(arParent[x + 1 + (z + 2) * pw]) ? _idBeach : idx;
                    }
                    else
                    {
                        ar[x + z * width] = idx;
                    }
                }
            }
            return ar;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _Check(int idx) => idx == _idPlain || idx == _idDesert;
    }
}
