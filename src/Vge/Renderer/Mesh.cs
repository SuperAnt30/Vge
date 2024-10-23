using System;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект буфера сетки через VAO
    /// </summary>
    public abstract class Mesh : IDisposable
    {
        protected uint _ebo;
        protected readonly uint _vao;
        protected uint _vbo;
        protected readonly GL _gl;
        protected int _countVertices;
        private int _countIndices;
        protected int _vertexSize;

        public Mesh(GL gl)
        {
            _gl = gl;
            uint[] id = new uint[1];
            gl.GenVertexArrays(1, id);
            _vao = id[0];
            gl.BindVertexArray(_vao);
            Debug.MeshId = _vao;

            // Инициализация VBO
            _InitVbo();

            // Инициализация ebo если это надо
            _InitEbo();

            // Инициализация атрибут
            _InitAtributs();
            //gl.BindVertexArray(0); // ?
        }

        /// <summary>
        /// Инициализация VBO
        /// </summary>
        protected virtual void _InitVbo()
        {
            uint[] id = new uint[1];
            _gl.GenBuffers(1, id);
            _vbo = id[0];
        }

        /// <summary>
        /// Инициализация ebo если это надо
        /// </summary>
        protected virtual void _InitEbo()
        {
            uint[] id = new uint[1];
            _gl.GenBuffers(1, id);
            _ebo = id[0];
        }

        /// <summary>
        /// Инициализация атрибут
        /// </summary>
        protected virtual void _InitAtributs() { }

        /// <summary>
        /// Инициализация атрибут
        /// </summary>
        protected void _InitAtributs(int[] attrs)
        {
            uint i;
            int count = attrs.Length;
            for (i = 0; i < count; i++)
            {
                _vertexSize += attrs[i];
            }

            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vbo);

            int stride = _vertexSize * sizeof(float);
            int offset = 0;
            for (i = 0; i < count; i++)
            {
                if (attrs[i] == 0) break;
                int size = attrs[i];
                _gl.VertexAttribPointer(i, size, GL.GL_FLOAT, false,
                    stride, new IntPtr(offset * sizeof(float)));
                _gl.EnableVertexAttribArray(i);
                offset += size;
            }
        }

        /// <summary>
        /// Генерация массива вершин, для построения квада
        /// </summary>
        protected int[] QuadIndices()
        {
            _countIndices = _countVertices / 4 * 6;
            int[] indices = new int[_countIndices];
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
            _gl.DeleteVertexArrays(1, new uint[] { _vao });
            _gl.DeleteBuffers(1, new uint[] { _vbo });
            if (_ebo != 0)
            {
                _gl.DeleteBuffers(1, new uint[] { _ebo });
            }
        }

        public void Dispose() => Delete();

        /// <summary>
        /// Прорисовать меш с треугольными полигонами через EBO
        /// </summary>
        public virtual void Draw()
        {
            Debug.MeshCount++;
            _gl.BindVertexArray(_vao);
            _gl.DrawElements(GL.GL_TRIANGLES, _countIndices, GL.GL_UNSIGNED_INT, IntPtr.Zero);
        }

        #region Reload

        /// <summary>
        /// Перезаписать полигоны, не создавая и не меняя длинну одной точки
        /// </summary>
        public virtual void Reload(float[] vertices)
        {
            _countVertices = vertices.Length / _vertexSize;
            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vbo);
            _gl.BufferData(GL.GL_ARRAY_BUFFER, vertices, GL.GL_STATIC_DRAW);
            _gl.BindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, _ebo);
            _gl.BufferData(GL.GL_ELEMENT_ARRAY_BUFFER, QuadIndices(), GL.GL_STREAM_DRAW);
        }

        /// <summary>
        /// Перезаписать полигоны, не создавая и не меняя длинну одной точки
        /// </summary>
        public void Reload(float[] vertices, int count)
        {
            _countVertices = count / _vertexSize;
            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vbo);
            _gl.BufferData(GL.GL_ARRAY_BUFFER, count, vertices, GL.GL_STATIC_DRAW);
            _gl.BindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, _ebo);
            _gl.BufferData(GL.GL_ELEMENT_ARRAY_BUFFER, QuadIndices(), GL.GL_STREAM_DRAW);
        }

        /// <summary>
        /// Перезаписать полигоны, не создавая и не меняя длинну одной точки
        /// </summary>
        public void Reload(BufferFast bufferFast)
        {
            int count = bufferFast.Count;
            _countVertices = count / _vertexSize;
            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vbo);
            _gl.BufferData(GL.GL_ARRAY_BUFFER, count, bufferFast.ToBuffer(), GL.GL_STATIC_DRAW);
            _gl.BindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, _ebo);
            _gl.BufferData(GL.GL_ELEMENT_ARRAY_BUFFER, QuadIndices(), GL.GL_STREAM_DRAW);
        }

        #endregion
    }
}
