using Vge.Renderer.Mesh;
using WinGL.OpenGL;

namespace Mvk2.Renderer.Mesh
{
    /// <summary>
    /// Объект сетки для звёзд неба, без текстуры
    /// </summary>
    public class MeshSkyStar : MeshBase
    {
        /// <summary>
        /// Объект сетки для звёзд неба, без текстуры
        /// </summary>
        public MeshSkyStar(GL gl) : base(gl) { }

        protected override void _InitAtributs()
            => _InitAtributs(new int[] { 3, 1 });

        /// <summary>
        /// Перезаписать полигоны, не создавая и не меняя длинну одной точки
        /// </summary>
        public override void Reload(float[] vertices)
        {
            _countVertices = vertices.Length / _vertexSize;
            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vbo);
            _gl.BufferData(GL.GL_ARRAY_BUFFER, vertices, GL.GL_DYNAMIC_DRAW);
            _gl.BindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, _ebo);
            _gl.BufferData(GL.GL_ELEMENT_ARRAY_BUFFER, _QuadIndices(), GL.GL_STATIC_DRAW);
        }
    }
}
