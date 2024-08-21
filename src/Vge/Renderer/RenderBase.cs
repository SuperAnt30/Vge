using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Vge.Renderer.Font;
using Vge.Util;
using WinGL.OpenGL;
using WinGL.Util;

namespace Vge.Renderer
{
    /// <summary>
    /// Базовый класс отвечающий за прорисовку
    /// </summary>
    public class RenderBase
    {
        /// <summary>
        /// Объект окна
        /// </summary>
        protected readonly WindowMain window;
        /// <summary>
        /// Объект OpenGL для элемента управления
        /// </summary>
        protected readonly GL gl;
        /// <summary>
        /// Объект текстур
        /// </summary>
        protected readonly TextureMap textureMap;

        public FontBase font8;
        public FontBase font12;
        public FontBase font16;

        public RenderBase(WindowMain window, GL gl)
        {
            this.window = window;
            this.gl = gl;
            textureMap = new TextureMap(gl);
        }

        /// <summary>
        /// Стартовая инициализация до загрузчика
        /// </summary>
        public virtual void InitializeFirst()
        {
            TextureSetCount();

            //TODO::2024-08-21 временное создание текстур, надо заменить!!!
            SetTexture(Options.PathTextures + "cursor.png", (int)AssetsTexture.cursor);

            font8 = new FontBase(gl, SetTexture(Options.PathTextures + "Font8.png", (int)AssetsTexture.Font8), 1);
            font12 = new FontBase(gl, SetTexture(Options.PathTextures + "Font12.png", (int)AssetsTexture.Font12), 1);
            font16 = new FontBase(gl, SetTexture(Options.PathTextures + "Font16.png", (int)AssetsTexture.Font16), 2);
        }

        #region Texture

        /// <summary>
        /// Задать количество текстур
        /// </summary>
        protected virtual void TextureSetCount() => textureMap.SetCount(4);

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
    }
}
