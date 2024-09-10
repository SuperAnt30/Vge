using System;
using System.Drawing;
using System.Drawing.Imaging;
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
    public class RenderMain : Warp
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

        /// <summary>
        /// Время выполнения кадра
        /// </summary>
        private float speedFrameAll;
        /// <summary>
        /// Время выполнения такта
        /// </summary>
        private float speedTickAll;
        /// <summary>
        /// Счётчик времени кратно секундам в мс
        /// </summary>
        private long timeSecond;
        /// <summary>
        /// Количество фпс
        /// </summary>
        private int fps;
        /// <summary>
        /// Количество тпс
        /// </summary>
        private int tps;
        /// <summary>
        /// Время перед начало прорисовки кадра
        /// </summary>
        private long timeBegin;

        public RenderMain(WindowMain window) : base(window)
        {
            textureMap = new TextureMap(gl);
            shader2D = new Shader2d(gl);
            shaderText = new ShaderText(gl);
        }

        /// <summary>
        /// Стартовая инициализация до загрузчика
        /// </summary>
        public virtual void InitializeFirst()
        {
            // Задать количество текстур
            TextureSetCount();

            string[] vs = GetFileNameTextures();
            FontMain = new FontBase(gl, SetTexture(vs[0], 0), 1);
            SetTexture(vs[1], 1);
        }

        /// <summary>
        /// Получить массив имён файл текстур,
        /// 0 - FontMain основной шрифт
        /// 1 - Widgets
        /// </summary>
        protected virtual string[] GetFileNameTextures() => new string[] {
            Options.PathTextures + "FontMain.png",
            Options.PathTextures + "Widgets.png"
        };

        #region Texture

        /// <summary>
        /// Задать количество текстур
        /// </summary>
        protected virtual void TextureSetCount() => textureMap.SetCount(2);

        /// <summary>
        /// Запустить текстуру основного шрифта
        /// </summary>
        public void BindTexutreFontMain() => textureMap.BindTexture(0);
        /// <summary>
        /// Запустить текстуру основного виджета
        /// </summary>
        public void BindTexutreWidgets() => textureMap.BindTexture(1);

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

        #region Draw

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
                    // Отсутствует прорисовка
                    throw new Exception(SR.ThereIsNoDrawing);
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

        public virtual void DrawBegin()
        {
            fps++;
            timeBegin = window.TimeTicks();
        }

        /// <summary>
        /// После прорисовки каждого кадра OpenGL
        /// </summary>
        public virtual void DrawEnd()
        {
            // Перерасчёт кадров раз в секунду, и среднее время прорисовки кадра
            if (window.Time() >= timeSecond)
            {
                float speedTick = 0;
                if (tps > 0) speedTick = speedTickAll / tps;
                window.debug.SetTpsFps(fps, speedFrameAll / fps, tps, speedTick);

                timeSecond += 1000;
                speedFrameAll = 0;
                speedTickAll = 0;
                fps = 0;
                tps = 0;
            }

            speedFrameAll += (float)(window.TimeTicks() - timeBegin) / Ticker.TimerFrequency;
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
    }
}
