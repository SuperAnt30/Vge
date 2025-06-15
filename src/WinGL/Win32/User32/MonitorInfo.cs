using System.Runtime.InteropServices;

namespace WinGL.Win32.User32
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    public class MonitorInfoEx
    {
        public int Size = Marshal.SizeOf(typeof(MonitorInfoEx));
        public RectStruct Monitor;
        public RectStruct WorkArea;
        public int Flags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RectStruct
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
