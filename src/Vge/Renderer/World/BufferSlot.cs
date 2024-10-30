using System;
using System.Runtime.InteropServices;
using Vge.Util;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Слот буфера
    /// </summary>
    public class BufferSlot : IDisposable
    {
        /// <summary>
        /// Ссылка на буфер
        /// </summary>
        public IntPtr Buffer { get; private set; } = IntPtr.Zero;

        /// <summary>
        /// Размер буфера
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Пустой ли буфер
        /// </summary>
        public bool Empty { get; private set; } = true;

        /// <summary>
        /// Задать буфер
        /// </summary>
        public void Set(IBufferFast bufferFast)
        {
            Size = bufferFast.ToSize();
            Buffer = Marshal.AllocHGlobal(Size);
            bufferFast.CopyBuffer(Buffer);
            Empty = false;
        }

        /// <summary>
        /// Очистить буфер
        /// </summary>
        public void Clear()
        {
            Marshal.FreeHGlobal(Buffer);
            Buffer = IntPtr.Zero;
            Empty = true;
        }

        public void Dispose() => Clear();
    }
}
