using Mvk2.Renderer;
using System.Numerics;
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

        public ScreenDebug(WindowMvk window) : base(window)
        {
            cursorVBO = new Mesh(gl, RenderFigure.Rectangle2d(0, 0, 24, 24, 0, 0, 1, 1), new int[] { 2, 2 });
        }

        public override void Draw(float timeIndex)
        {
            if (window.Game == null) gl.ClearColor(.7f, .4f, .4f, 1f);
            //gl.BlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);
            //gl.Enable(GL.GL_BLEND);

            if (window is WindowMvk windowMvk)
            {
                RenderMvk render = windowMvk.Render as RenderMvk;
                render.FontSmall.BufferClear();
                render.FontMain.BufferClear();
                render.FontLarge.BufferClear();

                render.shaderText.Bind(gl);
                render.shaderText.SetUniformMatrix4(gl, "projview", window.Ortho2D);

                render.RenderTextDebug();
                render.DrawTextDebug();

                Vector3 bg = new Vector3(.2f, .2f, .2f);
                Vector3 cw = new Vector3(.9f, .9f, .9f);

                render.FontSmall.RenderString(render.xx + 1, 201, "-C-", bg);
                render.FontSmall.RenderString(render.xx, 200, "-C-", cw);

                if (++render.xx > 900) render.xx = 0;

                render.FontSmall.RenderString(render.xx2 + 1, 221, "-S-", bg);
                render.FontSmall.RenderString(render.xx2, 220, "-S-", cw);

                //if (++xx2 > 900) xx2 = 0;

                int width = window.Width;
                int height = window.Height;

                // Version
                int w = render.FontLarge.WidthString(window.Version);
                render.FontLarge.RenderString(width - w - 9, height - 18, window.Version, bg);
                render.FontLarge.RenderString(width - w - 10, height - 19, window.Version, new Vector3(0.6f, 0.9f, .9f));

                string str;
                // fps
                //string str = "FPS " + window.Fps.ToString() + " TPS " + window.Tps.ToString();
                //FontMain.RenderString(11, height - 18, str, bg);
                //FontMain.RenderString(10, height - 19, str, cw);

                // XYZ
                w = 190;
                str = width + " " + height;
                if (window.VSync) str += " VSync";
                render.FontMain.RenderString(w + 1, height - 18, str, bg);
                render.FontMain.RenderString(w, height - 19, str, cw);

                // XY
                w = 400;
                str = "XY";
                render.FontMain.RenderString(w + 1, height - 18, str, bg);
                w += render.FontMain.RenderString(w, height - 19, str, cw) + 10;
                str = window.MouseX.ToString("0.0");
                render.FontMain.RenderString(w + 1, height - 18, str, bg);
                w += render.FontMain.RenderString(w, height - 19, str, cw) + 10;
                str = window.MouseY.ToString("0.0");
                render.FontMain.RenderString(w + 1, height - 18, str, bg);
                render.FontMain.RenderString(w, height - 19, str, cw);

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
    }
}
