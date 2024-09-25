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

        protected readonly Label label;
        protected readonly Slider sliderFps;
        protected readonly Slider sliderChunk;
        protected readonly CheckBox checkBox;
        protected readonly CheckBox checkBox2;

        protected readonly Button buttonDone;
        protected readonly Button buttonCancel;

        public ScreenOptions(WindowMain window, ScreenBase parent, bool inGame) : base(window)
        {
            this.inGame = inGame;
            this.parent = parent;
            FontBase font = window.Render.FontMain;

            label = new Label(window, font, L.T("Op&4tio&3ns"));
            label.SetTextAlight(EnumAlight.Center, EnumAlightVert.Bottom);
            buttonDone = new Button(window, font, 300, L.T("Done"));
            buttonDone.Click += ButtonDone_Click;
            buttonCancel = new Button(window, font, 300, L.T("Cancel"));
            buttonCancel.Click += ButtonCancel_Click;

            sliderFps = new Slider(window, font, 300, 10, 260, 10, L.T("Fps")) { Value = 60 };
            sliderFps.AddParam(260, L.T("MaxFps"));
            sliderChunk = new Slider(window, font, 300, 2, 32, 1, L.T("Ove&3rviewChunks")) { Value = 16 };
            checkBox = new CheckBox(window, font, 300, "Check");
            checkBox2 = new CheckBox(window, font, 300, "Check2");
            checkBox2.SetCheck(true).SetEnable(false);
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
            AddControls(label);
            AddControls(buttonDone);
            AddControls(buttonCancel);

            AddControls(sliderFps);
            AddControls(sliderChunk);
            AddControls(checkBox);
            AddControls(checkBox2);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            int w = Width / 2;
            int h = Height / 2;
            label.SetSize(Width - 100, label.Height).SetPosition(50, h - label.Height - 200);

            buttonDone.SetPosition(w - buttonDone.Width - 2, h + 92);
            buttonCancel.SetPosition(w + 2, h + 92);

            sliderFps.SetPosition(w - sliderFps.Width - 2, h + 42);
            sliderChunk.SetPosition(w + 2, h + 42);

            checkBox.SetPosition(w - checkBox.Width - 2, h);
            checkBox2.SetPosition(w + 2, h);
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
