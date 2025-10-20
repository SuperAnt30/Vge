using Vge.Gui.Controls;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран процесс выполнения
    /// </summary>
    public class ScreenProcess : ScreenBase
    {
        protected readonly Label label;

        public ScreenProcess(WindowMain window, string text) : base(window)
        {
            label = new Label(window, window.Render.FontMain, text);
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
            label.SetSize(Width, Height);
        }

        public override void Draw(float timeIndex)
        {
            //gl.ClearColor(.5f, .3f, .02f, 1f);
            gl.ClearColor(.486f, .569f, .616f, 1f);
            //gl.ClearColor(.827f, .796f, .745f, 1f);
            base.Draw(timeIndex);
        }
    }
}
