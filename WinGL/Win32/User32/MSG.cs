using System;
using System.Runtime.InteropServices;
using WinGL.Util;

namespace WinGL.Win32.User32
{
    /// <summary>
    /// Содержит информацию сообщения из очереди сообщений потока
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public short time;
        public Vector2l pt;
        public short lPrivate;
    }
}
