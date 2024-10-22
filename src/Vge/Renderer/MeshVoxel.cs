using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект сетки для воксельного мира с цветом альфа без текстуры
    /// </summary>
    public class MeshVoxel : Mesh
    {
        public MeshVoxel(GL gl) : base(gl, new int[] { 3, 2, 1, 1 }, true) { }

    }
}
