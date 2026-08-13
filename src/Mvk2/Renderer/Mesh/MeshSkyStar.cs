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
        public MeshSkyStar(GL gl) : base(gl)
            => _typeDraw = GL.GL_STATIC_DRAW;

        protected override void _InitAtributs()
            => _InitAtributs(new int[] { 3, 1 });
    }
}
