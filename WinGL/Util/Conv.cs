using System;

namespace WinGL.Util
{
    /// <summary>
    /// Класс всячиских конверторов
    /// </summary>
    internal static class Conv
    {
        /// <summary>
        /// Сконвертировать из параметра IntPtr в uint
        /// </summary>
        //public static long IntPtrToLong(IntPtr param)
        //    => unchecked(IntPtr.Size == 8 ? (uint)param.ToInt64() : (uint)param.ToInt32());

        /// <summary>
        /// Сконвертировать из параметра IntPtr в uint
        /// </summary>
        //public static uint IntPtrToUint(IntPtr param)
        //    => unchecked(IntPtr.Size == 8 ? (uint)param.ToInt64() : (uint)param.ToInt32());

        /// <summary>
        /// Сконвертировать из параметра IntPtr в два uint через Vec2i
        /// </summary>
        public static Vec2i UintToVec2i(IntPtr param)
        {
            uint field = unchecked(IntPtr.Size == 8 ? (uint)param.ToInt64() : (uint)param.ToInt32());
            return new Vec2i(unchecked((short)field), unchecked((short)(field >> 16)));
        }
    }
}
