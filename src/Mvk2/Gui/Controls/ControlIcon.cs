using Mvk2.Renderer;
using Vge.Gui.Controls;
using Vge.Item;
using Vge.Renderer;
using Vge.Renderer.Font;

namespace Mvk2.Gui.Controls
{
    /// <summary>
    /// Контрол оконка предмета, нужна для визуализации восле мышки и на контроле
    /// </summary>
    public class ControlIcon : WidgetBase
    {
        /// <summary>
        /// Стак
        /// </summary>
        public ItemStack Stack { get; protected set; }

        protected readonly RenderMvk _render;

        /// <summary>
        /// Сетка текста
        /// </summary>
        private readonly MeshGuiColor _meshTxt;
        /// <summary>
        /// Объект шрифта
        /// </summary>
        public readonly FontBase _font;
        /// <summary>
        /// Имеется ли текст
        /// </summary>
        public bool _isText;

        /// <summary>
        /// Расположение предмета по Х
        /// </summary>
        private int _posItemX;
        /// <summary>
        /// Расположение предмета по Y
        /// </summary>
        private int _posItemY;

        private float _mouseX;
        private float _mouseY;

        public ControlIcon(WindowMvk window, ItemStack stack) : base(window)
        {
            _render = window.GetRender();
            SetSize(50, 50).SetText("");
            Stack = stack;
            _meshTxt = new MeshGuiColor(gl);
            _font = window.GetRender().FontSmall;
        }

        /// <summary>
        /// Задать новый или изменённый стак
        /// </summary>
        public void SetStack(ItemStack stack)
        {
            Stack = stack;
            IsRender = true;
        }

        #region OnMouse

        /// <summary>
        /// Нажатие клавиши мышки
        /// </summary>
        public override void OnMouseMove(int x, int y)
        {
            _mouseX = x - 25 * si;
            _mouseY = y - 25 * si;
        }

        #endregion

        #region Draw

        public override void Rendering()
        {
            _posItemX = (PosX + 25) * si;
            _posItemY = (PosY + 25) * si;

            // Ренедер текста
            if (Stack != null && Stack.Amount != 1)
            {
                // Чистим буфер
                _font.Clear();
                // Указываем опции
                _font.SetFontFX(EnumFontFX.Outline);
                // Готовим рендер текста
                _font.RenderString((PosX + 7) * si, (PosY + 36) * si, Stack.Amount.ToString());
                // Имеется Outline значит рендерим FX
                _font.RenderFX();
                // Вносим сетку
                _font.Reload(_meshTxt);
                _isText = true;
            }
            else
            {
                _isText = false;
            }

            IsRender = false;
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            // Рисуем предмет
            if (Stack != null)
            {
                // Всё 2d закончено, рисуем 3д элементы в Gui
                // Заносим в шейдор
                _render.ShsEntity.BindUniformBeginGui();
                window.Game.WorldRender.Entities.GetItemGuiRender(Stack.Item.IndexItem)
                    .MeshDraw(_posItemX + _mouseX, _posItemY + _mouseY);
                
                if (_isText)
                {
                    // Рисуем текст
                    _render.ShaderBindGuiColor(_mouseX, _mouseY);
                    _font.BindTexture();
                    _meshTxt.Draw();
                }
                _render.ShaderBindGuiColor(0, 0);
            }
        }

        #endregion

        public override void Dispose()
        {
            base.Dispose();
            _meshTxt?.Dispose();
        }
    }
}
