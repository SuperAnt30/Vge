using System;
using System.Runtime.CompilerServices;
using Vge.Gui.Controls;
using Vge.Realms;
using Vge.Renderer;
using WinGL.Actions;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Абстрактный класс экрана которое создаёт окно
    /// </summary>
    public abstract class ScreenWindow : ScreenBase
    {
        /// <summary>
        /// Ширина окна
        /// </summary>
        public readonly int WidthWindow;
        /// <summary>
        /// Высотаа окна
        /// </summary>
        public readonly int HeightWindow;
        /// <summary>
        /// Позиция X
        /// </summary>
        public int PosX { get; protected set; }
        /// <summary>
        /// Позиция Y
        /// </summary>
        public int PosY { get; protected set; }

        protected Label _labelTitle;
        protected ButtonClose _buttonClose;

        /// <summary>
        /// Сетка фона
        /// </summary>
        protected readonly MeshGuiColor _meshBg;
        /// <summary>
        /// Размер текстуры фона, квадрат в пикселах
        /// </summary>
        protected readonly float _sizeBg;

        protected ScreenWindow(WindowMain window, float sizeBg, int width, int height, bool closeHide = false) : base(window)
        {
            WidthWindow = width;
            HeightWindow = height;
            _sizeBg = sizeBg;
            _meshBg = new MeshGuiColor(gl);

            _buttonClose = new ButtonClose(window);
            if (!closeHide)
            {
                _buttonClose.Click += ButtonCancel_Click;
            }
            else
            {
                _buttonClose.SetVisible(false);
            }

            _InitTitle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _InitTitle()
        {
            _labelTitle = new Label(window, window.Render.FontMain, 128, 16,
                ChatStyle.Bolb + _GetTitle() + ChatStyle.Reset);
            _labelTitle.SetTextAlight(EnumAlight.Left, EnumAlightVert.Top);
        }

        /// <summary>
        /// Название заголовка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual string _GetTitle() => "Малювекi";

        private void ButtonCancel_Click(object sender, EventArgs e) => _Close();

        /// <summary>
        /// Закрытие скрина
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _Close() => window.LScreen.Close();

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            AddControls(_labelTitle);
            AddControls(_buttonClose);
        }

        /// <summary>
        /// Курсор за пределами окна
        /// </summary>
        protected virtual bool _IsOutsideWindow(int x, int y)
            => x < PosX * _si || x > (PosX + WidthWindow) * _si
            || y < PosY * _si || y > (PosY + HeightWindow) * _si;

        /// <summary>
        /// Клик за пределами окна
        /// </summary>
        protected virtual void _OnClickOutsideWindow() => _Close();

        /// <summary>
        /// Нажатие курсора мыши
        /// </summary>
        public override void OnMouseDown(MouseButton button, int x, int y)
        {
            if (_IsOutsideWindow(x, y))
            {
                _OnClickOutsideWindow();
            }
            else
            {
                base.OnMouseDown(button, x, y);
            }
        }

        public override void OnKeyDown(Keys keys)
        {
            if (keys == Keys.Escape)
            {
                _Close();
            }
            else
            {
                base.OnKeyDown(keys);
            }
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            base.OnResized();
            _buttonClose.SetPosition(PosX + WidthWindow - 20, PosY + 4);
            _labelTitle.SetPosition(PosX + 14, PosY + 9);
            _isRenderAdd = true;
        }

        protected override void _RenderingAdd()
        {
            _meshBg.Reload(RenderFigure.Rectangle(PosX * _si, PosY * _si,
                (PosX + WidthWindow) * _si, (PosY + HeightWindow) * _si,
                0, 0, WidthWindow / _sizeBg, HeightWindow / _sizeBg));
            _isRenderAdd = false;
        }

        protected override void _DrawAdd()
        {
            _BindTextureBg();
            _meshBg.Draw();
        }

        /// <summary>
        /// Запустить текстуру фона
        /// </summary>
        protected virtual void _BindTextureBg() { }

        public override void Dispose()
        {
            base.Dispose();
            _meshBg?.Dispose();
        }
    }
}
