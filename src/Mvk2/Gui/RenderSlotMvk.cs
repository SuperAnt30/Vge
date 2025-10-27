using Mvk2;
using System.Collections.Generic;
using Vge.Item;
using Vge.Realms;
using Vge.Renderer;
using Vge.Renderer.Font;

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
        private readonly FontBase _font;
        /// <summary>
        /// Имеется ли текст
        /// </summary>
        private bool _isText;
        /// <summary>
        /// Имеется ли урон в предметах
        /// </summary>
        private bool _isDamage;

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
            _font = _windowMvk.Render.FontMain;
            _meshTxt = new MeshGuiColor(_window.GetOpenGL());
            _meshDamage = new MeshGuiColor(_window.GetOpenGL());
        }

        #region Draw

        public override void Rendering()
        {
            _posItemX = 18 * _si;
            _posItemY = 18 * _si;


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
                    _font.RenderString(3 * _si, 3 * _si, ChatStyle.Bolb + Stack.Amount.ToString());
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
                        2 * _si, 30 * _si, 34 * _si, 34 * _si, .16f, .16f, .16f));

                    list.AddRange(RenderFigure.Rectangle(
                        3 * _si, 31 * _si, (3 + line) * _si, 33 * _si,
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
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        public override void Draw()
        {
            // Рисуем предмет
            if (Stack != null)
            {
                // Всё 2d закончено, рисуем 3д элементы в Gui
                // Заносим в шейдор
                _window.Render.ShsEntity.BindUniformBeginGui();
                _window.Game.WorldRender.Entities.GetItemGuiRender(Stack.Item.IndexItem)
                    .MeshDraw(_posItemX + PosX, _posItemY + PosY);

                if (_isText || _isDamage)
                {
                    // Рисуем текст
                    _window.Render.ShaderBindGuiColor(PosX, PosY);
                    _font.BindTexture();
                    if (_isDamage) _meshDamage.Draw();
                    if (_isText) _meshTxt.Draw();
                }

                _window.Render.ShaderBindGuiColor(0, 0);
            }
        }

        #endregion

        public override void Dispose()
        {
            _meshDamage?.Dispose();
            _meshTxt?.Dispose();
        }
    }
}
