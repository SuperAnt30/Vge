using Vge;
using Vge.Gui.Controls;
using Vge.Renderer;

namespace Mvk2.Gui.Controls
{
    /// <summary>
    /// Контрол фиксированной кнопки Стрелки Left, 24 на 24
    /// </summary>
    public class ButtonArrow : ButtonIcon
    {
        private readonly float u1;
        private readonly float u2;

        public ButtonArrow(WindowMain window, bool right)
            : base(window, 24, 20)
        {
            if (right)
            {
                u1 = .375f;
                u2 = .4375f;
            }
            else
            {
                u1 = .3125f;
                u2 = .375f;
            }
        }

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
                u1, v1, u2, v1 + .0625f));

            
        }

        #endregion
    }
}
