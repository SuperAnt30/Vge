using System.Runtime.CompilerServices;

namespace Vge.Renderer
{
    public static class RenderFigure
    {
        /// <summary>
        /// Прямоугольник из линий
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float[] RectangleLine(int x1, float y1, float x2, float y2, 
            float r, float g, float b, float a)
        {
            return new float[]
            {
                x1, y1, r, g, b, a,
                x1, y2, r, g, b, a,

                x1, y2, r, g, b, a,
                x2, y2, r, g, b, a,

                x2, y2, r, g, b, a,
                x2, y1, r, g, b, a,

                x2, y1, r, g, b, a,
                x1, y1, r, g, b, a
            };
        }

        /// <summary>
        /// Нарисовать прямоугольник в 2д, c цветом
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float[] Rectangle(int x1, float y1, int x2, int y2,
            float u1, float v1, float u2, float v2,
            float r, float g, float b, float a = 1f)
        {
            return new float[]
            {
                x1, y1, u1, v1, r, g, b, a,
                x1, y2, u1, v2, r, g, b, a,
                x2, y2, u2, v2, r, g, b, a,
                x2, y1, u2, v1, r, g, b, a
            };
        }

        /// <summary>
        /// Нарисовать прямоугольник в 2д, без цвета
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float[] Rectangle(int x1, float y1, int x2, int y2,
            float u1, float v1, float u2, float v2)
        {
            return new float[]
            {
                x1, y1, u1, v1, 1, 1, 1, 1,
                x1, y2, u1, v2, 1, 1, 1, 1,
                x2, y2, u2, v2, 1, 1, 1, 1,
                x2, y1, u2, v1, 1, 1, 1, 1
            };
        }

        /// <summary>
        /// Нарисовать прямоугольник в 2д, с цветом но без текстуры 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float[] Rectangle(int x1, int y1, int x2, int y2, 
            float r, float g, float b)
        {
            return new float[]
            {
                x1, y1, 0, 0, r, g, b, 1,
                x1, y2, 0, 0, r, g, b, 1,
                x2, y2, 0, 0, r, g, b, 1,
                x2, y1, 0, 0, r, g, b, 1
            };
        }
    }
}
