using Mvk2;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран выбора одиночной игры
    /// </summary>
    public class ScreenSingleMvk : ScreenSingle
    {
        public ScreenSingleMvk(WindowMvk window) : base(window)
            => label.SetFont(window.GetRender().FontLarge);
    }
}
