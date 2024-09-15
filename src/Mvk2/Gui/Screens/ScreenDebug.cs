using Mvk2.Renderer;
using System.Numerics;
using Vge.Gui.Controls;
using Vge.Gui.Screens;
using Vge.Realms;
using Vge.Renderer;
using Vge.Renderer.Font;
using WinGL.OpenGL;

namespace Mvk2.Gui.Screens
{
    /// <summary>
    /// Заставка
    /// </summary>
    public class ScreenDebug : ScreenBase
    {
        private Label label;
        private Button button;
        private Button button2;
        private Label[] labels = new Label[4];

        public ScreenDebug(WindowMvk window) : base(window)
        {
            FontBase font = window.Render.FontMain;
            label = new Label(window, ((RenderMvk)window.Render).FontLarge, 400, 40, "&nhttp://superant.by/mkv", true);
            label.Click += Label_Click;
            label.SetTextAlight(EnumAlight.Right, EnumAlightVert.Bottom);
            button = new Button(window, font, 360, 40, "Кнопка супер Tag");
            button2 = new Button(window, font, 800, 40, "Test &0Black &6Gold &cRed &9Blue &fWhile &rReset &mStrike&r &lBold &r&oItalic &r&nUnderline");

            string textDebug =
            "&lЗанесло&r меня на остров, . Тут надо добавить длинее строку, чтоб она была подлинее, причём очень.\r\n" +
            "&r&oОжидало много бед.\r\n" +
            "&nЖить на нём совсем не просто,\r\n" +
            "&rА прошло не мало лет.\r\n\r\n" +
            "&9Почти вымерли все звери,\r\n" +
            "&cЯ остался лишь живой.\r\n" +
            "&mИ ходил я всё и думал,\r\n" +
            "&nКак попасть же мне домой.\r\n\r\n" +
            "&rЗанесло меня на остров,\r\n" +
            "Ожидало много бед.\r\n" +
            "&nЖить на &6нём совсем&r не просто,\r\n" +
            "А прошло не мало лет.\r\n\r\n" +
            "Почти вымерли все звери,\r\n" +
            "Я остался лишь живой.\r\n" +
            "И ходил я всё и думал,\r\n" +
            "Как попасть же мне домой.\r\n\r\n" +
            "Тут строка" + ChatStyle.Br + "перенеслась";

            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = new Label(window, font, 280, 256, textDebug, true);
                labels[i].Multiline();
            }

            labels[0].SetTextAlight(EnumAlight.Center, EnumAlightVert.Top);
            labels[1].LimitationHeight().SetTextAlight(EnumAlight.Left, EnumAlightVert.Top);
            labels[2].LimitationHeight().SetTextAlight(EnumAlight.Center, EnumAlightVert.Top);
            labels[3].LimitationHeight().SetTextAlight(EnumAlight.Right, EnumAlightVert.Top);
        }

        private void Label_Click(object sender, System.EventArgs e)
        {
            label.SetText(label.Text + "*");
        }

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            meshTextDebug = new MeshGuiColor(gl);
            AddControls(label);
            AddControls(button);
            AddControls(button2);
            for (int i = 0; i < labels.Length; i++)
            {
                AddControls(labels[i]);
            }
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            // Положение устанваливаем тут, если есть привязка к размеру окна
            int w = Gi.Width / 2 / si;
            
            button.SetPosition(w - button.Width / 2, 40);
            button2.SetPosition(w - button2.Width / 2, 120);
            label.SetPosition(w - label.Width / 2, 200);

            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].SetPosition(i * 300 + 10, 250);
            }
        }

        /// <summary>
        /// Игровой такт
        /// </summary>
        public override void OnTick(float deltaTime)
        {
            // Отладка на экране
            textDebug = window.debug.ToText();
            isTextDebug = true;

            //int x = button.PosX + 1;
            //if (x > 500) x = 100;
            //button.SetPosition(x, button.PosY);

            if (((WindowMvk)window).cursorShow)
            {
            }
        }

        #region TextDebug

        private string textDebug = "";
        private bool isTextDebug = false;
        private MeshGuiColor meshTextDebug;

        /// <summary>
        /// Рендер текста отладки
        /// </summary>
        public void RenderTextDebug()
        {
            if (isTextDebug)
            {
                isTextDebug = false;
                window.Render.FontMain.Clear();
                window.Render.FontMain.SetFontFX(EnumFontFX.Shadow).SetColor(new Vector3(.9f, .9f, .9f));
                window.Render.FontMain.RenderText(10 * Gi.Si, 10 * Gi.Si, textDebug);
                window.Render.FontMain.RenderFX();
                meshTextDebug.Reload(window.Render.FontMain.ToBuffer());
                window.Render.FontMain.Clear();

                //int*[] p = &

                //IntPtr p = new IntPtr(*i);
            }
        }

        /// <summary>
        /// Прорисовка отладки
        /// </summary>
        public void DrawTextDebug()
        {
            window.Render.FontMain.BindTexture();
            meshTextDebug.Draw();
        }

        #endregion

        public int xx = 0;
        public int xx2 = 0;

        public override void Draw(float timeIndex)
        {
            base.Draw(timeIndex);

            if (window.Game == null) gl.ClearColor(.7f, .4f, .4f, 1f);
            //gl.Enable(GL.GL_DEPTH_TEST);
            //gl.PolygonMode(GL.GL_FRONT_AND_BACK, GL.GL_FILL);
            gl.BlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);
            gl.Enable(GL.GL_BLEND);
            gl.Enable(GL.GL_ALPHA_TEST);

            if (window is WindowMvk windowMvk)
            {
                int si = Gi.Si;

                RenderMvk render = windowMvk.Render as RenderMvk;
                render.FontSmall.Clear();
                render.FontMain.Clear();
                render.FontLarge.Clear();

                render.ShaderBindGuiColor();

                RenderTextDebug();
                DrawTextDebug();

                Vector3 cw = new Vector3(.9f, .9f, .9f);

                render.FontSmall.SetColor(cw).SetFontFX(EnumFontFX.Shadow);
                render.FontSmall.RenderString(xx, 200 * si, "-C-");

                if (++xx > 900) xx = 0;

                render.FontSmall.RenderString(xx2, 220 * si, "-S-");

                //if (++xx2 > 900) xx2 = 0;

                int width = window.Width;
                int height = window.Height;

                // Version
                render.FontLarge.SetColor(new Vector3(0.6f, 0.9f, .9f)).SetFontFX(EnumFontFX.Outline);
                int w = render.FontLarge.WidthString(window.Version) * si;
                render.FontLarge.RenderString(width - w - 10 * si, height - 19 * si, window.Version);

                string str;
                // fps
                //string str = "FPS " + window.Fps.ToString() + " TPS " + window.Tps.ToString();
                //FontMain.RenderString(11, height - 18, str, bg);
                //FontMain.RenderString(10, height - 19, str, cw);

               
                // XYZ
                w = 190 * si;
                str = window.Width + " " + window.Height;
                if (window.VSync) str += " VSync";
                render.FontMain.SetColor(cw).SetFontFX(EnumFontFX.Shadow);
                //render.FontMain.RenderString(w + 1 * si, height - 18 * si, str, bg);
                render.FontMain.RenderString(w, height - 19 * si, str);

                // XY
                w = 400 * si;
                str = "XY";
                //render.FontMain.RenderString(w + 1 * si, height - 18 * si, str, bg);
                render.FontMain.RenderString(w, height - 19 * si, str);
                w = 430 * si;
                str = window.MouseX.ToString("0.0");
                //render.FontMain.RenderString(w + 1 * si, height - 18 * si, str, bg);
                render.FontMain.RenderString(w, height - 19 * si, str);
                w = 490 * si;
                str = window.MouseY.ToString("0.0");
                //render.FontMain.RenderString(w + 1 * si, height - 18 * si, str, bg);
                render.FontMain.RenderString(w, height - 19 * si, str);

                //textDb
                //if (textDb != "")
                //{
                //    FontMain.RenderString(11, height - 38, textDb, bg);
                //    FontMain.RenderString(10, height - 39, textDb, cw);
                //}

                // Draw

                render.FontSmall.BindTexture();
                render.FontSmall.RenderFX();
                render.FontSmall.ReloadDraw();
                render.FontMain.BindTexture();
                render.FontMain.RenderFX();
                render.FontMain.ReloadDraw();
                render.FontLarge.BindTexture();
                render.FontLarge.RenderFX();
                render.FontLarge.ReloadDraw();

                if (windowMvk.cursorShow)
                {
                   
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            meshTextDebug.Dispose();
        }
    }
}
