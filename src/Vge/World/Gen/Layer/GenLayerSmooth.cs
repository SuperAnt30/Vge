namespace Vge.World.Gen.Layer
{
    /// <summary>
    /// Эфект гладкости, с помощью рандома для биомов
    /// </summary>
    public class GenLayerSmooth : GenLayer
    {
        public GenLayerSmooth(long baseSeed, GenLayer parent) : base(baseSeed) => _parent = parent;

        public override int[] GetInts(int areaX, int areaZ, int width, int height)
        {
            int px = areaX - 1;
            int pz = areaZ - 1;
            int pw = width + 2;
            int ph = height + 2;
            int[] arParent = _parent.GetInts(px, pz, pw, ph);
            int[] ar = new int[width * height];
            int x, z, zpw;
            int c01, c21, c10, c12, c11;
            for (z = 0; z < height; z++)
            {
                zpw = (z + 1) * pw;
                for (x = 0; x < width; x++)
                {
                    c01 = arParent[x + zpw];
                    c21 = arParent[x + 2 + zpw];
                    c10 = arParent[x + 1 + z * pw];
                    c12 = arParent[x + 1 + (z + 2) * pw];
                    c11 = arParent[x + 1 + zpw];

                    if (c01 == c21 && c10 == c12)
                    {
                        InitChunkSeed(x + areaX, z + areaZ);
                        c11 = _NextInt(2) == 0 ? c01 : c10;
                    }
                    else
                    {
                        if (c01 == c21) c11 = c01;
                        if (c10 == c12) c11 = c10;
                    }

                    ar[x + z * width] = c11;
                }
            }

            return ar;
        }
    }
}
