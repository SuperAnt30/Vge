using Vge.Gui.Controls;
using Vge.Renderer.Font;
using WinGL.Actions;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран меню во время игры
    /// </summary>
    public class ScreenInGameMenu : ScreenBase
    {
        protected readonly ButtonWide _buttonBack;
        protected readonly ButtonWide _buttonOptions;
        protected readonly ButtonWide _buttonExit;

        public ScreenInGameMenu(WindowMain window) : base(window)
        {
            FontBase font = window.Render.FontMain;
            _buttonBack = new ButtonWide(window, font, 340, L.T("BackGame"));
            _buttonBack.Click += ButtonBack_Click;
            _buttonOptions = new ButtonWide(window, font, 340, L.T("Options"));
            _buttonOptions.Click += ButtonOptions_Click;
            _buttonExit = new ButtonWide(window, font, 340, L.T("ExitGame"));
            _buttonExit.Click += ButtonExit_Click;
        }

        #region Clicks

        private void ButtonBack_Click(object sender, System.EventArgs e)
            => CloseMenu();

        private void ButtonOptions_Click(object sender, System.EventArgs e)
            => window.LScreen.Options(this, true);

        private void ButtonExit_Click(object sender, System.EventArgs e)
            => window.GameStoping();

        #endregion

        public override void OnKeyDown(Keys keys)
        {
            //base.OnKeyDown(keys);
            if (keys == Keys.Escape || keys == Keys.Menu)
            {
                CloseMenu();
            }
        }

        /// <summary>
        /// Закрыть меню
        /// </summary>
        protected void CloseMenu()
        {
            if (window.Game != null)
            {
                window.Game.SetGamePauseSingle(false);
                window.ScreenClose();
            }
        }

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            AddControls(_buttonBack);
            AddControls(_buttonOptions);
            AddControls(_buttonExit);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            int w = Width / 2;
            int h = Height / 2;
            _buttonBack.SetPosition(w - _buttonBack.Width / 2, h - 64);
            _buttonOptions.SetPosition(w - _buttonOptions.Width / 2, h);
            _buttonExit.SetPosition(w - _buttonExit.Width / 2, h + 92);
        }
    }
}
