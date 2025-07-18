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
        public const byte SizeInt = 2;

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
        /// Склеит два буфера в новый
        /// </summary>
        public static VertexEntityBuffer CopyConcat(VertexEntityBuffer buffer1, VertexEntityBuffer buffer2)
        {
            int count1 = buffer1.BufferFloat.Length;
            int count2 = buffer2.BufferFloat.Length;
            float[] bf = new float[count1 + count2];
            Array.Copy(buffer1.BufferFloat, bf, count1);
            Array.Copy(buffer2.BufferFloat, 0, bf, count1, count2);

            count1 = buffer1.BufferInt.Length;
            count2 = buffer2.BufferInt.Length;
            int[] bi = new int[count1 + count2];
            Array.Copy(buffer1.BufferInt, bi, count1);
            Array.Copy(buffer2.BufferInt, 0, bi, count1, count2);
            return new VertexEntityBuffer(bf, bi);
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
        public int GetCountVertices() => BufferInt.Length / SizeInt;

        /// <summary>
        /// Корректировка размера ширины текстуры, в буффере UV
        /// </summary>
        public void SizeAdjustmentTextureWidth(float coef)
        {
            // XYZ UV B - 6 флоатов на вершину
            for (int i = 3; i < BufferFloat.Length; i += 8)
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
            for (int i = 4; i < BufferFloat.Length; i += 8)
            {
                BufferFloat[i] *= coef;
            }
        }

        /// <summary>
        /// Задать глубины текстуры на вершину, для одежды
        /// </summary>
        public void SetDepthTexture(int depth)
        {
            for (int i = 1; i < BufferInt.Length; i += SizeInt)
            {
                BufferInt[i] = depth;
            }
        }

        /// <summary>
        /// Изменить глубину текстуры для маленькой текстуры. На вершину, для одежды
        /// </summary>
        public void SetSmallDepthTexture()
        {
            for (int i = 1; i < BufferInt.Length; i += SizeInt)
            {
                BufferInt[i] += 65536;
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
                for (int i = 0; i < bufferFloat.Length; i += 8)
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
