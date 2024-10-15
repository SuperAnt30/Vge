using System;
using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект буфера сетки через VAO
    /// </summary>
    public abstract class Mesh : IDisposable
    {
        private readonly uint ebo;
        protected readonly uint vao;
        protected readonly uint vbo;
        private readonly int[] attrs;
        protected readonly GL gl;
        protected int countVertices;
        private int countIndices;
        protected readonly int vertexSize;
        /// <summary>
        /// Количество float в буфере на один полигон
        /// </summary>
        public readonly int PoligonFloat;

        public Mesh(GL gl, int[] attrs, bool isQuad = true)
        {
            this.gl = gl;
            for (int i = 0; i < attrs.Length; i++)
            {
                vertexSize += attrs[i];
            }
            PoligonFloat = vertexSize * 3;
            this.attrs = attrs;

            uint[] id = new uint[1];
            gl.GenVertexArrays(1, id);
            vao = id[0];
            gl.BindVertexArray(vao);
            gl.GenBuffers(1, id);
            vbo = id[0];
            gl.BindBuffer(GL.GL_ARRAY_BUFFER, vbo);
            if (isQuad)
            {
                gl.GenBuffers(1, id);
                ebo = id[0];
            }

            Debug.MeshId = vao;

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
            gl.BindVertexArray(0); // ?
        }

        public Mesh(GL gl, float[] vertices, int[] attrs) : this(gl, attrs)
            => Reload(vertices);

        /// <summary>
        /// Генерация массива вершин, для построения квада
        /// </summary>
        private int[] QuadIndices()
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
            if (ebo != 0)
            {
                gl.DeleteBuffers(1, new uint[] { ebo });
            }
        }

        public void Dispose() => Delete();

        /// <summary>
        /// Прорисовать меш с треугольными полигонами через EBO
        /// </summary>
        public virtual void Draw()
        {
            Debug.MeshCount++;
            gl.BindVertexArray(vao);
            gl.DrawElements(GL.GL_TRIANGLES, countIndices, GL.GL_UNSIGNED_INT, IntPtr.Zero);
        }

        #region Reload

        /// <summary>
        /// Перезаписать полигоны, не создавая и не меняя длинну одной точки
        /// </summary>
        public virtual void Reload(float[] vertices)
        {
            countVertices = vertices.Length / vertexSize;
            gl.BindVertexArray(vao);
            gl.BindBuffer(GL.GL_ARRAY_BUFFER, vbo);
            gl.BufferData(GL.GL_ARRAY_BUFFER, vertices, GL.GL_STATIC_DRAW);
            gl.BindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, ebo);
            gl.BufferData(GL.GL_ELEMENT_ARRAY_BUFFER, QuadIndices(), GL.GL_STREAM_DRAW);
        }

        /// <summary>
        /// Перезаписать полигоны, не создавая и не меняя длинну одной точки
        /// </summary>
        public void Reload(float[] vertices, int count)
        {
            countVertices = count / vertexSize;
            gl.BindVertexArray(vao);
            gl.BindBuffer(GL.GL_ARRAY_BUFFER, vbo);
            gl.BufferData(GL.GL_ARRAY_BUFFER, count, vertices, GL.GL_STATIC_DRAW);
            gl.BindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, ebo);
            gl.BufferData(GL.GL_ELEMENT_ARRAY_BUFFER, QuadIndices(), GL.GL_STREAM_DRAW);
        }

        #endregion
    }
}
