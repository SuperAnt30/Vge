using Vge.Util;

namespace Vge.World.Gen
{
    /// <summary>
    /// Объект шума Перлина
    /// </summary>
    public class NoiseGeneratorPerlin
    {
        /// <summary>
        /// Сборник функций генерации шума. Выходной сигнал комбинируется для получения различных октав шума. 
        /// </summary>
        private readonly NoiseGeneratorSimplex[] _generatorCollection;
        /// <summary>
        /// Количество октав
        /// </summary>
        private readonly int _octaves;

        public NoiseGeneratorPerlin(Rand random, int octave)
        {
            _octaves = octave;
            _generatorCollection = new NoiseGeneratorSimplex[octave];

            for (int i = 0; i < octave; i++)
            {
                _generatorCollection[i] = new NoiseGeneratorSimplex(random);
            }
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
        /// <returns>вернёт массив noiseArray</returns>
        public float[] GenerateNoise3d(float[] noiseArray, int xOffset, int yOffset, int zOffset,
            int xSize, int ySize, int zSize, float xScale, float yScale, float zScale)
        {
            int i;
            if (noiseArray == null)
            {
                noiseArray = new float[xSize * ySize * zSize];
            }
            else
            {
                for (i = 0; i < noiseArray.Length; i++)
                {
                    noiseArray[i] = 0f;
                }
            }
            float d = 1f;
            float x, y, z;
            for (i = 0; i < _octaves; i++)
            {
                x = xOffset * d * xScale;
                y = yOffset * d * yScale;
                z = zOffset * d * zScale;
                _generatorCollection[i].PopulateNoiseArray3d(noiseArray, x, y, z, xSize, ySize, zSize, xScale * d, yScale * d, zScale * d, d);
                d /= 2.0f;
            }
            return noiseArray;
        }

        /// <summary>
        /// Генерация шума плоскости в массив
        /// </summary>
        /// <param name="noiseArray">массив</param>
        /// <param name="xOffset">координата Х</param>
        /// <param name="zOffset">координата Z</param>
        /// <param name="xSize">ширина по Х</param>
        /// <param name="zSize">ширина по Z</param>
        /// <param name="xScale">масштаб по X</param>
        /// <param name="zScale">масштаб по Z</param>
        /// <returns></returns>
        public float[] GenerateNoise2d(float[] noiseArray, int xOffset, int zOffset,
            int xSize, int zSize, float xScale, float zScale)
        {
            int i;
            if (noiseArray == null)
            {
                noiseArray = new float[xSize * zSize];
            }
            else
            {
                for (i = 0; i < noiseArray.Length; i++)
                {
                    noiseArray[i] = 0f;
                }
            }
            float d = 1f;
            float x, z;
            for (i = 0; i < _octaves; i++)
            {
                x = xOffset * d * xScale;
                z = zOffset * d * zScale;
                _generatorCollection[i].PopulateNoiseArray2d(noiseArray, x, z, xSize, zSize, xScale * d, zScale * d, d);
                d /= 2.0f;
            }
            return noiseArray;
        }
    }
}
