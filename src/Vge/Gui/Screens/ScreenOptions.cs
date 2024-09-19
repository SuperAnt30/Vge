using Vge.Gui.Controls;
using Vge.Renderer.Font;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран настроек
    /// </summary>
    public class ScreenOptions : ScreenBase
    {
        private readonly ScreenBase parent;
        /// <summary>
        /// Опции во время игры
        /// </summary>
        private readonly bool inGame;

        protected readonly Button buttonDone;
        protected readonly Button buttonCancel;

        public ScreenOptions(WindowMain window, ScreenBase parent, bool inGame) : base(window)
        {
            this.inGame = inGame;
            this.parent = parent;
            FontBase font = window.Render.FontMain;
            buttonDone = new Button(window, font, 300, L.T("Done"));
            buttonDone.Click += ButtonDone_Click;
            buttonCancel = new Button(window, font, 300, L.T("Cancel"));
            buttonCancel.Click += ButtonCancel_Click;
        }

        #region Clicks

        private void ButtonDone_Click(object sender, System.EventArgs e)
        {
            //string s = "&r &oОжидало много бед.\r\n" +
            //"&nЖить на нём совсем не просто,\r\n" +
            //"&rА прошло не мало лет.\r\n\r\n" +
            //"&9Почти вымерли все звери,\r\n" +
            //"&cЯ остался лишь живой.\r\n" +
            //"&mИ ходил я всё и думал,\r\n" +
            //"&nКак попасть же мне домой.\r\n\r\n" +
            //"&rЗанесло меня на остров,\r\n" +
            //"Ожидало много бед.";

            //window.LScreen.YesNo(this, s);
            // Ответ ловим в методе LaunchFromParent

            window.LScreen.Parent(parent, EnumScreenParent.Yes);
        }

        private void ButtonCancel_Click(object sender, System.EventArgs e)
            => window.LScreen.Parent(parent, EnumScreenParent.None);

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
            int w = Width / 2;
            int h = Height / 2;
            buttonDone.SetPosition(w - buttonDone.Width - 2, h + 92);
            buttonCancel.SetPosition(w + 2, h + 92);
        }

        public override void Draw(float timeIndex)
        {
            if (!inGame)
            {
                gl.ClearColor(.5f, .3f, .02f, 1f);
            }
            base.Draw(timeIndex);
        }
    }
}
