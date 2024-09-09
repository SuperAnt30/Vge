using Mvk2.Renderer;
using System.Numerics;
using Vge.Gui.Controls;
using Vge.Gui.Screens;
using Vge.Renderer;
using WinGL.OpenGL;

namespace Mvk2.Gui.Screens
{
    /// <summary>
    /// Заставка
    /// </summary>
    public class ScreenDebug : ScreenBase
    {
        /// <summary>
        /// Объект сетки курсора, временно
        /// </summary>
        private Mesh cursorVBO;

        private Label label;
        private Button button;
        private Button button2;

        public ScreenDebug(WindowMvk window) : base(window)
        {
            label = new Label(window, 400, 40, "http://superant.by/mkv");
            label.Click += Label_Click;
            button = new Button(window, 360, 40, "Кнопка супер Tag");
            button2 = new Button(window, 640, 40, "Кнопка супер Tag2");
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
            cursorVBO = new Mesh(gl, new float[0], new int[] { 2, 2 });
            meshTextDebug = new Mesh2d(gl);
            AddControls(label);
            AddControls(button);
            AddControls(button2);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            // Положение устанваливаем тут, если есть привязка к размеру окна
            int w = Gi.Width / 2 / si;
            label.SetPosition(w - label.Width / 2, 200);
            button.SetPosition(w - button.Width / 2, 100);
            button2.SetPosition(w - button2.Width / 2, 150);
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
                cursorVBO.Reload(RenderFigure.Rectangle2d(0, 0, 24 * Gi.Si, 24 * Gi.Si, 0, 0, 1, 1));
            }
        }

        #region TextDebug

        private string textDebug = "";
        private bool isTextDebug = false;
        private Mesh2d meshTextDebug;

        /// <summary>
        /// Рендер текста отладки
        /// </summary>
        public void RenderTextDebug()
        {
            if (isTextDebug)
            {
                isTextDebug = false;
                window.Render.FontMain.BufferClear();
                window.Render.FontMain.RenderText(11 * Gi.Si, 11 * Gi.Si, textDebug, new Vector3(.2f, .2f, .2f));
                window.Render.FontMain.RenderText(10 * Gi.Si, 10 * Gi.Si, textDebug, new Vector3(.9f, .9f, .9f));

                meshTextDebug.Reload(window.Render.FontMain.ToBuffer());
                window.Render.FontMain.BufferClear();

                //int*[] p = &

                //IntPtr p = new IntPtr(*i);
            }
        }

        /// <summary>
        /// Прорисовка отладки
        /// </summary>
        public void DrawTextDebug()
        {
            window.Render.BindTexutreFontMain();
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
                render.FontSmall.BufferClear();
                render.FontMain.BufferClear();
                render.FontLarge.BufferClear();

                render.shaderText.Bind(gl);
                render.shaderText.SetUniformMatrix4(gl, "projview", window.Ortho2D);

                RenderTextDebug();
                DrawTextDebug();

                Vector3 bg = new Vector3(.2f, .2f, .2f);
                Vector3 cw = new Vector3(.9f, .9f, .9f);

                render.FontSmall.RenderString(xx + 1 * si, 201 * si, "-C-", bg);
                render.FontSmall.RenderString(xx, 200 * si, "-C-", cw);

                if (++xx > 900) xx = 0;

                render.FontSmall.RenderString(xx2 + 1 * si, 221 * si, "-S-", bg);
                render.FontSmall.RenderString(xx2, 220 * si, "-S-", cw);

                //if (++xx2 > 900) xx2 = 0;

                int width = window.Width;
                int height = window.Height;

                // Version
                int w = render.FontLarge.WidthString(window.Version) * si;
                render.FontLarge.RenderString(width - w - 9 * si, height - 18 * si, window.Version, bg);
                render.FontLarge.RenderString(width - w - 10 * si, height - 19 * si, window.Version, new Vector3(0.6f, 0.9f, .9f));

                string str;
                // fps
                //string str = "FPS " + window.Fps.ToString() + " TPS " + window.Tps.ToString();
                //FontMain.RenderString(11, height - 18, str, bg);
                //FontMain.RenderString(10, height - 19, str, cw);

               
                // XYZ
                w = 190 * si;
                str = window.Width + " " + window.Height;
                if (window.VSync) str += " VSync";
                render.FontMain.RenderString(w + 1 * si, height - 18 * si, str, bg);
                render.FontMain.RenderString(w, height - 19 * si, str, cw);

                // XY
                w = 400 * si;
                str = "XY";
                render.FontMain.RenderString(w + 1 * si, height - 18 * si, str, bg);
                render.FontMain.RenderString(w, height - 19 * si, str, cw);
                w = 430 * si;
                str = window.MouseX.ToString("0.0");
                render.FontMain.RenderString(w + 1 * si, height - 18 * si, str, bg);
                render.FontMain.RenderString(w, height - 19 * si, str, cw);
                w = 490 * si;
                str = window.MouseY.ToString("0.0");
                render.FontMain.RenderString(w + 1 * si, height - 18 * si, str, bg);
                render.FontMain.RenderString(w, height - 19 * si, str, cw);

                //textDb
                //if (textDb != "")
                //{
                //    FontMain.RenderString(11, height - 38, textDb, bg);
                //    FontMain.RenderString(10, height - 39, textDb, cw);
                //}

                // Draw
                render.BindTexture(AssetsTexture.FontSmall);
                render.FontSmall.ReloadDraw();
                render.BindTexutreFontMain();
                render.FontMain.ReloadDraw();
                render.BindTexture(AssetsTexture.FontLarge);
                render.FontLarge.ReloadDraw();

                if (windowMvk.cursorShow)
                {
                    render.BindTexture(AssetsTexture.Cursor);
                    render.shader2D.Bind(gl);
                    render.shader2D.SetUniformMatrix4(gl, "projview", window.Ortho2D);
                    render.shader2D.SetUniform1(gl, "biasX", window.MouseX);
                    render.shader2D.SetUniform1(gl, "biasY", window.MouseY);
                    render.shader2D.SetUniform4(gl, "color", 1, 1, 1, 1);
                    cursorVBO.Draw();
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            cursorVBO.Dispose();
            meshTextDebug.Dispose();
        }
    }
}
