using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Vge.Util
{
    public static class BufferedFileImage
    {
        /// <summary>
        /// Конвертировать картинку в структуру BufferedImage
        /// </summary>
        public static BufferedImage FileToBufferedImage(string fileName)
        {
            Bitmap bitmap = Image.FromFile(fileName) as Bitmap;
            return new BufferedImage(bitmap.Width, bitmap.Height, BitmapToByteArray(bitmap));
        }

        /// <summary>
        /// Конвертировать картинку в структуру BufferedImage
        /// </summary>
        public static BufferedImage FileToBufferedImage(byte[] bufferPng)
        {
            Bitmap bitmap = Image.FromStream(new MemoryStream(bufferPng)) as Bitmap;
            //bitmap.Save("c.png", ImageFormat.Png);
            return new BufferedImage(bitmap.Width, bitmap.Height, BitmapToByteArray(bitmap));
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

        /// <summary>
        /// Оставить половину сверху
        /// </summary>
        public static BufferedImage LeaveHalfOnTop(BufferedImage buffered)
        {
            int count = buffered.Buffer.Length / 2;
            byte[] buffer = new byte[count];
            Buffer.BlockCopy(buffered.Buffer, 0, buffer, 0, count);
            return new BufferedImage(buffered.Width, buffered.Height / 2, buffer);
        }

        /// <summary>
        /// Оставить половину снизу
        /// </summary>
        public static BufferedImage LeaveHalfOnBottom(BufferedImage buffered)
        {
            int count = buffered.Buffer.Length / 2;
            byte[] buffer = new byte[count];
            Buffer.BlockCopy(buffered.Buffer, count, buffer, 0, count);
            return new BufferedImage(buffered.Width, buffered.Height / 2, buffer);
        }
    }
}
