using Vge.Renderer;
using Vge.Renderer.Font;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Контрол фиксированной кнопки, 128 на 24
    /// </summary>
    public class Button128 : Button
    {
        public Button128(WindowMain window, FontBase font, string text)
            : base(window, font, 128, text, 24) { }

        #region Draw

        /// <summary>
        /// Прорисовка внутреняя
        /// </summary>
        /// <param name="x">Позиция X с учётом интерфейса</param>
        /// <param name="y">Позиция Y с учётом интерфейса</param>
        protected override void _RenderInside(RenderMain render, int x, int y)
        {
            _RenderInside(render, x, _click ? y + _si : y, Text);
            float u1 = Enabled ? _click ? .75f : (Enter ? .5f : .25f) : 0f;
            _meshBg.Reload(RenderFigure.Rectangle(x, y, x + Width * _si, y + Height * _si,
                u1, 0, u1 + .25f, .046875f));
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
