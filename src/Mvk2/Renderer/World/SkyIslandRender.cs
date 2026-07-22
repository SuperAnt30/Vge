using Vge.Entity.Player;
using Vge.Renderer.World;
using WinGL.OpenGL;

namespace Mvk2.Renderer.World
{
    /// <summary>
    /// Объект отвечает за прорисовку неба для мира с островом
    /// </summary>
    public class SkyIslandRender : SkyRender
    {
        /// <summary>
        /// Объект OpenGL для элемента управления
        /// </summary>
        private readonly GL gl;

        public SkyIslandRender(PlayerClientOwner player, WorldRenderer worldRenderer) 
            : base(player, worldRenderer)
        {
            gl = worldRenderer.GetOpenGL();
        }

        /// <summary>
        /// Прорисовка неба
        /// </summary>
        public override void DrawSky(float timeIndex)
        {
            gl.ClearColor(_worldRenderer.ColorSky.X, _worldRenderer.ColorSky.Y, _worldRenderer.ColorSky.Z, 1f);
        }
    }
}
