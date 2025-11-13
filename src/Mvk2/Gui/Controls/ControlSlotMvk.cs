using Mvk2.Item;
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
        /// <summary>
        /// Слот относится ли к одежде
        /// </summary>
        private readonly EnumCloth _cloth;

        public ControlSlotMvk(WindowMvk window, ItemStack stack, byte slotId, EnumCloth cloth = EnumCloth.None) 
            : base(window, new RenderSlotMvk(window, stack), slotId)
        {
            _meshBg = new MeshGuiColor(gl);
            _cloth = cloth;
        }

        public override void Rendering()
        {
            bool bg = true;
            float v1 = 0;
            float v2 = 0;
            if (Enabled)
            {
                if (Enter) v2 = .0703125f;
                else bg = false;
            }
            else
            {
                v1 = .0703125f;
                v2 = .140625f;
            }

            if (_cloth != EnumCloth.None && Stack == null)
            {
                // Слот одежды (с маркером фона)
                float v3 = .5f + (int)_cloth * .0625f;

                if (bg)
                {
                    RenderSlots.ListBuffer.Clear();
                    
                    // Маркер 32*32
                    RenderSlots.ListBuffer.AddRange(RenderFigure.Rectangle((PosX + 2) * _si, (PosY + 2) * _si,
                        (PosX + 34) * _si, (PosY + 34) * _si,
                        v3 - .0625f, .9375f, v3, 1));

                    // Выделенный слот
                    RenderSlots.ListBuffer.AddRange(RenderFigure.Rectangle(PosX * _si, PosY * _si,
                        (PosX + Width) * _si, (PosY + Height) * _si,
                        v1, .9296875f, v2, 1));

                    _meshBg.Reload(RenderSlots.ListBuffer.GetBufferAll(), RenderSlots.ListBuffer.Count);
                }
                else
                {
                    // Маркер 32*32
                    _meshBg.Reload(RenderFigure.Rectangle((PosX + 2) * _si, (PosY + 2) * _si,
                        (PosX + 34) * _si, (PosY + 34) * _si,
                        v3 - .0625f, .9375f, v3, 1));
                }
            }
            else
            {
                // Обычный слот
                if (bg)
                {
                    // Фон слота
                    _meshBg.Reload(RenderFigure.Rectangle(PosX * _si, PosY * _si, 
                        (PosX + Width) * _si, (PosY + Height) * _si,
                        v1, .9296875f, v2, 1));
                }
                else
                {
                    // Нет фона
                    _meshBg.Reload(new float[] { });
                }
            }

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
