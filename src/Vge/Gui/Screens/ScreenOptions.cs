using Vge.Gui.Controls;
using Vge.Renderer.Font;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран настроек
    /// </summary>
    public class ScreenOptions : ScreenBase
    {
        protected readonly Button buttonDone;
        protected readonly Button buttonCancel;

        public ScreenOptions(WindowMain window) : base(window)
        {
            FontBase font = window.Render.FontMain;
            
            buttonDone = new Button(window, font, 300, 40, "Done");
            buttonDone.Click += ButtonDone_Click;
            buttonCancel = new Button(window, font, 300, 40, "Cancel");
            buttonCancel.Click += ButtonCancel_Click;
        }

        #region Clicks

        private void ButtonDone_Click(object sender, System.EventArgs e)
        {
            window.ScreenMainMenu();
        }

        private void ButtonCancel_Click(object sender, System.EventArgs e)
        {
            window.ScreenMainMenu();
        }

        #endregion

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            AddControls(buttonDone);
            AddControls(buttonCancel);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            buttonDone.SetPosition(window.Width / (si * 2) - (buttonDone.Width + 2),
                window.Height / (si * 2) + 92);
            buttonCancel.SetPosition(window.Width / (si * 2) + 2,
                window.Height / (si * 2) + 92);
        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.5f, .3f, .02f, 1f);
            base.Draw(timeIndex);
        }
    }
}
