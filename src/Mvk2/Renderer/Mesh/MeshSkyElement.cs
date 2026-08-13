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
        /// Объект сетки для элементов неба, с текстурой
        /// </summary>
        /// <param name="typeDraw">GL_STATIC_DRAW, GL_DYNAMIC_DRAW, GL_STREAM_DRAW</param>
        public MeshSkyElement(GL gl, uint typeDraw) : base(gl)
            => _typeDraw = typeDraw;

        protected override void _InitAtributs()
            => _InitAtributs(new int[] { 3, 2 });
    }
}
