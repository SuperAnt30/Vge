using System;
using System.Runtime.InteropServices;
using WinGL.Util;

namespace WinGL.Win32.User32
{
    [StructLayout(LayoutKind.Sequential)]
    struct CursorInfoStruct
    {
        /// <summary> The structure size in bytes that must be set via calling Marshal.SizeOf(typeof(CursorInfoStruct)).</summary>
        public int cbSize;
        /// <summary> The cursor state: 0 == hidden, 1 == showing, 2 == suppressed (is supposed to be when finger touch is used, but in practice finger touch results in 0, not 2)</summary>
        public int flags;
        /// <summary> A handle to the cursor. </summary>
        public IntPtr hCursor;
        /// <summary> The cursor screen coordinates.</summary>
        public Vector2i pt;
    }
}
