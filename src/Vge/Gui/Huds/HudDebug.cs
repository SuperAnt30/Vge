using Vge.Games;
using Vge.Renderer;

namespace Vge.Gui.Huds
{
    /// <summary>
    /// Отладочная индикация Heads-Up Display
    /// </summary>
    public class HudDebug : HudBase
    {
        /// <summary>
        /// Текстура карты света
        /// </summary>
        private readonly MeshGuiColor _meshLightMap;

        public HudDebug(GameBase game) : base(game)
        {
            _meshLightMap = new MeshGuiColor(game.GetOpenGL());
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            base.Draw(timeIndex);
#if DEBUG

            // Отладка Карты света
            _meshLightMap.Reload(RenderFigure.Rectangle(Gi.Width - 144, 16, Gi.Width - 16, 144, 0, 0, 1, 1));
            _game.Render.LightMap.BindTexture2dGui();
            _game.Render.ShaderBindGuiColor();
            _meshLightMap.Draw();
            
#endif
        }

        public override void Dispose()
        {
            base.Dispose();
            _meshLightMap.Dispose();
        }
    }
}
