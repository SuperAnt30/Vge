using WinGL.Actions;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Контрол для GUI
    /// </summary>
    public abstract class ControlBase
    {
        /// <summary>
        /// Ширина
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Высотаа
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Активный
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// Видимый
        /// </summary>
        public bool Visible { get; set; } = true;
        /// <summary>
        /// Позиция X
        /// </summary>
        public int PositionX { get; set; }
        /// <summary>
        /// Позиция Y
        /// </summary>
        public int PositionY { get; set; }
        /// <summary>
        /// Дополнительный объект
        /// </summary>
        public object Tag { get; set; }
        /// <summary>
        /// Фокус
        /// </summary>
        public bool Focus { get; protected set; } = false;

        /// <summary>
        /// Размер интерфеса
        /// </summary>
        protected readonly int sizeInterface = 1;



        /// <summary>
        /// Нажатие клавиши мышки
        /// </summary>
        public virtual void MouseDown(MouseButton button, int x, int y) { }

        /// <summary>
        /// Отпущена клавиша мышки
        /// </summary>
        public virtual void MouseUp(MouseButton button, int x, int y) { }

    }
}
