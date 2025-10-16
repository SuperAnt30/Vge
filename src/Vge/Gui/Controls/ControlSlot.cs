using System;
using Vge.Item;
using Vge.Realms;
using Vge.Renderer;
using Vge.Renderer.Font;
using WinGL.Actions;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Контрол слота предмета
    /// </summary>
    public class ControlSlot : ControlIcon
    {
        /// <summary>
        /// Номер слота
        /// </summary>
        public byte SlotId { get; private set; }

        /// <summary>
        /// Сетка фона
        /// </summary>
        private readonly MeshGuiColor _meshBg;

        public ControlSlot(WindowMain window, FontBase font, byte slotId,  ItemStack stack) 
            : base(window, font, stack)
        {
            SlotId = slotId;
            Stack = stack;
            _meshBg = new MeshGuiColor(gl);
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
            // 0
            // .09765625f;
            // .1953125f
            // .29296875f
            float v1, v2;
            if (Enabled)
            {
                if (Enter)
                {
                    v1 = .140625f;
                    v2 = .2109375f;
                }
                else
                {
                    v1 = .0703125f;
                    v2 = .140625f;
                }
            }
            else
            {
                v2 = .0703125f;
                v1 = 0;
            }
            _meshBg.Reload(RenderFigure.Rectangle(PosX * _si, PosY * _si, (PosX + Width) * _si, (PosY + Height) * _si,
                    v1, .9296875f, v2, 1));

            base.Rendering();
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            // Рисуем фон кнопки
            window.Render.BindTextureWidgets();
            _meshBg.Draw();
            
            base.Draw(timeIndex);
        }

        #endregion

        /// <summary>
        /// Вернуть подсказку у контрола
        /// </summary>
        public override string GetToolTip()
        {
            if (Stack != null)
            {
                return "Testing" + ChatStyle.Blue + "Stak\r\n" + ChatStyle.Reset + ChatStyle.Bolb + Stack.ToString();
            }
            return "";
        }

        public override void Dispose()
        {
            base.Dispose();
            _meshBg?.Dispose();
        }

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
