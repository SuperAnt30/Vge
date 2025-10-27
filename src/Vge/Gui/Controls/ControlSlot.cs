using System;
using Vge.Item;
using WinGL.Actions;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Контрол слота предмета
    /// </summary>
    public class ControlSlot : WidgetBase
    {
        /// <summary>
        /// Номер слота
        /// </summary>
        public byte SlotId { get; private set; }

        /// <summary>
        /// Объект рендера
        /// </summary>
        private readonly RenderSlot _renderSlot;

        public ControlSlot(WindowMain window, RenderSlot renderSlot, byte slotId) 
            : base(window)
        {
            SetSize(36, 36).SetText("");
            _renderSlot = renderSlot;
            SlotId = slotId;
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

        /// <summary>
        /// Задать позицию контрола
        /// </summary>
        public override WidgetBase SetPosition(int x, int y)
        {
            _renderSlot.PosX = x * _si;
            _renderSlot.PosY = y * _si;
            base.SetPosition(x, y);
            return this;
        }

        public override void OnResized()
        {
            base.OnResized();
            _renderSlot.OnResized();
        }

        #region OnMouse

        /// <summary>
        /// Нажатие клавиши мышки
        /// </summary>
        public override void OnMouseDown(MouseButton button, int x, int y)
        {
            if (button == MouseButton.Left)
            {
                OnMouseMove(x, y);
                if (Enter) _OnClickLeft();
            }
            else if (button == MouseButton.Right)
            {
                OnMouseMove(x, y);
                if (Enter) _OnClickRight();
            }
        }

        /// <summary>
        /// Нажатие клавиши мышки
        /// </summary>
        public override void OnMouseMove(int x, int y) => _CanEnter(x, y);

        #endregion

        #region Draw

        public override void Rendering()
        {
            _renderSlot.Rendering();
            base.Rendering();
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex) => _renderSlot.Draw();

        #endregion

        public override void Dispose() => _renderSlot.Dispose();

        #region Event

        /// <summary>
        /// Событие клика ЛКМ
        /// </summary>
        public event EventHandler ClickLeft;
        /// <summary>
        /// Событие клика ЛКМ
        /// </summary>
        private void _OnClickLeft() => ClickLeft?.Invoke(this, new EventArgs());

        /// <summary>
        /// Событие клика ПКМ
        /// </summary>
        public event EventHandler ClickRight;
        /// <summary>
        /// Событие клика ПКМ
        /// </summary>
        private void _OnClickRight() => ClickRight?.Invoke(this, new EventArgs());

        #endregion
    }
}
