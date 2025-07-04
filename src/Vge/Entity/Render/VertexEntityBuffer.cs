using System;
using System.Runtime.CompilerServices;

namespace Vge.Entity.Render
{
    /// <summary>
    /// Буфер вершин сущности
    /// </summary>
    public class VertexEntityBuffer
    {
        /// <summary>
        /// Количество элементов в типе buffersInt в одной вершине
        /// </summary>
        private const byte _sizeInt = 2;

        /// <summary>
        /// Буфер для склейки рендера, Float данных
        /// </summary>
        public readonly float[] BufferFloat;
        /// <summary>
        /// Буфер для склейки рендера, Byte данных
        /// </summary>
        public readonly int[] BufferInt;

        public VertexEntityBuffer(float[] bufferFloat, int[] bufferInt)
        {
            BufferFloat = bufferFloat;
            BufferInt = bufferInt;
        }

        /// <summary>
        /// Пустой
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Empty() => BufferInt.Length == 0;

        /// <summary>
        /// Получить количество вершин
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCountVertices() => BufferInt.Length / _sizeInt;

        /// <summary>
        /// Корректировка размера ширины текстуры, в буффере UV
        /// </summary>
        public void SizeAdjustmentTextureWidth(float coef)
        {
            // XYZ UV B - 6 флоатов на вершину
            for (int i = 3; i < BufferFloat.Length; i += 5)
            {
                BufferFloat[i] *= coef;
            }
        }

        /// <summary>
        /// Корректировка размера высоты текстуры, в буффере UV
        /// </summary>
        public void SizeAdjustmentTextureHeight(float coef)
        {
            // XYZ UV B - 6 флоатов на вершину
            for (int i = 4; i < BufferFloat.Length; i += 5)
            {
                BufferFloat[i] *= coef;
            }
        }

        /// <summary>
        /// Копия буфера сетки с масштабом
        /// </summary>
        public VertexEntityBuffer CopyBufferMesh(float scale = 1)
        {
            float[] bufferFloat = new float[BufferFloat.Length];
            Array.Copy(BufferFloat, bufferFloat, bufferFloat.Length);
            int[] bufferInt = new int[BufferInt.Length];
            Array.Copy(BufferInt, bufferInt, bufferInt.Length);

            if (scale != 1)
            {
                for (int i = 0; i < bufferFloat.Length; i += 5)
                {
                    bufferFloat[i] *= scale;
                    bufferFloat[i + 1] *= scale;
                    bufferFloat[i + 2] *= scale;
                }
            }

            return new VertexEntityBuffer(bufferFloat, bufferInt);
        }
    }
}
