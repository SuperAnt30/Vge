using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект сетки квадрата 2д, для отладки
    /// </summary>
    public class MeshQuad : Mesh
    {
        public MeshQuad(GL gl, float x1, float y1, float x2, float y2) : base(gl)
        {
            _countVertices = 4;
            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vbo);
            _gl.BufferData(GL.GL_ARRAY_BUFFER, new float[]
            {
                x1, y1, 0, 1,
                x1, y2, 0, 0,
                x2, y2, 1, 0,
                x2, y1, 1, 1
            }, GL.GL_STATIC_DRAW);
            _gl.BindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, _ebo);
            _gl.BufferData(GL.GL_ELEMENT_ARRAY_BUFFER, _QuadIndices(), GL.GL_STATIC_DRAW);
        }

        protected override void _InitAtributs()
            => _InitAtributs(new int[] { 2, 2 });
    }
}
