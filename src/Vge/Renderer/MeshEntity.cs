using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект сетки для сущностей с текстурой
    /// </summary>
    public class MeshEntity : Mesh
    {
        public MeshEntity(GL gl) : base(gl) { }

        protected override void _InitAtributs()
            => _InitAtributs(new int[] { 3, 2, 1 });
    }
}
