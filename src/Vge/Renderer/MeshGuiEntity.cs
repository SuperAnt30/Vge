using Vge.Entity.Render;
using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект сетки для сущностей и предметов с текстурой для Gui
    /// </summary>
    public class MeshGuiEntity : Mesh
    {
        public MeshGuiEntity(GL gl) : base(gl) { }

        protected override void _InitAtributs()
            => _InitAtributs(new int[] { 3, 3, 2 });

        /// <summary>
        /// Перезаписать полигоны
        /// </summary>
        public void Reload(VertexEntityBuffer buffers)
        {
            _countVertices = buffers.GetCountVertices();

            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vbo);
            _gl.BufferData(GL.GL_ARRAY_BUFFER, buffers.BufferFloat, GL.GL_STATIC_DRAW);
            _gl.BindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, _ebo);
            _gl.BufferData(GL.GL_ELEMENT_ARRAY_BUFFER, _QuadIndices(), GL.GL_STATIC_DRAW);
        }
    }
}
