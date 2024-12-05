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
        protected readonly Button buttonBack;
        protected readonly Button buttonOptions;
        protected readonly Button buttonExit;

        public ScreenInGameMenu(WindowMain window) : base(window)
        {
            FontBase font = window.Render.FontMain;
            buttonBack = new Button(window, font, 300, L.T("BackGame"));
            buttonBack.Click += ButtonBack_Click;
            buttonOptions = new Button(window, font, 300, L.T("Options"));
            buttonOptions.Click += ButtonOptions_Click;
            buttonExit = new Button(window, font, 300, L.T("ExitGame"));
            buttonExit.Click += ButtonExit_Click;
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
            AddControls(buttonBack);
            AddControls(buttonOptions);
            AddControls(buttonExit);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            int w = Width / 2;
            int h = Height / 2;
            buttonBack.SetPosition(w - buttonBack.Width / 2, h - 40);
            buttonOptions.SetPosition(w - buttonOptions.Width / 2, h + 4);
            buttonExit.SetPosition(w - buttonExit.Width / 2, h + 92);
        }

        //public override void Draw(float timeIndex)
        //{
        //    gl.ClearColor(.5f, .3f, .02f, 1f);
        //    base.Draw(timeIndex);
        //}
    }
}
