using System;
using System.Runtime.InteropServices;

namespace WinGL.Win32.User32
{
    /// <summary>
    /// Содержит сведения о классе окна.
    /// Он используется с функциями RegisterClassEx и GetClassInfoEx.
    /// Структура WNDCLASSEX аналогична структуре WNDCLASS
    /// https://learn.microsoft.com/ru-ru/windows/win32/api/winuser/ns-winuser-wndclassexa
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct WndClassEx
    {
        public uint cbSize;
        public ClassStyles style;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public WndProc lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
        public IntPtr hIconSm;

        public void Init()
        {
            cbSize = (uint)Marshal.SizeOf(this);
        }
    }
}
