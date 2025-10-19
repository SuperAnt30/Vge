using Vge.Gui.Screens;

namespace Mvk2.Gui.Screens
{
    public class ScreenMainMenuMvk : ScreenMainMenu
    {
        public ScreenMainMenuMvk(WindowMvk window) : base(window)
        {
            _buttonSingle.SetFont(window.GetRender().FontLarge);
            _buttonMultiplayer.SetFont(window.GetRender().FontLarge);
            _buttonOptions.SetFont(window.GetRender().FontLarge);
            _buttonExit.SetFont(window.GetRender().FontLarge);
        }
    }
}
