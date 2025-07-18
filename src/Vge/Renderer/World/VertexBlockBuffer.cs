using System;
using System.Runtime.CompilerServices;
using WinGL.Util;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Буфер вершин блоков
    /// </summary>
    public class VertexBlockBuffer : IDisposable
    {
        /// <summary>
        /// Количество элементов Float в одной вершине
        /// </summary>
        private const byte _sizeFloat = 5;
        /// <summary>
        /// Количество элементов Byte в одной вершине
        /// </summary>
        private const byte _sizeByte = 8;

        /// <summary>
        /// Буфер для склейки рендера, Float данных
        /// </summary>
        public readonly BufferFastFloat BufferFloat;
        /// <summary>
        /// Буфер для склейки рендера, Byte данных
        /// </summary>
        public readonly BufferFastByte BufferByte;

        public VertexBlockBuffer(int maxSizeVertex = 100)
        {
            BufferFloat = new BufferFastFloat(maxSizeVertex * _sizeFloat);
            BufferByte = new BufferFastByte(maxSizeVertex * _sizeByte);
        }

        /// <summary>
        /// Пустой
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Empty() => BufferByte.Count == 0;

        /// <summary>
        /// Получить количество вершин
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCountVertices() => BufferByte.Count / _sizeByte;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            BufferFloat.Clear();
            BufferByte.Clear();
        }

        /// <summary>
        /// Добавить буфер с дистанции
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBlockBufferDistance(BlockBufferDistance blockBuffer)
        {
            BufferFloat.AddRange(blockBuffer.BufferFloat);
            BufferByte.AddRange(blockBuffer.BufferByte);
        }

        /// <summary>
        /// Добавить вершину
        /// </summary>
        /// <param name="flag">По битовые данные, 0 - wind (ветер), 1 - wave (волна), 2 - sharpness (резкость)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddVertex(float x, float y, float z, float u, float v, uint normal,
            byte r, byte g, byte b, byte light, byte animFrame, byte animPause, byte flag)
        {
            BufferFloat.Add(x);
            BufferFloat.Add(y);
            BufferFloat.Add(z);
            BufferFloat.Add(u);
            BufferFloat.Add(v);

            BufferByte.Add(r);
            BufferByte.Add(g);
            BufferByte.Add(b);
            BufferByte.Add(light);
            BufferByte.Add(animFrame);
            BufferByte.Add(animPause);
            BufferByte.Add(flag);
            BufferByte.AddNull();
            BufferByte.Add((byte)(normal & 0xFF));
            BufferByte.Add((byte)((normal >> 8) & 0xFF));
            BufferByte.Add((byte)((normal >> 16) & 0xFF));
            BufferByte.AddNull();
        }

        public void Dispose()
        {
            BufferFloat.Dispose();
            BufferByte.Dispose();
        }
    }
}
