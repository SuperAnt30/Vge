using Vge.Renderer;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Абстрактный класс экрана
    /// </summary>
    public abstract class ScreenBase : RenderBase
    {

        public ScreenBase(WindowMain window) : base(window) => Initialize();

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        public virtual void Initialize() { }

    }
}
