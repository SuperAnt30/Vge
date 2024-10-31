using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using WinGL.Util;

namespace Vge.Util
{
    public static class BufferedFileImage
    {
        /// <summary>
        /// Конвертировать картинку в структуру BufferedImage
        /// </summary>
        public static BufferedImage FileToBufferedImage(string fileName, bool minmap = false)
        {
            Bitmap bitmap = Image.FromFile(fileName) as Bitmap;
            return new BufferedImage(bitmap.Width, bitmap.Height, BitmapToByteArray(bitmap))
            { FlagMipMap = minmap };
        }

        /// <summary>
        /// Конвертация из Bitmap в объект BufferedImage
        /// </summary>
        private static byte[] BitmapToByteArray(Bitmap bitmap)
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
