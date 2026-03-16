using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект сетки частички, без текстуры
    /// </summary>
    public class MeshParticle : Mesh
    {
        public MeshParticle(GL gl) : base(gl) { }

        protected override void _InitAtributs()
            => _InitAtributs(new int[] { 3, 1 });
    }
}
