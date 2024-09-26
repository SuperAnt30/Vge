using Vge.Gui.Screens;

namespace Mvk2.Gui.Screens
{
    public class ScreenOptionsMvk : ScreenOptions
    {
        public ScreenOptionsMvk(WindowMvk window, ScreenBase parent, bool inGame) 
            : base(window, parent, inGame)
        {
            label.SetFont(window.GetRender().FontLarge);
        }
    }
}
