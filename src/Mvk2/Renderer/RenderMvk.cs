using System.Numerics;
using Vge;
using Vge.Renderer;
using WinGL.OpenGL;

namespace Mvk2.Renderer
{
    /// <summary>
    /// Класс отвечающий за прорисовку для малювек
    /// </summary>
    public class RenderMvk : RenderBase
    {
        public RenderMvk(WindowMain window, GL gl) : base(window, gl)
        {
            //textureMap = new TextureMap(gl, 4);
        }

        /// <summary>
        /// Стартовая инициализация до загрузчика
        /// </summary>
        public override void InitializeFirst()
        {
            base.InitializeFirst();
        }

        /// <summary>
        /// Отладочный теккст
        /// VBO 450-460 fps, если biasY вывести на строку один то 540
        /// DL 650 fps
        /// </summary>
        public string textDebug =
            "Занесло меня на остров,\r\n" +
            "Ожидало много бед.\r\n" +
            "Жить на нём совсем не просто,\r\n" +
            "А прошло не мало лет.\r\n\r\n" +
            "Почти вымерли все звери,\r\n" +
            "Я остался лишь живой.\r\n" +
            "И ходил я всё и думал,\r\n" +
            "Как попасть же мне домой.\r\n\r\n" +
            "Занесло меня на остров,\r\n" +
            "Ожидало много бед.\r\n" +
            "Жить на нём совсем не просто,\r\n" +
            "А прошло не мало лет.\r\n\r\n" +
            "Почти вымерли все звери,\r\n" +
            "Я остался лишь живой.\r\n" +
            "И ходил я всё и думал,\r\n" +
            "Как попасть же мне домой.\r\n\r\n";

        public int xx = 0;

        Mesh mesh1;

        public void DrawDebug()
        {
            font8.BufferClear();
            font12.BufferClear();
            font16.BufferClear();

            WindowMain.shaderText.Bind(gl);
            WindowMain.shaderText.SetUniformMatrix4(gl, "projview", window.Ortho2D);

            Vector3 bg = new Vector3(.2f, .2f, .2f);
            Vector3 cw = new Vector3(.9f, .9f, .9f);

            font8.RenderString(xx + 1, 401, "-O-", bg);
            font8.RenderString(xx, 400, "-O-", cw);

            if (++xx > 900) xx = 0;
            
            // textDebug
            if (mesh1 == null)
            {

                font12.RenderText(11, 11, textDebug, bg);

                font12.RenderText(10, 10, textDebug, cw);
                font12.RenderText(211, 11, textDebug, bg);
                font12.RenderText(210, 10, textDebug, cw);
                font12.RenderText(411, 11, textDebug, bg);
                font12.RenderText(410, 10, textDebug, cw);
                font12.RenderText(611, 11, textDebug, bg);
                font12.RenderText(610, 10, textDebug, cw);

                mesh1 = new Mesh(gl, font12.ToBuffer(), new int[] { 2, 2, 3 });
                font12.BufferClear();

            }

            BindTexture((int)AssetsTexture.Font12);
            mesh1.Draw();

            int width = window.Width;
            int height = window.Height;

            // Version
            int w = font16.WidthString(window.Version);
            font16.RenderString(width - w - 9, height - 18, window.Version, bg);
            font16.RenderString(width - w - 10, height - 19, window.Version, new Vector3(0.6f, 0.9f, .9f));

            // fps
            string str = "FPS " + window.Fps.ToString() + " TPS " + window.Tps.ToString();
            font12.RenderString(11, height - 18, str, bg);
            font12.RenderString(10, height - 19, str, cw);

            // XYZ
            w = 190;
            str = width + " " + height;
            if (window.VSync) str += " VSync";
            font12.RenderString(w + 1, height - 18, str, bg);
            font12.RenderString(w, height - 19, str, cw);

            // XY
            w = 400;
            str = "XY";
            font12.RenderString(w + 1, height - 18, str, bg);
            w += font12.RenderString(w, height - 19, str, cw) + 10;
            str = window.MouseX.ToString("0.0");
            font12.RenderString(w + 1, height - 18, str, bg);
            w += font12.RenderString(w, height - 19, str, cw) + 10;
            str = window.MouseY.ToString("0.0");
            font12.RenderString(w + 1, height - 18, str, bg);
            font12.RenderString(w, height - 19, str, cw);

            // Draw
            BindTexture((int)AssetsTexture.Font8);
            font8.ReloadDraw();
            BindTexture((int)AssetsTexture.Font12);
            font12.ReloadDraw();
            BindTexture((int)AssetsTexture.Font16);
            font16.ReloadDraw();
        }
    }
}
