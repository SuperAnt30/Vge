using Vge.Renderer;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Абстрактный класс экрана
    /// </summary>
    public abstract class ScreenBase : RenderBase
    {
        /// <summary>
        /// Размер интерфеса
        /// </summary>
        public int SizeInterface { get; private set; }
        
        public ScreenBase(WindowMain window) : base(window) { }

    }
}
