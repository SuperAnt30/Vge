using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект буфера сетки через VAO для 2д спрайтов
    /// </summary>
    public class Mesh2d : Mesh
    {
        public Mesh2d(GL gl) : base(gl, new int[] { 2, 2, 3 }) { }
    }
}
