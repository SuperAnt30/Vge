using System;
using System.Runtime.CompilerServices;

namespace Vge.World.Gen.Layer
{
    /// <summary>
    /// Увеличение со случайными соседями
    /// </summary>
    public class GenLayerZoomRandom : GenLayer
    {
        public GenLayerZoomRandom(long baseSeed, GenLayer parent) : base(baseSeed)
            => _parent = parent;

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
            int x, z, idx, old1, old2, old3, old4;

            for (z = 0; z < ph - 1; z++)
            {
                idx = (z << 1) * nw;
                old1 = arParent[z * pw];
                old2 = arParent[(z + 1) * pw];
                for (x = 0; x < pw - 1; x++)
                {
                    InitChunkSeed((x + px) << 1, (z + pz) << 1);
                    old3 = arParent[x + 1 + z * pw];
                    old4 = arParent[x + 1 + (z + 1) * pw];
                    ar[idx] = old1;
                    ar[idx + nw] = _SelectRandom(old1, old2);
                    ar[idx + 1] = _SelectRandom(old1, old3);
                    ar[idx + 1 + nw] = _SelectModeOrRandom(old1, old3, old2, old4);
                    idx += 2;
                    old1 = old3;
                    old2 = old4;
                }
            }

            int[] ar2 = new int[width * height];

            for (idx = 0; idx < height; ++idx)
            {
                Array.Copy(ar, (idx + (areaZ & 1)) * nw + (areaX & 1), ar2, idx * width, width);
            }

            return ar2;
        }

        /// <summary>
        /// Выбирает случайное целое число из набора предоставленных целых чисел
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int _SelectRandom(params int[] args) => args[_NextInt(args.Length)];

        /// <summary>
        /// Возвращает наиболее часто встречающееся число набора или случайное число из предоставленных
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int _SelectModeOrRandom(int param1, int param2, int param3, int param4)
        {
            return param2 == param3 && param3 == param4
                ? param2 : (param1 == param2 && param1 == param3
                    ? param1 : (param1 == param2 && param1 == param4
                        ? param1 : (param1 == param3 && param1 == param4
                            ? param1 : (param1 == param2 && param3 != param4
                                ? param1 : (param1 == param3 && param2 != param4
                                    ? param1 : (param1 == param4 && param2 != param3
                                        ? param1 : (param2 == param3 && param1 != param4
                                            ? param2 : (param2 == param4 && param1 != param3
                                                ? param2 : (param3 == param4 && param1 != param2
                                                    ? param3 : _SelectRandom(param1, param2, param3, param4)
                                                )))))))));
        }
    }
}
