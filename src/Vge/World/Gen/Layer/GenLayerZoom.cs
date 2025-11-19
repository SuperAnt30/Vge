using System;

namespace Vge.World.Gen.Layer
{
    /// <summary>
    /// Увеличение
    /// </summary>
    public class GenLayerZoom : GenLayer
    {
        public GenLayerZoom(GenLayer parent) => _parent = parent;

        public override int[] GetInts(int areaX, int areaZ, int width, int height)
        {
            int px = areaX >> 1;
            int pz = areaZ >> 1;
            int pw = (width >> 1) + 2;
            int ph = (height >> 1) + 2;
            int[] arParent = _parent.GetInts(px, pz, pw, ph);
            int nw = pw - 1 << 1;
            int nh = ph - 1 << 1;
            int[] ar = new int[nw * nh];
            int x, z, idx;

            for (z = 0; z < ph - 1; z++)
            {
                idx = (z << 1) * nw;
                for (x = 0; x < pw - 1; x++)
                {
                    ar[idx] = ar[idx + nw] = ar[idx + 1] = ar[idx + 1 + nw] = arParent[x + z * pw];
                    idx += 2;
                }
            }

            int[] ar2 = new int[width * height];

            for (idx = 0; idx < height; ++idx)
            {
                Array.Copy(ar, (idx + (areaZ & 1)) * nw + (areaX & 1), ar2, idx * width, width);
            }

            return ar2;
        }
    }
}
