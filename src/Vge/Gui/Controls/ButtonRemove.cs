using Vge.Renderer;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Контрол фиксированной кнопки Удаления, 24 на 24
    /// </summary>
    public class ButtonRemove : ButtonIcon
    {
        public ButtonRemove(WindowMain window)
            : base(window, 24, 24) { }

        #region Draw

        /// <summary>
        /// Прорисовка внутреняя
        /// </summary>
        /// <param name="x">Позиция X с учётом интерфейса</param>
        /// <param name="y">Позиция Y с учётом интерфейса</param>
        protected override void _RenderInside(RenderMain render, int x, int y)
        {
            float v1 = Enabled ? _isLeftDown ? .75f : (Enter ? .6875f : .625f) : .5625f;
            _meshBg.Reload(RenderFigure.Rectangle(x, y, x + 32 * _si, y + 32 * _si,
                .1875f, v1, .25f, v1 + .0625f));
        }

        #endregion
    }
}
