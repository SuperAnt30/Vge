using Vge.Gui.Controls;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран соединения
    /// </summary>
    public class ScreenConnection : ScreenBase
    {
        protected readonly Label label;

        public ScreenConnection(WindowMain window) : base(window)
        {
            label = new Label(window, window.Render.FontMain, 0, 0, L.T("Connection") + Ce.Ellipsis);
            label.SetTextAlight(EnumAlight.Center, EnumAlightVert.Middle);
        }

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            AddControls(label);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            label.SetSize(Width, Height).SetPosition(0, 0);
        }
    }
}
