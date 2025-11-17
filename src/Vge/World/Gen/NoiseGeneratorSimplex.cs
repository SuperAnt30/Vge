using System.Runtime.CompilerServices;
using Vge.Util;

namespace Vge.World.Gen
{
    /// <summary>
    /// Объект обработки шума Перлина одной актавы
    /// </summary>
    public class NoiseGeneratorSimplex
    {
        private readonly int[] _permutations = new int[512];
        private readonly float _xCoord;
        private readonly float _yCoord;
        private readonly float _zCoord;

        private static readonly float[] _arStat1 = new float[] { 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, -1.0f, 0.0f };
        private static readonly float[] _arStat2 = new float[] { 1.0f, 1.0f, -1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f };
        private static readonly float[] _arStat3 = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, -1.0f, -1.0f, 1.0f, 1.0f, -1.0f, -1.0f, 0.0f, 1.0f, 0.0f, -1.0f };
        private static readonly float[] _arStat4 = new float[] { 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, -1.0f, 0.0f };
        private static readonly float[] _arStat5 = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, -1.0f, -1.0f, 1.0f, 1.0f, -1.0f, -1.0f, 0.0f, 1.0f, 0.0f, -1.0f };

        public NoiseGeneratorSimplex() : this(new Rand()) { }
        public NoiseGeneratorSimplex(Rand random) 
        {
            _xCoord = random.NextFloat() * 256f;
            _yCoord = random.NextFloat() * 256f;
            _zCoord = random.NextFloat() * 256f;
            int i, r, old;

            for (i = 0; i < 256; _permutations[i] = i++) ;

            for (i = 0; i < 256; i++)
            {
                r = random.Next(256 - i) + i;
                old = _permutations[i];
                _permutations[i] = _permutations[r];
                _permutations[r] = old;
                _permutations[i + 256] = _permutations[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Lerp(float f1, float f2, float f3) => f2 + f1 * (f3 - f2);

        /// <summary>
        /// Умнажаем первые значения с массива и суммируем их
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Multiply(int index, float x, float y)
        {
            int i = index & 15;
            return _arStat4[i] * x + _arStat5[i] * y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Grad(int index, float x, float y, float z)
        {
            int i = index & 15;
            return _arStat1[i] * x + _arStat2[i] * y + _arStat3[i] * z;
        }

        /// <summary>
        /// Генерация шума объёма в массив
        /// </summary>
        /// <param name="noiseArray">массив</param>
        /// <param name="xOffset">координата Х</param>
        /// <param name="yOffset">координата Y</param>
        /// <param name="zOffset">координата Z</param>
        /// <param name="xSize">ширина по Х</param>
        /// <param name="ySize">ширина по Y</param>
        /// <param name="zSize">ширина по Z</param>
        /// <param name="xScale">масштаб по X</param>
        /// <param name="yScale">масштаб по Y</param>
        /// <param name="zScale">масштаб по Z</param>
        /// <param name="noiseScale">масштаб шума</param>
        public void PopulateNoiseArray3d(float[] noiseArray, float xOffset, float yOffset, float zOffset,
            int xSize, int ySize, int zSize, float xScale, float yScale, float zScale, float noiseScale)
        {
            float noise = 1.0f / noiseScale;
            int count = 0;

            int x, y, z, xi, xa, zi, za, yi, ya, p1, p2, p3, p4, p5, p6;
            float xd, yd, zd, xr, yr, zr, ler5, ler6, ler7, f1, f2, f3;

            float ler1 = 0.0f;
            float ler2 = 0.0f;
            float ler3 = 0.0f;
            float ler4 = 0.0f;

            for (x = 0; x < xSize; x++)
            {
                xd = xOffset + x * xScale + _xCoord;
                xi = (int)xd;
                if (xd < xi) xi--;
                xa = xi & 255;
                xd -= xi;
                xr = xd * xd * xd * (xd * (xd * 6.0f - 15.0f) + 10.0f);

                for (z = 0; z < zSize; z++)
                {
                    zd = zOffset + z * zScale + _zCoord;
                    zi = (int)zd;
                    if (zd < zi) zi--;
                    za = zi & 255;
                    zd -= zi;
                    zr = zd * zd * zd * (zd * (zd * 6.0f - 15.0f) + 10.0f);

                    for (y = 0; y < ySize; ++y)
                    {
                        yd = yOffset + y * yScale + _yCoord;
                        yi = (int)yd;
                        if (yd < yi) yi--;
                        ya = yi & 255;
                        yd -= yi;
                        yr = yd * yd * yd * (yd * (yd * 6.0f - 15.0f) + 10.0f);

                        if (y == 0 || ya >= 0)
                        {
                            p1 = _permutations[xa] + ya;
                            p2 = _permutations[p1] + za;
                            p3 = _permutations[p1 + 1] + za;
                            p4 = _permutations[xa + 1] + ya;
                            p5 = _permutations[p4] + za;
                            p6 = _permutations[p4 + 1] + za;
                            f1 = xr;
                            f2 = Grad(_permutations[p2], xd, yd, zd);
                            f3 = Grad(_permutations[p5], xd - 1, yd, zd);
                            ler1 = f2 + f1 * (f3 - f2);
                            f2 = Grad(_permutations[p3], xd, yd - 1, zd);
                            f3 = Grad(_permutations[p6], xd - 1, yd - 1, zd);
                            ler2 = f2 + f1 * (f3 - f2);
                            f2 = Grad(_permutations[p2 + 1], xd, yd, zd - 1);
                            f3 = Grad(_permutations[p5 + 1], xd - 1, yd, zd - 1);
                            ler3 = f2 + f1 * (f3 - f2);
                            f2 = Grad(_permutations[p3 + 1], xd, yd - 1, zd - 1);
                            f3 = Grad(_permutations[p6 + 1], xd - 1, yd - 1, zd - 1);
                            ler4 = f2 + f1 * (f3 - f2);
                        }
                        ler5 = ler1 + yr * (ler2 - ler1);
                        ler6 = ler3 + yr * (ler4 - ler3);
                        ler7 = ler5 + zr * (ler6 - ler5);

                        noiseArray[count] += ler7 * noise;
                        count++;
                    }
                }
            }
        }

        /// <summary>
        /// Генерация шума плоскости в массиве
        /// </summary>
        /// <param name="noiseArray">массив</param>
        /// <param name="xOffset">координата Х</param>
        /// <param name="zOffset">координата Z</param>
        /// <param name="xSize">ширина по Х</param>
        /// <param name="zSize">ширина по Z</param>
        /// <param name="xScale">масштаб по X</param>
        /// <param name="zScale">масштаб по Z</param>
        /// <param name="noiseScale">масштаб шума</param>
        public void PopulateNoiseArray2d(float[] noiseArray, float xOffset, float zOffset,
            int xSize, int zSize, float xScale, float zScale, float noiseScale)
        {
            float noise = 1.0f / noiseScale;
            int count = 0;

            int x, z, xi, xa, zi, za, p1, p2, p3, p4;
            float xd, zd, xr, zr, ler1, ler2, ler3, f1, f2, f3;

            for (x = 0; x < xSize; x++)
            {
                xd = xOffset + x * xScale + _xCoord;
                xi = (int)xd;
                if (xd < xi) xi--;
                xa = xi & 255;
                xd -= xi;
                xr = xd * xd * xd * (xd * (xd * 6.0f - 15.0f) + 10.0f);

                for (z = 0; z < zSize; z++)
                {
                    zd = zOffset + z * zScale + _zCoord;
                    zi = (int)zd;
                    if (zd < zi) zi--;
                    za = zi & 255;
                    zd -= zi;
                    zr = zd * zd * zd * (zd * (zd * 6.0f - 15.0f) + 10.0f);
                    p1 = _permutations[xa] + 0;
                    p2 = _permutations[p1] + za;
                    p3 = _permutations[xa + 1] + 0;
                    p4 = _permutations[p3] + za;
                    f1 = xr;
                    f2 = Multiply(_permutations[p2], xd, zd);
                    f3 = Grad(_permutations[p4], xd - 1.0f, 0.0f, zd);
                    ler1 = f2 + f1 * (f3 - f2);
                    f2 = Grad(_permutations[p2 + 1], xd, 0.0f, zd - 1.0f);
                    f3 = Grad(_permutations[p4 + 1], xd - 1.0f, 0.0f, zd - 1.0f);
                    ler2 = f2 + f1 * (f3 - f2);
                    ler3 = ler1 + zr * (ler2 - ler1);
                    noiseArray[count] += ler3 * noise;
                    count++;
                }
            }
        }
    }
}
