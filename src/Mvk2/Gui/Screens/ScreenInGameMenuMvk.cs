using Vge.Gui.Screens;

namespace Mvk2.Gui.Screens
{
    public class ScreenInGameMenuMvk : ScreenInGameMenu
    {
        public ScreenInGameMenuMvk(WindowMvk window) : base(window)
        {
            _buttonBack.SetFont(window.GetRender().FontLarge);
            _buttonOptions.SetFont(window.GetRender().FontLarge);
            _buttonExit.SetFont(window.GetRender().FontLarge);
        }
    }
}
