using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;
using Vge.Renderer.Font;
using Vge.Renderer.Shaders;
using Vge.Util;
using WinGL.OpenGL;
using WinGL.Util;

namespace Vge.Renderer
{
    /// <summary>
    /// Основной класс рендера
    /// </summary>
    public class RenderMain : RenderBase
    {
        /// <summary>
        /// Основной шрифт
        /// </summary>
        public FontBase FontMain { get; private set; }

        /// <summary>
        /// Объект текстур
        /// </summary>
        public readonly TextureMap textureMap;
        /// <summary>
        /// Шейдоры для 2д
        /// </summary>
        public readonly Shader2d shader2D;
        /// <summary>
        /// Шейдоры для текста
        /// </summary>
        public readonly ShaderText shaderText;

        public RenderMain(WindowMain window) : base(window)
        {
            textureMap = new TextureMap(gl);
            shader2D = new Shader2d(gl);
            shaderText = new ShaderText(gl);
            meshTextDebug = new Mesh(gl, new float[0], new int[] { 2, 2, 3 });
        }

        /// <summary>
        /// Стартовая инициализация до загрузчика
        /// </summary>
        public virtual void InitializeFirst()
        {
            // Задать количество текстур
            TextureSetCount();

            FontMain = new FontBase(gl, SetTexture(Options.PathTextures + "FontMain.png", 0), 1);
        }

        #region Texture

        /// <summary>
        /// Задать количество текстур
        /// </summary>
        protected virtual void TextureSetCount() => textureMap.SetCount(1);

        /// <summary>
        /// Запустить текстуру основного шрифта
        /// </summary>
        public void BindTexutreFontMain() => textureMap.BindTexture(0);
        /// <summary>
        /// Запустить текстуру, указав индекс текстуры массива
        /// </summary>
        public void BindTexture(int index, uint texture = 0) => textureMap.BindTexture(index, texture);

        /// <summary>
        /// Задать текстуру
        /// </summary>
        protected BufferedImage SetTexture(string fileName, int index)
        {
            Bitmap bitmap = Image.FromFile(fileName) as Bitmap;
            BufferedImage image = new BufferedImage(bitmap.Width, bitmap.Height, BitmapToByteArray(bitmap));
            textureMap.SetTexture(index, image);
            return image;
        }

        /// <summary>
        /// Конвертация из Bitmap в объект BufferedImage
        /// </summary>
        private byte[] BitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bmpdata = null;
            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }
        }

        #endregion

        #region TextDebug

        private string textDebug = "";
        private bool isTextDebug = false;
        private Mesh meshTextDebug;

        /// <summary>
        /// Внести текст отладки
        /// </summary>
        public void SetTextDebug(string text)
        {
            textDebug = text;
            isTextDebug = true;
        }

        /// <summary>
        /// Рендер текста отладки
        /// </summary>
        public void RenderTextDebug()
        {
            if (isTextDebug)
            {
                isTextDebug = false;
                FontMain.RenderText(11 * Gi.Si, 11 * Gi.Si, textDebug, new Vector3(.2f, .2f, .2f));
                FontMain.RenderText(10 * Gi.Si, 10 * Gi.Si, textDebug, new Vector3(.9f, .9f, .9f));

                meshTextDebug.Reload(FontMain.ToBuffer());
                FontMain.BufferClear();
            }
        }

        /// <summary>
        /// Прорисовка отладки
        /// </summary>
        public void DrawTextDebug()
        {
            BindTexutreFontMain();
            meshTextDebug.Draw();
        }

        #endregion

        #region Draw + Debug

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex) 
        {
            DrawBegin();
            if (window.Game == null)
            {
                // Нет игры
                if (window.Screen == null)
                {
                    //TODO::2024-09-05 Ой, такого быть посути не должно!
                }
                else
                {
                    window.Screen.Draw(timeIndex);
                }
            }
            else
            {
                // Есть игра
                window.Game.Draw(timeIndex);
                if (window.Screen != null)
                {
                    window.Screen.Draw(timeIndex);
                }
            }
            DrawEnd();
        }

        private float speedFrameAll;
        private float speedTickAll;
        private long timerSecond;
        private int fps;
        private int tps;
        private long tickDraw;

        public virtual void DrawBegin()
        {
            fps++;
            tickDraw = window.TimeTicks();
        }

        /// <summary>
        /// После прорисовки каждого кадра OpenGL
        /// </summary>
        public virtual void DrawEnd()
        {
            // Перерасчёт кадров раз в секунду, и среднее время прорисовки кадра
            if (window.Time() >= timerSecond + 1000)
            {
                float speedTick = 0;
                if (tps > 0) speedTick = speedTickAll / tps;
                window.debug.SetTpsFps(fps, speedFrameAll / fps, tps, speedTick);

                timerSecond += 1000;
                speedFrameAll = 0;
                speedTickAll = 0;
                fps = 0;
                tps = 0;
            }

            speedFrameAll += (float)(window.TimeTicks() - tickDraw) / Ticker.TimerFrequency;
        }

        /// <summary>
        /// Время выполнения такта в мс
        /// </summary>
        public void SetExecutionTime(float time)
        {
            speedTickAll += time;
            tps++;
        }

        #endregion

        public virtual void DrawDebug() { }
    }
}
