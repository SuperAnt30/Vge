//using Mvk2.Renderer;
//using System.Numerics;
//using Vge.Gui.Screens;
//using Vge.Renderer;
//using Vge.Renderer.Font;
//using WinGL.OpenGL;

//namespace Mvk2.Gui.Screens
//{
//    /// <summary>
//    /// Заставка
//    /// </summary>
//    public class ScreenGameDebug : ScreenBase
//    {
//        /// <summary>
//        /// Объект сетки курсора, временно
//        /// </summary>
//        private Mesh cursorVBO;

//        public ScreenGameDebug(WindowMvk window) : base(window) { }

//        /// <summary>
//        /// Запускается при создании объекта и при смене режима FullScreen
//        /// </summary>
//        protected override void OnInitialize()
//        {
//            base.OnInitialize();
//            cursorVBO = new Mesh(gl, new float[0], new int[] { 2, 2 });
//            meshTextDebug = new MeshGuiColor(gl);
//        }

//        /// <summary>
//        /// Игровой такт
//        /// </summary>
//        public override void OnTick(float deltaTime)
//        {
//            // Отладка на экране
//            textDebug = window.debug.ToText();
//            isTextDebug = true;

//            if (((WindowMvk)window).cursorShow)
//            {
//                cursorVBO.Reload(RenderFigure.Rectangle2d(0, 0, 24 * Gi.Si, 24 * Gi.Si, 0, 0, 1, 1));
//            }
//        }

//        #region TextDebug

//        private string textDebug = "";
//        private bool isTextDebug = false;
//        private MeshGuiColor meshTextDebug;

//        /// <summary>
//        /// Рендер текста отладки
//        /// </summary>
//        public void RenderTextDebug()
//        {
//            if (isTextDebug)
//            {
//                isTextDebug = false;
//                window.Render.FontMain.Clear();
//                window.Render.FontMain.SetFontFX(EnumFontFX.Shadow).SetColor(new Vector3(.9f, .9f, .9f));
//                window.Render.FontMain.RenderText(10 * Gi.Si, 10 * Gi.Si, textDebug);
//                window.Render.FontMain.RenderFX();
//                meshTextDebug.Reload(window.Render.FontMain.ToBuffer());
//                window.Render.FontMain.Clear();

//                //int*[] p = &

//                //IntPtr p = new IntPtr(*i);
//            }
//        }

//        /// <summary>
//        /// Прорисовка отладки
//        /// </summary>
//        public void DrawTextDebug()
//        {
//            window.Render.BindTexutreFontMain();
//            meshTextDebug.Draw();
//        }

//        #endregion

//        public int xx = 0;
//        public int xx2 = 0;

//        public override void Draw(float timeIndex)
//        {
//            base.Draw(timeIndex);

//            if (window.Game == null) gl.ClearColor(.7f, .4f, .4f, 1f);
//            //gl.BlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);
//            //gl.Enable(GL.GL_BLEND);

//            if (window is WindowMvk windowMvk)
//            {
//                int si = Gi.Si;

//                RenderMvk render = windowMvk.Render as RenderMvk;
//                render.FontSmall.Clear();
//                render.FontMain.Clear();
//                render.FontLarge.Clear();

//                render.ShaderBindGuiColor();

//                RenderTextDebug();
//                DrawTextDebug();

//                Vector3 cw = new Vector3(.9f, .9f, .9f);

//                render.FontSmall.SetColor(cw).SetFontFX(EnumFontFX.Shadow);
//                //render.FontSmall.RenderString(xx + 1 * si, 201 * si, "-C-", bg);
//                render.FontSmall.RenderString(xx, 200 * si, "-C-");

//                if (++xx > 900) xx = 0;

//                //render.FontSmall.RenderString(xx2 + 1 * si, 221 * si, "-S-", bg);
//                render.FontSmall.RenderString(xx2, 220 * si, "-S-");

//                //if (++xx2 > 900) xx2 = 0;

//                int width = window.Width;
//                int height = window.Height;

//                string str;

//                // Version
//                render.FontLarge.SetColor(new Vector3(0.6f, 0.9f, .9f)).SetFontFX(EnumFontFX.Shadow);
//                str = "GAME! " + window.Version;
//                int w = render.FontLarge.WidthString(str) * si;
//                render.FontLarge.RenderString(width - w - 10 * si, height - 19 * si, str);

//                // fps
//                //string str = "FPS " + window.Fps.ToString() + " TPS " + window.Tps.ToString();
//                //FontMain.RenderString(11, height - 18, str, bg);
//                //FontMain.RenderString(10, height - 19, str, cw);


//                // XYZ
//                w = 190 * si;
//                str = window.Width + " " + window.Height;
//                if (window.VSync) str += " VSync";
//                render.FontMain.SetColor(cw).SetFontFX(EnumFontFX.Shadow);
//                //render.FontMain.RenderString(w + 1 * si, height - 18 * si, str, bg);
//                render.FontMain.RenderString(w, height - 19 * si, str);

//                // XY
//                w = 400 * si;
//                str = "XY";
//                //render.FontMain.RenderString(w + 1 * si, height - 18 * si, str, bg);
//                render.FontMain.RenderString(w, height - 19 * si, str);
//                w = 430 * si;
//                str = window.MouseX.ToString("0.0");
//              //  render.FontMain.RenderString(w + 1 * si, height - 18 * si, str, bg);
//                render.FontMain.RenderString(w, height - 19 * si, str);
//                w = 490 * si;
//                str = window.MouseY.ToString("0.0");
//               // render.FontMain.RenderString(w + 1 * si, height - 18 * si, str, bg);
//                render.FontMain.RenderString(w, height - 19 * si, str);

//                //textDb
//                //if (textDb != "")
//                //{
//                //    FontMain.RenderString(11, height - 38, textDb, bg);
//                //    FontMain.RenderString(10, height - 39, textDb, cw);
//                //}

//                // Draw
//                render.BindTexture(AssetsTexture.FontSmall);
//                render.FontSmall.RenderFX();
//                render.FontSmall.ReloadDraw();
//                render.BindTexutreFontMain();
//                render.FontMain.RenderFX();
//                render.FontMain.ReloadDraw();
//                render.BindTexture(AssetsTexture.FontLarge);
//                render.FontLarge.RenderFX();
//                render.FontLarge.ReloadDraw();

//                if (windowMvk.cursorShow)
//                {
//                    render.BindTexture(AssetsTexture.Cursor);
//                    render.shader2D.Bind(gl);
//                    render.shader2D.SetUniformMatrix4(gl, "projview", window.Ortho2D);
//                    render.shader2D.SetUniform1(gl, "biasX", window.MouseX);
//                    render.shader2D.SetUniform1(gl, "biasY", window.MouseY);
//                    render.shader2D.SetUniform4(gl, "color", 1, 1, 1, 1);
//                    cursorVBO.Draw();
//                }
//            }
//        }

//        public override void Dispose()
//        {
//            cursorVBO.Dispose();
//            meshTextDebug.Dispose();
//        }
//    }
//}
