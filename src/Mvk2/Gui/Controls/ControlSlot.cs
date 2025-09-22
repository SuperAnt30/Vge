using Mvk2.Renderer;
using System;
using Vge.Entity.Inventory;
using Vge.Gui.Controls;
using Vge.Item;
using Vge.Renderer;
using Vge.Renderer.Font;
using WinGL.Actions;

namespace Mvk2.Gui.Controls
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
        /// Стак
        /// </summary>
        public ItemStack Stack { get; private set; }

        private readonly RenderMvk _render;

        /// <summary>
        /// Сетка фона
        /// </summary>
        private readonly MeshGuiColor _meshBg;
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

        public ControlSlot(WindowMvk window, byte sloyId, ItemStack stack) : base(window)
        {
            _render = window.GetRender();
            SetSize(50, 50).SetText("");
            SlotId = sloyId;
            Stack = stack;
            _meshBg = new MeshGuiColor(gl);
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
        public override void OnMouseDown(MouseButton button, int x, int y)
        {
            if (button == MouseButton.Left)
            {
                OnMouseMove(x, y);
                if (enter) _OnClickLeft();
            }
            else if (button == MouseButton.Right)
            {
                OnMouseMove(x, y);
                if (enter) _OnClickRight();
            }
        }

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
            // Рисуем фон кнопки
            _render.BindTextureHud();
            _meshBg.Draw();
            
            // Рисуем предмет
            if (Stack != null)
            {
                // Всё 2d закончено, рисуем 3д элементы в Gui
                _render.DepthOn();
                // Заносим в шейдор
                _render.ShsEntity.BindUniformBiginGui();
                window.Game.WorldRender.Entities.GetItemGuiRender(Stack.Item.IndexItem).MeshDraw(_posItemX, _posItemY);
                _render.DepthOff();
                _render.ShaderBindGuiColor();

                if (_isText)
                {
                    // Рисуем текст
                    _font.BindTexture();
                    _meshTxt.Draw();
                }
            }
        }

        #endregion

        public override void Dispose()
        {
            base.Dispose();
            _meshBg?.Dispose();
            _meshTxt?.Dispose();
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
