namespace Vge.World.Gen.Layer
{
    /// <summary>
    /// Абстрактный класс по добавлении высот
    /// </summary>
    public abstract class GenLayerHeightAddParam : GenLayer
    {
        public GenLayerHeightAddParam(long baseSeed, GenLayer parent) : base(baseSeed) 
            => _parent = parent;

        public override int[] GetInts(int areaX, int areaZ, int width, int height)
        {
            int[] arParent = _parent.GetInts(areaX, areaZ, width, height);
            int count = width * height;
            int[] ar = new int[count];
            int x, z, idx, zw;

            for (z = 0; z < height; z++)
            {
                zw = z * width;
                for (x = 0; x < width; x++)
                {
                    idx = x + zw;
                    InitChunkSeed(x + areaX, z + areaZ);
                    ar[idx] = _GetParam(arParent[idx]);
                }
            }
            return ar;
        }

        protected virtual int _GetParam(int param) => param;
    }
}
