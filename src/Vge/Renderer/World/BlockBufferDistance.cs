using System;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Блок буфера, с дистанцией до камеры, нужен для сортировки альфа блоков
    /// </summary>
    public struct BlockBufferDistance : IComparable
    {
        /// <summary>
        /// Буфер для склейки рендера, Float данных
        /// </summary>
        public float[] BufferFloat;
        /// <summary>
        /// Буфер для склейки рендера, Byte данных
        /// </summary>
        public byte[] BufferByte;
        /// <summary>
        /// Дистанция
        /// </summary>
        public float Distance;

        public int CompareTo(object obj)
        {
            if (obj is BlockBufferDistance v) return Distance.CompareTo(v.Distance);
            else throw new Exception(Sr.ItIsImpossibleToCompareTwoObjects);
        }
    }
}
