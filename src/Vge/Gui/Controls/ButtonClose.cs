using Vge.Renderer;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Контрол фиксированной кнопки закрыть окно, 15 на 13
    /// </summary>
    public class ButtonClose : ButtonIcon
    {
        public ButtonClose(WindowMain window)
            : base(window, 15, 13) { }

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
                .25f, v1, .3125f, v1 + .0625f));
        }

        #endregion
    }
}
