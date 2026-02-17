using Vge;
using Vge.Gui.Controls;
using Vge.Renderer;

namespace Mvk2.Gui.Controls
{
    /// <summary>
    /// Контрол фиксированной кнопки Стрелки Left, 24 на 24
    /// </summary>
    public class ButtonTab : ButtonIcon
    {
        private readonly float u1;
        private readonly float u2;

        public ButtonTab(WindowMain window, int index)
            : base(window, 24, 20)
        {
            u1 = .5f + index * .0625f;
            u2 = u1 + .0625f;
        }

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
                u1, v1, u2, v1 + .0625f));
        }

        #endregion
    }
}
