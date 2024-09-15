using Vge.Gui.Controls;
using Vge.Renderer.Font;
using Vge.Util;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран основного меню 
    /// </summary>
    public class ScreenMainMenu : ScreenBase
    {
        protected readonly Button buttonSingle;
        protected readonly Button buttonMultiplayer;
        protected readonly Button buttonOptions;
        protected readonly Button buttonExit;

        public ScreenMainMenu(WindowMain window) : base(window)
        {
            FontBase font = window.Render.FontMain;
            buttonSingle = new Button(window, font, 300, 40, L.T("Single"));
            buttonSingle.Click += ButtonSingle_Click;
            buttonMultiplayer = new Button(window, font, 300, 40, L.T("Multiplayer"));
            buttonMultiplayer.Click += ButtonMultiplayer_Click;
            buttonOptions = new Button(window, font, 300, 40, L.T("Options"));
            buttonOptions.Click += ButtonOptions_Click;
            buttonExit = new Button(window, font, 300, 40, L.T("Exit"));
            buttonExit.Click += ButtonExit_Click;
        }

        #region Clicks

        private void ButtonSingle_Click(object sender, System.EventArgs e)
        {
            window.GameLocalRun();
        }

        private void ButtonMultiplayer_Click(object sender, System.EventArgs e)
        {
            window.GameNetRun(Options.IpAddress, 32021);
        }

        private void ButtonOptions_Click(object sender, System.EventArgs e)
        {
            window.ScreenOptions();
        }

        private void ButtonExit_Click(object sender, System.EventArgs e)
        {
            window.Exit();
        }

        #endregion

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            AddControls(buttonSingle);
            AddControls(buttonMultiplayer);
            AddControls(buttonOptions);
            AddControls(buttonExit);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            buttonSingle.SetPosition((window.Width / si - buttonSingle.Width) / 2,
                window.Height / (si * 2) - 40);
            buttonMultiplayer.SetPosition((window.Width / si - buttonMultiplayer.Width) / 2,
                window.Height / (si * 2) + 4);
            buttonOptions.SetPosition((window.Width / si - buttonOptions.Width) / 2,
                window.Height / (si * 2) + 48);
            buttonExit.SetPosition((window.Width / si - buttonExit.Width) / 2,
                window.Height / (si * 2) + 92);

        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.5f, .3f, .02f, 1f);
            base.Draw(timeIndex);
        }
    }
}
