using Vge.Gui.Controls;
using Vge.Renderer.Font;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран основного меню 
    /// </summary>
    public class ScreenMainMenu : ScreenBase
    {
        protected readonly ButtonWide _buttonSingle;
        protected readonly ButtonWide _buttonMultiplayer;
        protected readonly ButtonWide _buttonOptions;
        protected readonly ButtonWide _buttonExit;

        public ScreenMainMenu(WindowMain window) : base(window)
        {
            FontBase font = window.Render.FontMain;
            _buttonSingle = new ButtonWide(window, font, 340, L.T("Single"));
            _buttonSingle.Click += ButtonSingle_Click;
            _buttonMultiplayer = new ButtonWide(window, font, 340, L.T("Multiplayer"));
            _buttonMultiplayer.Click += ButtonMultiplayer_Click;
            _buttonOptions = new ButtonWide(window, font, 340, L.T("Options"));
            _buttonOptions.Click += ButtonOptions_Click;
            _buttonExit = new ButtonWide(window, font, 340, L.T("Exit"));
            _buttonExit.Click += ButtonExit_Click;
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
            AddControls(_buttonSingle);
            AddControls(_buttonMultiplayer);
            AddControls(_buttonOptions);
            AddControls(_buttonExit);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            int w = Width / 2 - _buttonSingle.Width / 2;
            int h = Height / 2;
            _buttonSingle.SetPosition(w, h - 56);
            _buttonMultiplayer.SetPosition(w, h);
            _buttonOptions.SetPosition(w, h + 56);
            _buttonExit.SetPosition(w, h + 112);
        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.486f, .569f, .616f, 1f);
            //gl.ClearColor(.827f, .796f, .745f, 1f);
            base.Draw(timeIndex);
        }
    }
}
