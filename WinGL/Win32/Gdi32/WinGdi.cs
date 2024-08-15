using System;
using System.Runtime.InteropServices;

namespace WinGL.Win32.Gdi32
{
    internal static class WinGdi
    {
        public const string Gdi32 = "gdi32.dll";

        /// <summary>
        /// Пытается сопоставить соответствующий формат пикселей,
        /// поддерживаемый контекстом устройства, с заданной спецификацией
        /// формата пикселей.
        /// </summary>
        /// <param name="hDC"></param>
        /// <param name="ppfd"></param>
        /// <returns></returns>
        [DllImport(Gdi32, SetLastError = true)]
        public static extern int ChoosePixelFormat(IntPtr hDC,
            [In, MarshalAs(UnmanagedType.LPStruct)] PixelFormatDescriptor ppfd);

        /// <summary>
        /// Задает формат пикселей указанного контекста устройства в формат,
        /// заданный индексом iPixelFormat .
        /// </summary>
        [DllImport(Gdi32, SetLastError = true)]
        public static extern int SetPixelFormat(IntPtr hDC, int iPixelFormat,
            [In, MarshalAs(UnmanagedType.LPStruct)] PixelFormatDescriptor ppfd);

        /// <summary>
        /// Обменивается передним и задним буферами, если текущий формат пикселей для окна,
        /// на которое ссылается указанный контекст устройства, включает задний буфер.
        /// </summary>
        [DllImport(Gdi32, SetLastError = true)]
        public static extern int SwapBuffers(IntPtr hDC);

        public const byte PFD_TYPE_RGBA = 0;
        public const byte PFD_TYPE_COLORINDEX = 1;

        public const uint PFD_DOUBLEBUFFER = 1;
        public const uint PFD_STEREO = 2;
        public const uint PFD_DRAW_TO_WINDOW = 4;
        public const uint PFD_DRAW_TO_BITMAP = 8;
        public const uint PFD_SUPPORT_GDI = 16;
        public const uint PFD_SUPPORT_OPENGL = 32;
        public const uint PFD_GENERIC_FORMAT = 64;
        public const uint PFD_NEED_PALETTE = 128;
        public const uint PFD_NEED_SYSTEM_PALETTE = 256;
        public const uint PFD_SWAP_EXCHANGE = 512;
        public const uint PFD_SWAP_COPY = 1024;
        public const uint PFD_SWAP_LAYER_BUFFERS = 2048;
        public const uint PFD_GENERIC_ACCELERATED = 4096;
        public const uint PFD_SUPPORT_DIRECTDRAW = 8192;

        public const sbyte PFD_MAIN_PLANE = 0;
        public const sbyte PFD_OVERLAY_PLANE = 1;
        public const sbyte PFD_UNDERLAY_PLANE = -1;
    }
}
