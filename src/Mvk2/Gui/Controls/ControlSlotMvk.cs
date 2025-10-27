using Vge.Gui;
using Vge.Gui.Controls;
using Vge.Item;
using Vge.Realms;
using Vge.Renderer;

namespace Mvk2.Gui.Controls
{
    /// <summary>
    /// Контрол слота предмета
    /// </summary>
    public class ControlSlotMvk : ControlSlot
    {
        /// <summary>
        /// Сетка фона
        /// </summary>
        private readonly MeshGuiColor _meshBg;

        public ControlSlotMvk(WindowMvk window, ItemStack stack, byte slotId) 
            : base(window, new RenderSlotMvk(window, stack), slotId)
        {
            _meshBg = new MeshGuiColor(gl);
        }

        public override void Rendering()
        {
            float v1, v2;
            if (Enabled)
            {
                if (Enter)
                {
                    v1 = .9296875f;
                    v2 = 1f;
                }
                else
                {
                    v1 = .859375f;
                    v2 = .9296875f;
                }
            }
            else
            {
                v2 = .859375f;
                v1 = .7890625f;
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

        /// <summary>
        /// Вернуть подсказку у контрола
        /// </summary>
        public override string GetToolTip()
        {
            if (Stack != null)
            {
                return "!!!!Testing" + ChatStyle.Blue + "Stak\r\n" + ChatStyle.Reset + ChatStyle.Bolb + Stack.ToString();
            }
            return "";
        }

        public override void Dispose()
        {
            base.Dispose();
            _meshBg?.Dispose();
        }
    }
}
