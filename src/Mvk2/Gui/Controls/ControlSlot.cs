using Mvk2.Renderer;
using Vge.Entity.Inventory;
using Vge.Gui.Controls;
using Vge.Renderer;

namespace Mvk2.Gui.Controls
{
    /// <summary>
    /// Контрол слота предмета
    /// </summary>
    public class ControlSlot : WidgetBase
    {
        private readonly RenderMvk _render;

        /// <summary>
        /// Сетка фона
        /// </summary>
        private readonly MeshGuiColor _meshBg;

        /// <summary>
        /// Слот
        /// </summary>
        private Slot _slot;

        /// <summary>
        /// Расположение предмета по Х
        /// </summary>
        private int _posItemX;
        /// <summary>
        /// Расположение предмета по Y
        /// </summary>
        private int _posItemY;

        public ControlSlot(WindowMvk window, Slot slot) : base(window)
        {
            _render = window.GetRender();
            SetSize(50, 50).SetText("");
            _slot = slot;
            _meshBg = new MeshGuiColor(gl);
        }

        public void SetSlot(Slot slot)
        {
            _slot = slot;
            IsRender = true;
        }

        public Slot GetSlot() => _slot;

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
                if (enter)
                {
                    v1 = .1953125f;
                    v2 = .29296875f;
                }
                else
                {
                    v1 = .09765625f;
                    v2 = .1953125f;
                }
            }
            else
            {
                v2 = .09765625f;
                v1 = 0;
            }
            _meshBg.Reload(RenderFigure.Rectangle(PosX * si, PosY * si, (PosX + Width) * si, (PosY + Height) * si,
                    v1, .90234375f, v2, 1));

            _posItemX = (PosX + 25) * si;
            _posItemY = (PosY + 25) * si;
            IsRender = false;
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            // Рисуем фон кнопки
            _render.BindTextureHud();
            _meshBg.Draw();
            
            // Рисуем предмет
            if (_slot.Stack != null)
            {
                // Всё 2d закончено, рисуем 3д элементы в Gui
                _render.DepthOn();
                // Заносим в шейдор
                _render.ShsEntity.BindUniformBiginGui();
                window.Game.WorldRender.Entities.GetItemGuiRender(_slot.Stack.Item.IndexItem).MeshDraw(_posItemX, _posItemY);
                _render.DepthOff();
                _render.ShaderBindGuiColor();

                // Рисуем текст кнопки
                //...
            }
        }

        #endregion

        public override void Dispose()
        {
            base.Dispose();
            _meshBg?.Dispose();
        }
    }
}
