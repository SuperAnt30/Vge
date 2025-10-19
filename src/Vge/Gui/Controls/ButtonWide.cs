using Vge.Renderer;
using Vge.Renderer.Font;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Контрол толстой кнопки
    /// </summary>
    public class ButtonWide : Button
    {
        public ButtonWide(WindowMain window, FontBase font, int width, string text)
            : base(window, font, width, text, 48) { }

        #region Draw

        /// <summary>
        /// Прорисовка внутреняя
        /// </summary>
        /// <param name="x">Позиция X с учётом интерфейса</param>
        /// <param name="y">Позиция Y с учётом интерфейса</param>
        protected override void _RenderInside(RenderMain render, int x, int y)
        {
            base._RenderInside(render, x, _isLeftDown ? y + _si * 2 : y);
            float v1 = _isLeftDown ? .25f : (Enter ? .125f : 0);
            _meshBg.Reload(_RectangleTwo(x, y, .5f, v1, .5f, .125f, 64));
        }

        #endregion
    }
}
