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
            float u1 = _click ? .69921875f : Enter ? .669921875f : .640625f;
            _meshBg.Reload(RenderFigure.Rectangle(x, y, x + Width * _si, y + Height * _si,
                u1, .046875f, u1 + .029296875f, .072265625f));
        }

        #endregion
    }
}
