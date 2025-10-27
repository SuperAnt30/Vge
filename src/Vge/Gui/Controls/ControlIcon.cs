using Vge.Item;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Контрол оконка предмета, нужна для визуализации восле мышки и на контроле
    /// </summary>
    public class ControlIcon : WidgetBase
    {
        /// <summary>
        /// Объект рендера
        /// </summary>
        private readonly RenderSlot _renderSlot;

        public ControlIcon(WindowMain window, RenderSlot renderSlot) : base(window)
        {
            SetSize(32, 32).SetText("");
            _renderSlot = renderSlot;
        }

        /// <summary>
        /// Стак
        /// </summary>
        public ItemStack Stack => _renderSlot.Stack;

        /// <summary>
        /// Задать новый или изменённый стак
        /// </summary>
        public void SetStack(ItemStack stack)
        {
            _renderSlot.Stack = stack;
            IsRender = true;
        }

        #region OnMouse

        /// <summary>
        /// Нажатие клавиши мышки
        /// </summary>
        public override void OnMouseMove(int x, int y)
        {
            _renderSlot.PosX = x - 16 * _si;
            _renderSlot.PosY = y - 16 * _si;
        }

        #endregion

        public override void OnResized()
        {
            base.OnResized();
            _renderSlot.OnResized();
        }

        #region Draw

        public override void Rendering()
        {
            _renderSlot.Rendering();
            IsRender = false;
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex) => _renderSlot.Draw();

        #endregion

        public override void Dispose() => _renderSlot.Dispose();
    }
}
