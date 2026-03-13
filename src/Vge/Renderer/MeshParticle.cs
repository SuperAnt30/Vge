using System.Runtime.CompilerServices;
using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект сетки частички, без текстуры, пока
    /// </summary>
    public class MeshParticle : Mesh
    {
        public MeshParticle(GL gl) : base(gl) { }

        protected override void _InitAtributs()
            => _InitAtributs(new int[] { 3, 4 });
    }
}
