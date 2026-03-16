using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект сетки частички, с текстурой
    /// </summary>
    public class MeshParticleTexture : Mesh
    {
        public MeshParticleTexture(GL gl) : base(gl) { }

        protected override void _InitAtributs()
            => _InitAtributs(new int[] { 2, 2 });
    }
}
