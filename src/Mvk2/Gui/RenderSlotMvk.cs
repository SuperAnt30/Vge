using Mvk2;
using Mvk2.Gui;
using Vge.Item;

namespace Vge.Gui
{
    /// <summary>
    /// Рендер слота для игры Mvk 2
    /// </summary>
    public class RenderSlotMvk : RenderSlot
    {
        /// <summary>
        /// Объект окна
        /// </summary>
        protected readonly WindowMvk _windowMvk;
        /// <summary>
        /// Рендер слотов
        /// </summary>
        private readonly RenderSlots _renderSlots;

        /// <summary>
        /// Расположение предмета по Х для рисунка предмета
        /// </summary>
        private int _posItemX;
        /// <summary>
        /// Расположение предмета по Y для рисунка предмета
        /// </summary>
        private int _posItemY;

        public RenderSlotMvk(WindowMvk window, ItemStack stack)
            : base(window, stack)
        {
            _windowMvk = window;
            _renderSlots = new RenderSlots(_windowMvk.GetRender());
        }

        #region Draw

        public override void Rendering()
        {
            _posItemX = 18 * _si;
            _posItemY = 18 * _si;

            _renderSlots.Clear();

            if (Stack != null)
            {
                // Ренедер текста
                if (Stack.Amount != 1)
                {
                    _renderSlots.BeforeRenderText();
                    _renderSlots.TextRender(Stack);
                }

                // Ренедер урона
                _renderSlots.DamageRender(Stack, 0, 0);
            }
            else
            {
                _renderSlots.Clear();
            }

            _renderSlots.AfterRender();
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        public override void Draw()
        {
            // Рисуем предмет
            if (Stack != null)
            {
                // Заносим в шейдор
                _window.Render.ShsEntity.BindUniformBeginGui();

                _window.Game.WorldRender.Entities.GetItemGuiRender(Stack.Item.IndexItem)
                    .MeshDraw(_posItemX + PosX, _posItemY + PosY);

                _renderSlots.Draw(PosX, PosY);

                _window.Render.ShaderBindGuiColor(0, 0);
            }
        }

        #endregion

        public override void Dispose() => _renderSlots.Dispose();
    }
}
