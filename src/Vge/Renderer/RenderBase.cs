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
            textureMap = new TextureMap(gl, 4);
        }

        /// <summary>
        /// Стартовая инициализация до загрузчика
        /// </summary>
        public virtual void InitializeFirst()
        {
            //TODO::2024-08-21 временное создание текстур, надо заменить!!!
            Bitmap bitmap = Image.FromFile(Options.PathTextures + "cursor.png") as Bitmap;

            textureMap.SetTexture((int)AssetsTexture.cursor,
                new BufferedImage(bitmap.Width, bitmap.Height, BitmapToByteArray(bitmap)));

            bitmap = Image.FromFile(Options.PathTextures + "Font8.png") as Bitmap;
            BufferedImage font8 = new BufferedImage(bitmap.Width, bitmap.Height, BitmapToByteArray(bitmap));
            bitmap = Image.FromFile(Options.PathTextures + "Font12.png") as Bitmap;
            BufferedImage font12 = new BufferedImage(bitmap.Width, bitmap.Height, BitmapToByteArray(bitmap));
            bitmap = Image.FromFile(Options.PathTextures + "Font16.png") as Bitmap;
            BufferedImage font16 = new BufferedImage(bitmap.Width, bitmap.Height, BitmapToByteArray(bitmap));

            textureMap.SetTexture((int)AssetsTexture.Font8, font8);
            textureMap.SetTexture((int)AssetsTexture.Font12, font12);
            textureMap.SetTexture((int)AssetsTexture.Font16, font16);

            this.font8 = new FontBase(gl, font8, 1);
            this.font12 = new FontBase(gl, font12, 1);
            this.font16 = new FontBase(gl, font16, 2);
        }

        /// <summary>
        /// Запустить текстуру, указав индекс текстуры массива
        /// </summary>
        public void BindTexture(int index, uint texture = 0) => textureMap.BindTexture(index, texture);

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
    }
}
