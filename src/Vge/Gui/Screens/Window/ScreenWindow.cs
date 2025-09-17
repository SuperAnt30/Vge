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

        /// <summary>
        /// Заголовок окна
        /// </summary>
        protected string _title = "";
        /// <summary>
        /// Прозрачность окна
        /// </summary>
        protected float _alpha = 1f;

        protected ScreenWindow(WindowMain window, int width, int height) : base(window)
        {
            WidthWindow = width;
            HeightWindow = height;
        }

        protected void _Close() => window.LScreen.Close();

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
    }
}
