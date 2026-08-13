using Vge.Renderer.Mesh;
using WinGL.OpenGL;

namespace Mvk2.Renderer.Mesh
{
    /// <summary>
    /// Объект сетки для облаков, с текстурой
    /// </summary>
    public class MeshSkyClouds : MeshBase
    {
        /// <summary>
        /// Объект сетки для облаков, с текстурой
        /// </summary>
        public MeshSkyClouds(GL gl) : base(gl)
            => _typeDraw = GL.GL_STATIC_DRAW;

        protected override void _InitAtributs()
            => _InitAtributs(new int[] { 3, 2, 1 });
    }
}
