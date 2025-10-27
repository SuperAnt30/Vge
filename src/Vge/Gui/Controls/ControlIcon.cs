using System.Collections.Generic;
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
        /// Сетка уровня урона
        /// </summary>
        private readonly MeshGuiColor _meshDamage;
        /// <summary>
        /// Объект шрифта
        /// </summary>
        public readonly FontBase _font;
        /// <summary>
        /// Имеется ли текст
        /// </summary>
        public bool _isText;
        /// <summary>
        /// Имеется ли урон в предметах
        /// </summary>
        private bool _isDamage;

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
            _meshDamage = new MeshGuiColor(gl);
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
            

            if (Stack != null)
            {
                // Ренедер текста
                if (Stack.Amount != 1)
                {
                    // Чистим буфер
                    _font.Clear(true, true);
                    _font.SetColorShadow(Gi.ColorTextBlack);
                    // Указываем опции
                    _font.SetFontFX(EnumFontFX.Outline);
                    // Готовим рендер текста
                    _font.RenderString((PosX + 3) * _si, (PosY + 3) * _si, ChatStyle.Bolb + Stack.Amount.ToString());
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

                // Ренедер урона
                //if (Stack.Amount != 1)
                if (Stack.Item.MaxDamage != 0)
                {
                    int line = Stack.ItemDamage * 30 / Stack.Item.MaxDamage;
                    //int line = Stack.Amount * 30 / Stack.Item.MaxStackSize;
                    if (line > 30) line = 30;

                    List<float> list = new List<float>(RenderFigure.Rectangle(
                        (PosX + 2) * Gi.Si, (PosY + 30) * Gi.Si, (PosX + 34) * Gi.Si, (PosY + 34) * Gi.Si, .16f, .16f, .16f));

                    list.AddRange(RenderFigure.Rectangle(
                        (PosX + 3) * Gi.Si, (PosY + 31) * Gi.Si, (PosX + 3 + line) * Gi.Si, (PosY + 33) * Gi.Si,
                        line > 15 ? (30 - line) / 15f : 1,
                        line < 15 ? line / 15f : 1,
                        .16f));

                    _meshDamage.Reload(list.ToArray());
                    _isDamage = true;
                }
                else
                {
                    _isDamage = false;
                }
            }
            else
            {
                _isDamage = false;
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

                if (_isText || _isDamage)
                {
                    // Рисуем текст
                    window.Render.ShaderBindGuiColor(_mouseX, _mouseY);
                    _font.BindTexture();
                    if (_isDamage) _meshDamage.Draw();
                    if (_isText) _meshTxt.Draw();
                }

                window.Render.ShaderBindGuiColor(0, 0);
            }
        }

        #endregion

        public override void Dispose()
        {
            base.Dispose();
            _meshDamage?.Dispose();
            _meshTxt?.Dispose();
        }
    }
}
