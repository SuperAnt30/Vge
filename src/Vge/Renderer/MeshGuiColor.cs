using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект сетки для 2д gui с цветом
    /// </summary>
    public class MeshGuiColor : Mesh
    {
        public MeshGuiColor(GL gl) : base(gl) { }

        protected override void _InitAtributs()
            => _InitAtributs(new int[] { 2, 2, 4 });

        #region Rectangle

        /// <summary>
        /// Нарисовать прямоугольник в 2д, с цветом но без текстуры 
        /// </summary>
        public static float[] Rectangle(int x1, int y1, int x2, int y2, float r, float g, float b)
        {
            return new float[]
            {
                x1, y1, 0, 0, r, g, b, 1,
                x1, y2, 0, 0, r, g, b, 1,
                x2, y2, 0, 0, r, g, b, 1,
                x2, y1, 0, 0, r, g, b, 1
            };
        }

        /// <summary>
        /// Нарисовать прямоугольник в 2д, без цвета
        /// </summary>
        public static float[] Rectangle(int x1, int y1, int x2, int y2, float v1, float u1, float v2, float u2)
        {
            return new float[]
            {
                x1, y1, v1, u1, 1, 1, 1, 1,
                x1, y2, v1, u2, 1, 1, 1, 1,
                x2, y2, v2, u2, 1, 1, 1, 1,
                x2, y1, v2, u1, 1, 1, 1, 1
            };
        }

        #endregion
    }
}
