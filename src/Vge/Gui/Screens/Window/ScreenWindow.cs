using System;
using Vge.Gui.Controls;
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
        protected Button _buttonCancel;

        /// <summary>
        /// Сетка фона
        /// </summary>
        protected readonly MeshGuiColor _meshBg;

        protected ScreenWindow(WindowMain window, int width, int height) : base(window)
        {
            WidthWindow = width;
            HeightWindow = height;
            _meshBg = new MeshGuiColor(gl);

            _InitTitle();
            _buttonCancel.Click += ButtonCancel_Click;
        }

        protected virtual void _InitTitle() { }

        private void ButtonCancel_Click(object sender, EventArgs e) => _Close();

        protected void _Close() => window.LScreen.Close();

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            AddControls(_labelTitle);
            AddControls(_buttonCancel);
        }

        /// <summary>
        /// Курсор за пределами окна
        /// </summary>
        protected virtual bool _IsOutsideWindow(int x, int y)
            => x < PosX * si || x > (PosX + WidthWindow) * si
            || y < PosY * si || y > (PosY + HeightWindow) * si;

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

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            base.OnResized();
            _isRenderAdd = true;
        }

        protected override void _RenderingAdd()
        {
            _meshBg.Reload(RenderFigure.Rectangle(PosX * si, PosY * si,
                (PosX + WidthWindow) * si, (PosY + HeightWindow) * si,
                0, 0, 1, HeightWindow / (float)WidthWindow));
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
