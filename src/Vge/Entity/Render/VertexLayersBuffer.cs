using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Vge.Entity.Render
{
    /// <summary>
    /// Буфер вершин слоёв (одежды) сущности
    /// </summary>
    public class VertexLayersBuffer
    {
        /// <summary>
        /// Количество вершин
        /// </summary>
        public int CountVertices { get; private set; }

        /// <summary>
        /// Буфер для склейки рендера, Float данных
        /// </summary>
        private readonly List<float> _bufferFloat = new List<float>();
        /// <summary>
        /// Буфер для склейки рендера, Byte данных
        /// </summary>
        private readonly List<int> _bufferInt = new List<int>();

        /// <summary>
        /// Очистить полигоны
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _bufferFloat.Clear();
            _bufferInt.Clear();
            CountVertices = 0;
        }

        /// <summary>
        /// Пополнить буфер полигонами
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(VertexEntityBuffer buffer)
        {
            _bufferFloat.AddRange(buffer.BufferFloat);
            _bufferInt.AddRange(buffer.BufferInt);
            CountVertices = _bufferInt.Count / VertexEntityBuffer.SizeInt;
        }

        /// <summary>
        /// Сгенерировать массив float[]
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] ToArrayFloat() => _bufferFloat.ToArray();

        /// <summary>
        /// Сгенерировать массив int[]
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int[] ToArrayInt() => _bufferInt.ToArray();
    }
}
