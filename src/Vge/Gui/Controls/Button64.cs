using Vge.Realms;
using Vge.Renderer;
using Vge.Renderer.Font;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Контрол фиксированной кнопки, 64 на 24
    /// </summary>
    public class Button64 : Button
    {
        public Button64(WindowMain window, FontBase font, string text)
            : base(window, font, 64, ChatStyle.Bolb + text + ChatStyle.Reset, 24) { }

        #region Draw

        /// <summary>
        /// Прорисовка внутреняя
        /// </summary>
        /// <param name="x">Позиция X с учётом интерфейса</param>
        /// <param name="y">Позиция Y с учётом интерфейса</param>
        protected override void _RenderInside(RenderMain render, int x, int y)
        {
            _RenderInside(render, x, _click ? y + _si : y, Text);
            float u1 = Enabled ? _click ? .375f : (Enter ? .25f : .125f) : 0f;
            _meshBg.Reload(RenderFigure.Rectangle(x, y, x + Width * _si, y + Height * _si,
                u1, .046875f, u1 + .125f, .09375f));
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            // Рисуем фон кнопки
            window.Render.BindTextureWidgets();
            _meshBg.Draw();
            // Рисуем текст кнопки
            base.Draw(timeIndex);
        }

        #endregion
    }
}
