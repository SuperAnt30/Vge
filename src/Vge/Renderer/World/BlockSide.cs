using Vge.Util;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Построение полного блока с разных сторон
    /// </summary>
    public class BlockSide
    {
        /// <summary>
        /// Основной буфер
        /// </summary>
        public VertexBuffer Buffer;
        /// <summary>
        /// Буфер кэш для внутренней части жидких блоков
        /// </summary>
        public VertexBuffer BufferCache;

        public Vertex3d[] Vertex;

        public byte[] ColorsR;
        public byte[] ColorsG;
        public byte[] ColorsB;
        public byte[] Lights;

        public float PosCenterX;
        public float PosCenterY;
        public float PosCenterZ;

        public byte AnimationFrame;
        public byte AnimationPause;

        private float pos1x, pos1y, pos1z, pos2x, pos2y, pos2z, pos3x, pos3y, pos3z, pos4x, pos4y, pos4z;
        private float u1, u2, u3, u4, v1, v2, v3, v4;

        /// <summary>
        /// Построение буфера
        /// </summary>
        public void Building()
        {
            pos1x = Vertex[0].X + PosCenterX;
            pos1y = Vertex[0].Y + PosCenterY;
            pos1z = Vertex[0].Z + PosCenterZ;
            pos2x = Vertex[1].X + PosCenterX;
            pos2y = Vertex[1].Y + PosCenterY;
            pos2z = Vertex[1].Z + PosCenterZ;
            pos3x = Vertex[2].X + PosCenterX;
            pos3y = Vertex[2].Y + PosCenterY;
            pos3z = Vertex[2].Z + PosCenterZ;
            pos4x = Vertex[3].X + PosCenterX;
            pos4y = Vertex[3].Y + PosCenterY;
            pos4z = Vertex[3].Z + PosCenterZ;

            u1 = Vertex[0].U;
            u2 = Vertex[1].U;
            u3 = Vertex[2].U;
            u4 = Vertex[3].U;
            v1 = Vertex[0].V;
            v2 = Vertex[1].V;
            v3 = Vertex[2].V;
            v4 = Vertex[3].V;

            // БЕЗ EBO
            //Buffer.AddVertex(pos1x, pos1y, pos1z, u1, v1, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0]);
            //Buffer.AddVertex(pos2x, pos2y, pos2z, u2, v2, ColorsR[1], ColorsG[1], ColorsB[1], Lights[1]);
            //Buffer.AddVertex(pos3x, pos3y, pos3z, u3, v3, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2]);
            //Buffer.AddVertex(pos1x, pos1y, pos1z, u1, v1, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0]);
            //Buffer.AddVertex(pos3x, pos3y, pos3z, u3, v3, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2]);
            //Buffer.AddVertex(pos4x, pos4y, pos4z, u4, v4, ColorsR[3], ColorsG[3], ColorsB[3], Lights[3]);

            // С EBO
            Buffer.AddVertex(pos1x, pos1y, pos1z, u1, v1, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0]);
            Buffer.AddVertex(pos2x, pos2y, pos2z, u2, v2, ColorsR[1], ColorsG[1], ColorsB[1], Lights[1]);
            Buffer.AddVertex(pos3x, pos3y, pos3z, u3, v3, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2]);
            Buffer.AddVertex(pos4x, pos4y, pos4z, u4, v4, ColorsR[3], ColorsG[3], ColorsB[3], Lights[3]);
        }

        /*
        /// <summary>
        /// Построение буфера
        /// </summary>
        public void BuildingWind(byte wind)
        {
            pos1x = Vertex[0].x + PosCenterX;
            pos1y = Vertex[0].y + PosCenterY;
            pos1z = Vertex[0].z + PosCenterZ;
            pos2x = Vertex[1].x + PosCenterX;
            pos2y = Vertex[1].y + PosCenterY;
            pos2z = Vertex[1].z + PosCenterZ;
            pos3x = Vertex[2].x + PosCenterX;
            pos3y = Vertex[2].y + PosCenterY;
            pos3z = Vertex[2].z + PosCenterZ;
            pos4x = Vertex[3].x + PosCenterX;
            pos4y = Vertex[3].y + PosCenterY;
            pos4z = Vertex[3].z + PosCenterZ;

            u1 = Vertex[0].u;
            u2 = Vertex[1].u;
            u3 = Vertex[2].u;
            u4 = Vertex[3].u;
            v1 = Vertex[0].v;
            v2 = Vertex[1].v;
            v3 = Vertex[2].v;
            v4 = Vertex[3].v;

            if (wind == 0)
            {
                // Нет ветра
                AddVertex(pos1x, pos1y, pos1z, u1, v1, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0]);
                AddVertex(pos2x, pos2y, pos2z, u2, v2, ColorsR[1], ColorsG[1], ColorsB[1], Lights[1]);
                AddVertex(pos3x, pos3y, pos3z, u3, v3, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2]);
                AddVertex(pos1x, pos1y, pos1z, u1, v1, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0]);
                AddVertex(pos3x, pos3y, pos3z, u3, v3, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2]);
                AddVertex(pos4x, pos4y, pos4z, u4, v4, ColorsR[3], ColorsG[3], ColorsB[3], Lights[3]);
            }
            else if (wind == 1)
            {
                // Ветер как для травы, низ не двигается, вверх двигается
                AddVertex(pos1x, pos1y, pos1z, u1, v1, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0], 0);
                AddVertex(pos2x, pos2y, pos2z, u2, v2, ColorsR[1], ColorsG[1], ColorsB[1], Lights[1], 1);
                AddVertex(pos3x, pos3y, pos3z, u3, v3, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2], 1);
                AddVertex(pos1x, pos1y, pos1z, u1, v1, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0], 0);
                AddVertex(pos3x, pos3y, pos3z, u3, v3, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2], 1);
                AddVertex(pos4x, pos4y, pos4z, u4, v4, ColorsR[3], ColorsG[3], ColorsB[3], Lights[3], 0);
            }
            else if (wind == 2)
            {
                // Ветер как для ветки снизу, вверхняя часть не двигается, нижняя двигается
                AddVertex(pos1x, pos1y, pos1z, u1, v1, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0], 1);
                AddVertex(pos2x, pos2y, pos2z, u2, v2, ColorsR[1], ColorsG[1], ColorsB[1], Lights[1], 0);
                AddVertex(pos3x, pos3y, pos3z, u3, v3, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2], 0);
                AddVertex(pos1x, pos1y, pos1z, u1, v1, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0], 1);
                AddVertex(pos3x, pos3y, pos3z, u3, v3, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2], 0);
                AddVertex(pos4x, pos4y, pos4z, u4, v4, ColorsR[3], ColorsG[3], ColorsB[3], Lights[3], 1);
            }
            else
            {
                // Двигается всё
                AddVertex(pos1x, pos1y, pos1z, u1, v1, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0], 1);
                AddVertex(pos2x, pos2y, pos2z, u2, v2, ColorsR[1], ColorsG[1], ColorsB[1], Lights[1], 1);
                AddVertex(pos3x, pos3y, pos3z, u3, v3, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2], 1);
                AddVertex(pos1x, pos1y, pos1z, u1, v1, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0], 1);
                AddVertex(pos3x, pos3y, pos3z, u3, v3, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2], 1);
                AddVertex(pos4x, pos4y, pos4z, u4, v4, ColorsR[3], ColorsG[3], ColorsB[3], Lights[3], 1);
            }
        }

        /// <summary>
        /// Построение буфера с новой текстурой на прошлых позициях для разрушения
        /// </summary>
        public void BuildingDamaged(int numberTexture)
        {
            pos1x = Vertex[0].x + PosCenterX;
            pos1y = Vertex[0].y + PosCenterY;
            pos1z = Vertex[0].z + PosCenterZ;
            pos2x = Vertex[1].x + PosCenterX;
            pos2y = Vertex[1].y + PosCenterY;
            pos2z = Vertex[1].z + PosCenterZ;
            pos3x = Vertex[2].x + PosCenterX;
            pos3y = Vertex[2].y + PosCenterY;
            pos3z = Vertex[2].z + PosCenterZ;
            pos4x = Vertex[3].x + PosCenterX;
            pos4y = Vertex[3].y + PosCenterY;
            pos4z = Vertex[3].z + PosCenterZ;

            u3 = u4 = (numberTexture % 64) * .015625f;
            u1 = u2 = u3 + .015625f * 2f;
            v2 = v3 = numberTexture / 64 * .015625f;
            v1 = v4 = v2 + .015625f * 2f;

            // снаружи
            AddVertex(pos1x, pos1y, pos1z, u1, v1, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0]);
            AddVertex(pos2x, pos2y, pos2z, u2, v2, ColorsR[1], ColorsG[1], ColorsB[1], Lights[1]);
            AddVertex(pos3x, pos3y, pos3z, u3, v3, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2]);
            AddVertex(pos1x, pos1y, pos1z, u1, v1, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0]);
            AddVertex(pos3x, pos3y, pos3z, u3, v3, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2]);
            AddVertex(pos4x, pos4y, pos4z, u4, v4, ColorsR[3], ColorsG[3], ColorsB[3], Lights[3]);
        }

        /// <summary>
        /// Добавить вершину
        /// </summary>
        private void AddVertex(float x, float y, float z, float u, float v, 
            byte r, byte g, byte b, byte light, byte height = 0)
        {
            buffer.AddFloat(BitConverter.GetBytes(x));
            buffer.AddFloat(BitConverter.GetBytes(y));
            buffer.AddFloat(BitConverter.GetBytes(z));
            buffer.AddFloat(BitConverter.GetBytes(u));
            buffer.AddFloat(BitConverter.GetBytes(v));
            buffer.buffer[buffer.count++] = r;
            buffer.buffer[buffer.count++] = g;
            buffer.buffer[buffer.count++] = b;
            buffer.buffer[buffer.count++] = light;
            buffer.buffer[buffer.count++] = AnimationFrame;
            buffer.buffer[buffer.count++] = AnimationPause;
            buffer.buffer[buffer.count++] = height;
            buffer.count++;
        }

        /// <summary>
        /// Добавить вершину в кэш
        /// </summary>
        private void AddVertexCache(float x, float y, float z, float u, float v, byte r, byte g, byte b, byte light, byte height = 0)
        {
            bufferCache.AddRange(BitConverter.GetBytes(x));
            bufferCache.AddRange(BitConverter.GetBytes(y));
            bufferCache.AddRange(BitConverter.GetBytes(z));
            bufferCache.AddRange(BitConverter.GetBytes(u));
            bufferCache.AddRange(BitConverter.GetBytes(v));
            bufferCache.buffer[bufferCache.count++] = r;
            bufferCache.buffer[bufferCache.count++] = g;
            bufferCache.buffer[bufferCache.count++] = b;
            bufferCache.buffer[bufferCache.count++] = light;
            bufferCache.buffer[bufferCache.count++] = AnimationFrame;
            bufferCache.buffer[bufferCache.count++] = AnimationPause;
            bufferCache.buffer[bufferCache.count++] = height;
            bufferCache.count++;
        }

        /// <summary>
        /// Сгенерировать сетку VBO четырёхугольника снаружи
        /// </summary>
        public void BufferSideOutside(float pos1x, float pos1y, float pos1z,
            float pos2x, float pos2y, float pos2z,
            float pos3x, float pos3y, float pos3z,
            float pos4x, float pos4y, float pos4z,
            float u1, float v1, float u2, float v2,
            float u3, float v3, float u4, float v4
            )
        {
            AddVertex(pos1x, pos1y, pos1z, u1, v1, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0]);
            AddVertex(pos2x, pos2y, pos2z, u2, v2, ColorsR[1], ColorsG[1], ColorsB[1], Lights[1]);
            AddVertex(pos3x, pos3y, pos3z, u3, v3, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2]);
            AddVertex(pos1x, pos1y, pos1z, u1, v1, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0]);
            AddVertex(pos3x, pos3y, pos3z, u3, v3, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2]);
            AddVertex(pos4x, pos4y, pos4z, u4, v4, ColorsR[3], ColorsG[3], ColorsB[3], Lights[3]);
        }

        /// <summary>
        /// Сгенерировать сетку VBO четырёхугольника снаружи
        /// </summary>
        public void BufferSideOutsideCache(float pos1x, float pos1y, float pos1z,
            float pos2x, float pos2y, float pos2z,
            float pos3x, float pos3y, float pos3z,
            float pos4x, float pos4y, float pos4z,
            float u1, float v1, float u2, float v2,
            float u3, float v3, float u4, float v4,
            byte h1 = 0, byte h2 = 0, byte h3 = 0, byte h4 = 0
            )
        {
            AddVertexCache(pos1x, pos1y, pos1z, u1, v1, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0], h1);
            AddVertexCache(pos2x, pos2y, pos2z, u2, v2, ColorsR[1], ColorsG[1], ColorsB[1], Lights[1], h2);
            AddVertexCache(pos3x, pos3y, pos3z, u3, v3, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2], h3);
            AddVertexCache(pos1x, pos1y, pos1z, u1, v1, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0], h1);
            AddVertexCache(pos3x, pos3y, pos3z, u3, v3, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2], h3);
            AddVertexCache(pos4x, pos4y, pos4z, u4, v4, ColorsR[3], ColorsG[3], ColorsB[3], Lights[3], h4);
        }

        /// <summary>
        /// Сгенерировать сетку VBO четырёхугольника изнутри
        /// </summary>
        public void BufferSideInside(float pos1x, float pos1y, float pos1z,
            float pos2x, float pos2y, float pos2z,
            float pos3x, float pos3y, float pos3z,
            float pos4x, float pos4y, float pos4z,
            float u1, float v1, float u2, float v2,
            float u3, float v3, float u4, float v4,
            byte h1 = 0, byte h2 = 0, byte h3 = 0, byte h4 = 0
            )
        {
            AddVertex(pos3x, pos3y, pos3z, u3, v3, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2], h3);
            AddVertex(pos2x, pos2y, pos2z, u2, v2, ColorsR[1], ColorsG[1], ColorsB[1], Lights[1], h2);
            AddVertex(pos1x, pos1y, pos1z, u1, v1, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0], h1);
            AddVertex(pos4x, pos4y, pos4z, u4, v4, ColorsR[3], ColorsG[3], ColorsB[3], Lights[3], h4);
            AddVertex(pos3x, pos3y, pos3z, u3, v3, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2], h3);
            AddVertex(pos1x, pos1y, pos1z, u1, v1, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0], h1);
        }

        /// <summary>
        /// Сгенерировать сетку VBO четырёхугольника с двух сторон
        /// </summary>
        public void BufferSideTwo(float pos1x, float pos1y, float pos1z,
            float pos2x, float pos2y, float pos2z,
            float pos3x, float pos3y, float pos3z,
            float pos4x, float pos4y, float pos4z,
            float u1, float v1, float u2, float v2,
            float u3, float v3, float u4, float v4,
            bool insideNot = false,
            byte h1 = 0, byte h2 = 0, byte h3 = 0, byte h4 = 0
            )
        {
            if (!insideNot)
            {
                BufferSideInside(pos1x, pos1y, pos1z, pos2x, pos2y, pos2z, pos3x, pos3y, pos3z, pos4x, pos4y, pos4z,
                    u1, v1, u2, v2, u3, v3, u4, v4, h1, h2, h3, h4);
            }
            BufferSideOutsideCache(pos1x, pos1y, pos1z, pos2x, pos2y, pos2z, pos3x, pos3y, pos3z, pos4x, pos4y, pos4z,
                u1, v1, u2, v2, u3, v3, u4, v4, h1, h2, h3, h4);
        }

        */

        /// <summary>
        /// Добавить кэш сетку в основной буфер
        /// </summary>
        //public void AddBufferCache()
        //{
        //    if (bufferCache.count > 0)
        //    {
        //        buffer.AddRange(bufferCache);
        //    }
        //}
    }
}
