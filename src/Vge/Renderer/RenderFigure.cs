namespace Vge.Renderer
{
    public static class RenderFigure
    {
        //public static int[] Quad = new int[] { 0, 1, 2, 1, 3, 2 };
        /// <summary>
        /// Нарисовать прямоугольник в 2д, без цвета [2, 2]
        /// </summary>
        public static float[] Rectangle2d(float x1, float y1, float x2, float y2, float v1, float u1, float v2, float u2)
        {
            return new float[]
            {
                x1, y1, v1, u1,
                x1, y2, v1, u2,
                x2, y1, v2, u1,

               // x1, y2, v1, u2,
                x2, y2, v2, u2,
              //  x2, y1, v2, u1
            };
        }

        /// <summary>
        /// Нарисовать прямоугольник в 2д, с цветом [2, 2, 3]
        /// </summary>
        public static float[] Rectangle2d(float x1, float y1, float x2, float y2, float v1, float u1, float v2, float u2,
            float r, float g, float b)
        {
            return new float[]
            {
                x1, y1, v1, u1, r, g, b,
                x1, y2, v1, u2, r, g, b,
                x2, y1, v2, u1, r, g, b,

              //  x1, y2, v1, u2, r, g, b,
                x2, y2, v2, u2, r, g, b,
               // x2, y1, v2, u1, r, g, b
            };
        }
    }
}
