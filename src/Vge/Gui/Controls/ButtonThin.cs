using Vge.Renderer;
using Vge.Renderer.Font;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Контрол тонкой кнопки
    /// </summary>
    public class ButtonThin : Button
    {
        public ButtonThin(WindowMain window, FontBase font, int width, string text)
            : base(window, font, width, text, 24) { }

        #region Draw

        /// <summary>
        /// Прорисовка внутреняя
        /// </summary>
        /// <param name="x">Позиция X с учётом интерфейса</param>
        /// <param name="y">Позиция Y с учётом интерфейса</param>
        protected override void _RenderInside(RenderMain render, int x, int y)
        {
            base._RenderInside(render, x, _isLeftDown ? y + _si : y);
            float u1 = Enabled ? _isLeftDown ? .1875f : (Enter ? .125f : .0625f) : 0f;
            _meshBg.Reload(_RectangleTwo(x, y, 0, u1, .5f, .0625f, 32));
        }

        #endregion
    }
}
