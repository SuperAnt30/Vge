using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект сетки частички, с текстурой
    /// </summary>
    public class MeshParticleTexture : MeshParticle
    {
        public MeshParticleTexture(GL gl, float[] buffer) : base(gl, buffer) { }

        protected override void _InitAtributs()
            => _InitAtributs(new int[] { 2, 2 });
    }
}
