using Mvk2.Util;
using System.Numerics;
using Vge.Renderer;
using Vge.Renderer.Font;
using WinGL.OpenGL;
using WinGL.Util;

namespace Mvk2.Renderer
{
    /// <summary>
    /// Класс отвечающий за прорисовку для малювек
    /// </summary>
    public class RenderMvk : RenderBase
    {
        /// <summary>
        /// Мелкий шрифт
        /// </summary>
        public FontBase FontSmall { get; private set; }
        /// <summary>
        /// Крупный шрифт
        /// </summary>
        public FontBase FontLarge { get; private set; }

        /// <summary>
        /// Объект окна малювек
        /// </summary>
        protected readonly WindowMvk windowMvk;
        /// <summary>
        /// Объект сетки курсора, временно
        /// </summary>
        private Mesh cursorVBO;

        public RenderMvk(WindowMvk window, GL gl) : base(window, gl)
        {
            windowMvk = window;
            cursorVBO = new Mesh(gl, RenderFigure.Rectangle2d(0, 0, 24, 24, 0, 0, 1, 1), new int[] { 2, 2 });
        }

        /// <summary>
        /// Стартовая инициализация до загрузчика
        /// </summary>
        public override void InitializeFirst()
        {
            base.InitializeFirst();

            SetTexture(OptionsMvk.PathTextures + "Cursor.png", AssetsTexture.Cursor);

            FontSmall = new FontBase(gl, 
                SetTexture(OptionsMvk.PathTextures + "FontSmall.png", AssetsTexture.FontSmall), 1);
            FontLarge = new FontBase(gl, 
                SetTexture(OptionsMvk.PathTextures + "FontLarge.png", AssetsTexture.FontLarge), 2);
        }

        #region Texture

        /// <summary>
        /// Задать количество текстур
        /// </summary>
        protected override void TextureSetCount() => textureMap.SetCount(4);

        /// <summary>
        /// Запустить текстуру, указав индекс текстуры массива
        /// </summary>
        public void BindTexture(AssetsTexture index, uint texture = 0) 
            => textureMap.BindTexture((int)index, texture);

        /// <summary>
        /// Задать текстуру
        /// </summary>
        protected BufferedImage SetTexture(string fileName, AssetsTexture index)
            => SetTexture(fileName, (int)index);

        #endregion

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
            FontSmall.BufferClear();
            FontMain.BufferClear();
            FontLarge.BufferClear();

            shaderText.Bind(gl);
            shaderText.SetUniformMatrix4(gl, "projview", window.Ortho2D);

            Vector3 bg = new Vector3(.2f, .2f, .2f);
            Vector3 cw = new Vector3(.9f, .9f, .9f);

            FontSmall.RenderString(xx + 1, 401, "-O-", bg);
            FontSmall.RenderString(xx, 400, "-O-", cw);

            if (++xx > 900) xx = 0;
            
            // textDebug
            if (mesh1 == null)
            {

                FontMain.RenderText(11, 11, textDebug, bg);

                FontMain.RenderText(10, 10, textDebug, cw);
                FontMain.RenderText(211, 11, textDebug, bg);
                FontMain.RenderText(210, 10, textDebug, cw);
                FontMain.RenderText(411, 11, textDebug, bg);
                FontMain.RenderText(410, 10, textDebug, cw);
                FontMain.RenderText(611, 11, textDebug, bg);
                FontMain.RenderText(610, 10, textDebug, cw);

                mesh1 = new Mesh(gl, FontMain.ToBuffer(), new int[] { 2, 2, 3 });
                FontMain.BufferClear();

            }

            BindTexutreFontMain();
            mesh1.Draw();

            int width = window.Width;
            int height = window.Height;

            // Version
            int w = FontLarge.WidthString(window.Version);
            FontLarge.RenderString(width - w - 9, height - 18, window.Version, bg);
            FontLarge.RenderString(width - w - 10, height - 19, window.Version, new Vector3(0.6f, 0.9f, .9f));

            // fps
            string str = "FPS " + window.Fps.ToString() + " TPS " + window.Tps.ToString();
            FontMain.RenderString(11, height - 18, str, bg);
            FontMain.RenderString(10, height - 19, str, cw);

            // XYZ
            w = 190;
            str = width + " " + height;
            if (window.VSync) str += " VSync";
            FontMain.RenderString(w + 1, height - 18, str, bg);
            FontMain.RenderString(w, height - 19, str, cw);

            // XY
            w = 400;
            str = "XY";
            FontMain.RenderString(w + 1, height - 18, str, bg);
            w += FontMain.RenderString(w, height - 19, str, cw) + 10;
            str = window.MouseX.ToString("0.0");
            FontMain.RenderString(w + 1, height - 18, str, bg);
            w += FontMain.RenderString(w, height - 19, str, cw) + 10;
            str = window.MouseY.ToString("0.0");
            FontMain.RenderString(w + 1, height - 18, str, bg);
            FontMain.RenderString(w, height - 19, str, cw);

            // Draw
            BindTexture(AssetsTexture.FontSmall);
            FontSmall.ReloadDraw();
            BindTexutreFontMain();
            FontMain.ReloadDraw();
            BindTexture(AssetsTexture.FontLarge);
            FontLarge.ReloadDraw();

            if (windowMvk.cursorShow)
            {
                BindTexture(AssetsTexture.Cursor);
                shader2D.Bind(gl);
                shader2D.SetUniformMatrix4(gl, "projview", window.Ortho2D);
                shader2D.SetUniform1(gl, "biasX", window.MouseX);
                shader2D.SetUniform1(gl, "biasY", window.MouseY);
                shader2D.SetUniform4(gl, "color", 1, 1, 1, 1);
                cursorVBO.Draw();
            }
        }
    }
}
