using Vge.Renderer.Mesh;
using WinGL.OpenGL;

namespace Mvk2.Renderer.Mesh
{
    /// <summary>
    /// Объект сетки для элементов неба, с текстурой
    /// </summary>
    public class MeshSkyElement : MeshBase
    {
        /// <summary>
        /// Тип отрисовки.
        /// GL_STATIC_DRAW: данные либо никогда не будут изменяться, либо будут изменяться очень редко;
        /// GL_DYNAMIC_DRAW: данные будут меняться довольно часто;
        /// GL_STREAM_DRAW: данные будут меняться при каждой отрисовке.
        /// </summary>
        private readonly uint _typeDraw;

        /// <summary>
        /// Объект сетки для элементов неба, с текстурой
        /// </summary>
        /// <param name="typeDraw">GL_STATIC_DRAW, GL_DYNAMIC_DRAW, GL_STREAM_DRAW</param>
        public MeshSkyElement(GL gl, uint typeDraw) : base(gl)
            => _typeDraw = typeDraw;

        protected override void _InitAtributs()
            => _InitAtributs(new int[] { 3, 2 });

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
            _gl.BufferData(GL.GL_ELEMENT_ARRAY_BUFFER, _QuadIndices(), _typeDraw);
        }
    }
}
