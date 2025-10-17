using Vge.Gui.Controls;
using Vge.Realms;
using Vge.Renderer.Font;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран основного меню 
    /// </summary>
    public class ScreenMainMenu : ScreenBase
    {
        protected readonly Button128 buttonSingle;
        protected readonly Button128 buttonMultiplayer;
        protected readonly Button128 buttonOptions;
        protected readonly Button128 buttonExit;

        public ScreenMainMenu(WindowMain window) : base(window)
        {
            FontBase font = window.Render.FontMain;
            buttonSingle = new Button128(window, font, ChatStyle.Bolb + L.T("Single"));
            buttonSingle.Click += ButtonSingle_Click;
            buttonMultiplayer = new Button128(window, font, ChatStyle.Bolb + L.T("Multiplayer"));
            buttonMultiplayer.Click += ButtonMultiplayer_Click;
            buttonOptions = new Button128(window, font, ChatStyle.Bolb + L.T("Options"));
            buttonOptions.Click += ButtonOptions_Click;
            buttonExit = new Button128(window, font, ChatStyle.Bolb + L.T("Exit"));
            buttonExit.Click += ButtonExit_Click;
            buttonExit.SetEnable(false);
        }

        #region Clicks

        private void ButtonSingle_Click(object sender, System.EventArgs e)
            => window.LScreen.Single();

        private void ButtonMultiplayer_Click(object sender, System.EventArgs e)
            => window.LScreen.Multiplayer();

        private void ButtonOptions_Click(object sender, System.EventArgs e)
            => window.LScreen.Options(this, false);

        private void ButtonExit_Click(object sender, System.EventArgs e)
            => window.Exit();

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
            int w = Width / 2 - 64;
            int h = Height / 2;
            buttonSingle.SetPosition(w, h);
            buttonMultiplayer.SetPosition(w, h + 32);
            buttonOptions.SetPosition(w, h + 64);
            buttonExit.SetPosition(w, h + 96);
        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.5f, .3f, .02f, 1f);
            base.Draw(timeIndex);
        }
    }
}
