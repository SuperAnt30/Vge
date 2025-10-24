using Vge.Item;
using Vge.Realms;
using Vge.Renderer;
using Vge.Renderer.Font;

namespace Vge.Gui.Controls
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

        public ControlIcon(WindowMain window, FontBase font, ItemStack stack) : base(window)
        {
            SetSize(36, 36).SetText("");
            Stack = stack;
            _meshTxt = new MeshGuiColor(gl);
            _font = font;
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
            _mouseX = x - 18 * _si;
            _mouseY = y - 18 * _si;
        }

        #endregion

        #region Draw

        public override void Rendering()
        {
            _posItemX = (PosX + 18) * _si;
            _posItemY = (PosY + 18) * _si;

            // Ренедер текста
            if (Stack != null && Stack.Amount != 1)
            {
                // Чистим буфер
                _font.Clear(true, true);
                _font.SetColorShadow(Gi.ColorTextBlack);
                // Указываем опции
                _font.SetFontFX(EnumFontFX.Outline);
                // Готовим рендер текста
                _font.RenderString((PosX + 3) * _si, (PosY + 23) * _si, ChatStyle.Bolb + Stack.Amount.ToString());
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
                window.Render.ShsEntity.BindUniformBeginGui();
                window.Game.WorldRender.Entities.GetItemGuiRender(Stack.Item.IndexItem)
                    .MeshDraw(_posItemX + _mouseX, _posItemY + _mouseY);
                
                if (_isText)
                {
                    // Рисуем текст
                    window.Render.ShaderBindGuiColor(_mouseX, _mouseY);
                    _font.BindTexture();
                    _meshTxt.Draw();
                }

                window.Render.ShaderBindGuiColor(0, 0);
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
