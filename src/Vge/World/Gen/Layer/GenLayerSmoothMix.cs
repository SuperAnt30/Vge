namespace Vge.World.Gen.Layer
{
    /// <summary>
    /// Эфект гладкости, смешать соседние в средний, для высот рельефа
    /// </summary>
    public class GenLayerSmoothMix : GenLayer
    {
        public GenLayerSmoothMix(GenLayer parent) => _parent = parent;

        public override int[] GetInts(int areaX, int areaZ, int width, int height)
        {
            int px = areaX - 1;
            int pz = areaZ - 1;
            int pw = width + 2;
            int ph = height + 2;
            int[] arParent = _parent.GetInts(px, pz, pw, ph);
            int[] ar = new int[width * height];
            int x, z, c, zpw;


            for (z = 0; z < height; z++)
            {
                zpw = (z + 1) * pw;
                for (x = 0; x < width; x++)
                {
                    c = arParent[x + 1 + zpw];
                    c += arParent[x + zpw];
                    c += arParent[x + 2 + zpw];
                    c += arParent[x + 1 + z * pw];
                    c += arParent[x + 1 + (z + 2) * pw];
                    ar[x + z * width] = c / 5;
                }
            }

            return ar;
        }
    }
}
