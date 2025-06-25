using System.Runtime.CompilerServices;
using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект сетки для 3д линий с цветом альфа без текстуры
    /// Не используем EBO
    /// </summary>
    public class MeshLine : Mesh
    {
        /// <summary>
        /// Тип отрисовки.
        /// GL_STATIC_DRAW: данные либо никогда не будут изменяться, либо будут изменяться очень редко;
        /// GL_DYNAMIC_DRAW: данные будут меняться довольно часто;
        /// GL_STREAM_DRAW: данные будут меняться при каждой отрисовке.
        /// </summary>
        private readonly uint _typeDraw;

        /// <summary>
        /// Объект сетки для 3д линий с цветом альфа без текстуры
        /// Не используем EBO
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="typeDraw">GL_STATIC_DRAW, GL_DYNAMIC_DRAW, GL_STREAM_DRAW</param>
        public MeshLine(GL gl, uint typeDraw) : base(gl)
            => _typeDraw = typeDraw;

        protected override void _InitEbo() { }

        protected override void _InitAtributs()
            => _InitAtributs(new int[] { 3, 4 });

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
            // GL_STATIC_DRAW: данные либо никогда не будут изменяться, либо будут изменяться очень редко;
            // GL_DYNAMIC_DRAW: данные будут меняться довольно часто;
            // GL_STREAM_DRAW: данные будут меняться при каждой отрисовке.
            _gl.BufferData(GL.GL_ARRAY_BUFFER, vertices, _typeDraw);
        }

        /// <summary>
        /// Прямоугольный параллелепипед из линий
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float[] CubeLine(float x1, float y1, float z1, 
            float x2, float y2, float z2, float r, float g, float b, float a)
        {
            return new float[]
            {
                // Квад низа
                x1, y1, z1, r, g, b, a,
                x2, y1, z1, r, g, b, a,

                x1, y1, z1, r, g, b, a,
                x1, y1, z2, r, g, b, a,

                x2, y1, z1, r, g, b, a,
                x2, y1, z2, r, g, b, a,

                x1, y1, z2, r, g, b, a,
                x2, y1, z2, r, g, b, a,

                // Квад вверха
                x1, y2, z1, r, g, b, a,
                x2, y2, z1, r, g, b, a,

                x1, y2, z1, r, g, b, a,
                x1, y2, z2, r, g, b, a,

                x2, y2, z1, r, g, b, a,
                x2, y2, z2, r, g, b, a,

                x1, y2, z2, r, g, b, a,
                x2, y2, z2, r, g, b, a,

                // Стойки
                x1, y1, z1, r, g, b, a,
                x1, y2, z1, r, g, b, a,

                x2, y1, z1, r, g, b, a,
                x2, y2, z1, r, g, b, a,

                x1, y1, z2, r, g, b, a,
                x1, y2, z2, r, g, b, a,

                x2, y1, z2, r, g, b, a,
                x2, y2, z2, r, g, b, a,
            };
        }
    }
}
