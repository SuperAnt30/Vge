using System;

namespace WinGL.OpenGL
{
    /// <summary>
    /// Объект буфера сетки через VAO
    /// </summary>
    public class Mesh : IDisposable
    {
        private uint ebo = 0;
        private uint vao = 0;
        private uint vbo = 0;
        private int[] attrs;
        private GL gl;
        private int countVertices;
        private int countIndices;
        private readonly int vertexSize;
        /// <summary>
        /// Количество float в буфере на один полигон
        /// </summary>
        public int PoligonFloat { get; private set; }

        public Mesh(GL gl, float[] vertices, int[] attrs)
        {
            this.gl = gl;
            for (int i = 0; i < attrs.Length; i++)
            {
                vertexSize += attrs[i];
            }
            countVertices = vertices.Length / vertexSize;
            PoligonFloat = vertexSize * 3;
            this.attrs = attrs;
            BufferData(vertices);
        }

        private void BufferData(float[] vertices)
        {
            uint[] id = new uint[1];
            gl.GenVertexArrays(1, id);
            vao = id[0];
            gl.BindVertexArray(vao);
            gl.GenBuffers(1, id);
            vbo = id[0];
            gl.BindBuffer(GL.GL_ARRAY_BUFFER, vbo);
            gl.BufferData(GL.GL_ARRAY_BUFFER, vertices, GL.GL_STREAM_DRAW);

            gl.GenBuffers(1, id);
            ebo = id[0];
            gl.BindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, ebo);
            gl.BufferData(GL.GL_ELEMENT_ARRAY_BUFFER, Indices(), GL.GL_STREAM_DRAW);

            // attributes
            int stride = vertexSize * sizeof(float);
            int offset = 0;
            for (uint i = 0; i < attrs.Length; i++)
            {
                if (attrs[i] == 0) break;
                int size = attrs[i];
                gl.VertexAttribPointer(i, size, GL.GL_FLOAT, false, 
                    stride, new IntPtr(offset * sizeof(float)));
                gl.EnableVertexAttribArray(i);
                offset += size;
            }

            gl.BindVertexArray(0);
        }

        private int[] Indices()
        {
            countIndices = countVertices / 4 * 6;
            int[] indices = new int[countIndices];
            int[] quad = new int[] { 0, 1, 2, 1, 3, 2 };
            int c = 0;
            int c1 = 0;
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = quad[c++] + c1;
                if (c > 5)
                {
                    c = 0;
                    c1 += 4;
                }
            }
            return indices;
        }

        /// <summary>
        /// Удалить меш
        /// </summary>
        public void Delete()
        {
            gl.DeleteVertexArrays(1, new uint[] { vao });
            gl.DeleteBuffers(1, new uint[] { vbo });
            gl.DeleteBuffers(1, new uint[] { ebo });
        }

        /// <summary>
        /// Прорисовать меш с треугольными полигоvoa нами
        /// </summary>
        public void Draw()
        {
            gl.BindVertexArray(vao);
            //gl.DrawArrays(GL.GL_TRIANGLES, 0, countVertices);
            gl.DrawElements(GL.GL_TRIANGLES, countIndices, GL.GL_UNSIGNED_INT, IntPtr.Zero);
            gl.BindVertexArray(0);
        }

        /// <summary>
        /// Прорисовать меш с линиями
        /// </summary>
        //public void DrawLine()
        //{
        //    gl.BindVertexArray(vao);
        //    gl.DrawArrays(GL.GL_LINES, 0, countVertices);
        //    gl.BindVertexArray(0);
        //}

        /// <summary>
        /// Перезаписать полигоны, не создавая и не меняя длинну одной точки
        /// </summary>
        public void Reload(float[] vertices)
        {
            countVertices = vertices.Length / vertexSize;
            gl.BindVertexArray(vao);
            gl.BindBuffer(GL.GL_ARRAY_BUFFER, vbo);
            gl.BufferData(GL.GL_ARRAY_BUFFER, vertices, GL.GL_STATIC_DRAW);

            gl.BindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, ebo);
            gl.BufferData(GL.GL_ELEMENT_ARRAY_BUFFER, Indices(), GL.GL_STREAM_DRAW);

        }

        public void Dispose() => Delete();
    }
}
