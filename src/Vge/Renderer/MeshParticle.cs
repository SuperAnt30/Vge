using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект сетки частички, без текстуры
    /// </summary>
    public class MeshParticle : Mesh
    {
        public MeshParticle(GL gl, float[] buffer) : base(gl)
        {
            _countVertices = buffer.Length / _vertexSize;
            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vbo);
            _gl.BufferData(GL.GL_ARRAY_BUFFER, buffer, GL.GL_STATIC_DRAW);
            _gl.BindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, _ebo);
            _gl.BufferData(GL.GL_ELEMENT_ARRAY_BUFFER, _QuadIndices(), GL.GL_STATIC_DRAW);
        }

        protected override void _InitAtributs()
            => _InitAtributs(new int[] { 3, 1 });
    }
}
