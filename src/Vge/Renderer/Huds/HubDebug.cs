using System.Collections.Generic;
using Vge.Games;

namespace Vge.Renderer.Huds
{
    /// <summary>
    /// Отладочная индикация Heads-Up Display
    /// </summary>
    public class HubDebug : HudBase
    {
        /// <summary>
        /// Сетка курсора
        /// </summary>
        private readonly MeshGuiLine _meshCursor;

        /// <summary>
        /// Текстура карты света
        /// </summary>
        private readonly MeshGuiColor _meshLightMap;

        public HubDebug(GameBase game) : base(game)
        {
            _meshCursor = new MeshGuiLine(game.GetOpenGL());
            _meshLightMap = new MeshGuiColor(game.GetOpenGL());
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            int wc = Gi.Width / 2;
            int hc = Gi.Height / 2;
            List<float> buffer = new List<float>();
            buffer.AddRange(MeshGuiLine.RectangleLine(wc - 8, hc, wc + 8, hc, 1, 1, 1, 1));
            buffer.AddRange(MeshGuiLine.RectangleLine(wc, hc - 8, wc, hc + 8, 1, 1, 1, 1));
            _meshCursor.Reload(buffer.ToArray());

            // Для контура надо перекулючится без текстуры
            _game.Render.TextureDisable();
            _game.Render.ShaderBindGuiLine();
            _meshCursor.Draw();

#if DEBUG

            // Отладка Карты света
            _meshLightMap.Reload(MeshGuiColor.Rectangle(Gi.Width - 144, 16, Gi.Width - 16, 144, 0, 0, 1, 1));
            _game.Render.TextureEnable();
            _game.Render.LightMap.BindTexture2dGui();
            _game.Render.ShaderBindGuiColor();
            _meshLightMap.Draw();

#endif

        }

        public override void Dispose()
        {
            _meshCursor.Dispose();
            _meshLightMap.Dispose();
        }
    }
}
