using WinGL.OpenGL;

namespace Vge.Renderer.Mesh
{
    /// <summary>
    /// Объект сетки для 2д gui с цветом
    /// </summary>
    public class MeshGuiColor : MeshBase
    {
        public MeshGuiColor(GL gl) : base(gl) { }

        protected override void _InitAtributs()
            => _InitAtributs(new int[] { 2, 2, 4 });

    }
}
