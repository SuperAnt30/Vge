using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект сетки для 2д gui линий с цветом альфа без текстуры
    /// Не используем EBO
    /// </summary>
    public class MeshGuiLine : Mesh
    {
        public MeshGuiLine(GL gl) : base(gl) { }

        protected override void _InitEbo() { }

        protected override void _InitAtributs()
            => _InitAtributs(new int[] { 2, 4 });

        /// <summary>
        /// Прорисовать меш с линиями GL_LINES
        /// </summary>
        public override void Draw()
        {
            _gl.BindVertexArray(_vao);
            _gl.DrawArrays(GL.GL_LINES, 0, _countVertices);
        }

        /// <summary>
        /// Перезаписать полигоны, не создавая и не меняя длинну одной точки
        /// </summary>
        public override void Reload(float[] vertices)
        {
            _countVertices = vertices.Length / _vertexSize;
            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vbo);
            _gl.BufferData(GL.GL_ARRAY_BUFFER, vertices, GL.GL_STATIC_DRAW);
        }

        /// <summary>
        /// Прямоугольник из линий
        /// </summary>
        public static float[] RectangleLine(int x1, float y1, float x2, float y2, float r, float g, float b, float a)
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
    }
}
