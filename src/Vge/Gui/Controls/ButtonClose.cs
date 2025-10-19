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
            float v1 = Enabled ? _isLeftDown ? .5625f : (Enter ? .5f : .4375f) : .375f;

            _meshBg.Reload(RenderFigure.Rectangle(x, y, x + 32 * _si, y + 32 * _si,
                .5625f, v1, .625f, v1 + .0625f));
        }

        #endregion
    }
}
