using WinGL.OpenGL;

namespace Vge.Renderer.Mesh
{
    /// <summary>
    /// Объект сетки неба, без текстуры
    /// </summary>
    public class MeshSky : MeshBase
    {
        public MeshSky(GL gl) : base(gl) { }

        protected override void _InitEbo() { }

        protected override void _InitAtributs()
            => _InitAtributs(new int[] { 3, 1 });

        /// <summary>
        /// Прорисовать меш с линиями GL_LINES
        /// </summary>
        public override void Draw()
        {
            _gl.BindVertexArray(_vao);
            _gl.DrawArrays(GL.GL_TRIANGLES, 0, _countVertices);
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
            _gl.BufferData(GL.GL_ARRAY_BUFFER, vertices, GL.GL_STATIC_DRAW);
        }
    }
}
