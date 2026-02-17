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
        private readonly float _u1;
        private readonly float _u2;

        private readonly string _nameTab;

        public ButtonTab(WindowMain window, int index, string nameTab)
            : base(window, 24, 20)
        {
            _u1 = .5f + index * .0625f;
            _u2 = _u1 + .0625f;
            _nameTab = nameTab;
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
                _u1, v1, _u2, v1 + .0625f));
        }

        #endregion

        /// <summary>
        /// Вернуть подсказку у контрола
        /// </summary>
        public override string GetToolTip() => _nameTab;
    }
}
