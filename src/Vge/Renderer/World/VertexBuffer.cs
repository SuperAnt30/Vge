using System;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Буфер вершин блоков
    /// </summary>
    public class VertexBuffer : IDisposable
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

        public VertexBuffer(int maxSizeVertex = 100)
        {
            BufferFloat = new BufferFastFloat(maxSizeVertex * _sizeFloat);
            BufferByte = new BufferFastByte(maxSizeVertex * _sizeByte);
        }

        /// <summary>
        /// Получить количество вершин
        /// </summary>
        public int GetCountVertices() => BufferByte.Count / _sizeByte;

        public void Clear()
        {
            BufferFloat.Clear();
            BufferByte.Clear();
        }

        /// <summary>
        /// Добавить вершину
        /// </summary>
        public void AddVertex(float x, float y, float z, float u, float v, 
            byte r, byte g, byte b, byte light, byte height = 0)
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
            BufferByte.Add(0);
            BufferByte.Add(0);
            BufferByte.Add(height);
            BufferByte.AddNull();
        }

        public void Dispose()
        {
            BufferFloat.Dispose();
            BufferByte.Dispose();
        }
    }
}
