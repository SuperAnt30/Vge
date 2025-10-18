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
            float u1 = _click ? .59375f : Enter ? .546875f : .5f;
            _meshBg.Reload(RenderFigure.Rectangle(x, y, x + Width * _si, y + Height * _si,
                u1, .046875f, u1 + .046875f, .09375f));
        }

        #endregion
    }
}
