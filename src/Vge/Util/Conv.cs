using System.Runtime.CompilerServices;
using WinGL.Util;

namespace Vge.Util
{
    /// <summary>
    /// Конвертация
    /// </summary>
    public sealed class Conv
    {
        /// <summary>
        /// Конвертировать уникальный индекс из координат чанка x, y
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ChunkXyToIndex(int x, int y) => ((ulong)(uint)x << 32) | ((uint)y);
        /// <summary>
        /// Конвертировать из индекса в координату чанка X
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexToChunkX(ulong index) => (int)((index & 0xFFFFFFFF00000000) >> 32);
        /// <summary>
        /// Конвертировать из индекса в координату чанка Y
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexToChunkY(ulong index) => (int)index;
        /// <summary>
        /// Конвертировать из индекса в координату чанка X
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2i IndexToChunkVector2i(ulong index) 
            => new Vector2i((int)((index & 0xFFFFFFFF00000000) >> 32), (int)index);
    }
}
