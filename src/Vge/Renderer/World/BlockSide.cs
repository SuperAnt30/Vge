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

        /// <summary>
        /// Для анимации блока, указывается количество кадров в игровом времени (50 мс),
        /// можно кратно 2 в степени (2, 4, 8, 16, 32, 64...)
        /// 0 - нет анимации
        /// </summary>
        public byte AnimationFrame;
        /// <summary>
        /// Для анимации блока, указывается пауза между кадрами в игровом времени (50 мс),
        /// можно кратно 2 в степени (2, 4, 8, 16, 32, 64...)
        /// 0 или 1 - нет задержки, каждый такт игры смена кадра
        /// </summary>
        public byte AnimationPause;
        /// <summary>
        /// Резкость = 4; mipmap = 0;
        /// </summary>
        public byte Sharpness;

        /// <summary>
        /// Построение буфера
        /// </summary>
        public void Building()
        {
            // С EBO
            Buffer.AddVertex(Vertex[0].X + PosCenterX, Vertex[0].Y + PosCenterY, Vertex[0].Z + PosCenterZ,
                Vertex[0].U, Vertex[0].V, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0], Sharpness);
            Buffer.AddVertex(Vertex[1].X + PosCenterX, Vertex[1].Y + PosCenterY, Vertex[1].Z + PosCenterZ,
                Vertex[1].U, Vertex[1].V, ColorsR[1], ColorsG[1], ColorsB[1], Lights[1], Sharpness);
            Buffer.AddVertex(Vertex[2].X + PosCenterX, Vertex[2].Y + PosCenterY, Vertex[2].Z + PosCenterZ,
                Vertex[2].U, Vertex[2].V, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2], Sharpness);
            Buffer.AddVertex(Vertex[3].X + PosCenterX, Vertex[3].Y + PosCenterY, Vertex[3].Z + PosCenterZ,
                Vertex[3].U, Vertex[3].V, ColorsR[3], ColorsG[3], ColorsB[3], Lights[3], Sharpness);
        }

        /// <summary>
        /// Построение буфера с ветром
        /// </summary>
        public void BuildingWind(byte wind)
        {
            byte f1, f2, f3, f4;

            if (wind == 1)
            {
                // Ветер как для травы, низ не двигается, вверх двигается
                f1 = f4 = Sharpness;
                f2 = f3 = (byte)(Sharpness + 1);
            }
            else if(wind == 2)
            {
                // Ветер как для ветки снизу, вверхняя часть не двигается, нижняя двигается
                f2 = f3 = Sharpness;
                f1 = f4 = (byte)(Sharpness + 1);
            }
            else if (wind == 3)
            {
                // Ветер двигается всё
                f1 = f2 = f3 = f4 = (byte)(Sharpness + 1);
            }
            else
            {
                // Нет ветра
                f1 = f2 = f3 = f4 = Sharpness;
            }

            Buffer.AddVertex(Vertex[0].X + PosCenterX, Vertex[0].Y + PosCenterY, Vertex[0].Z + PosCenterZ,
                    Vertex[0].U, Vertex[0].V, ColorsR[0], ColorsG[0], ColorsB[0], Lights[0], f1);
            Buffer.AddVertex(Vertex[1].X + PosCenterX, Vertex[1].Y + PosCenterY, Vertex[1].Z + PosCenterZ,
                Vertex[1].U, Vertex[1].V, ColorsR[1], ColorsG[1], ColorsB[1], Lights[1], f2);
            Buffer.AddVertex(Vertex[2].X + PosCenterX, Vertex[2].Y + PosCenterY, Vertex[2].Z + PosCenterZ,
                Vertex[2].U, Vertex[2].V, ColorsR[2], ColorsG[2], ColorsB[2], Lights[2], f3);
            Buffer.AddVertex(Vertex[3].X + PosCenterX, Vertex[3].Y + PosCenterY, Vertex[3].Z + PosCenterZ,
                Vertex[3].U, Vertex[3].V, ColorsR[3], ColorsG[3], ColorsB[3], Lights[3], f4);
        }

            /*

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
