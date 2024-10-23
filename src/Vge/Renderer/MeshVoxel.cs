using System;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект сетки для воксельного мира с цветом альфа без текстуры
    /// </summary>
    public class MeshVoxel : Mesh
    {
        /// <summary>
        /// Дополнительный буфер для байтовых данных
        /// </summary>
        private uint _vboByte;

        public MeshVoxel(GL gl) : base(gl) { }

        protected override void _InitVbo()
        {
            uint[] id = new uint[2];
            _gl.GenBuffers(2, id);
            _vbo = id[0];
            _vboByte = id[1];
        }

        /// <summary>
        /// Инициализация атрибут
        /// </summary>
        protected override void _InitAtributs()
        {
            int countAttr = 7;// 3 + 2 + 1 + 1;
            _vertexSize = 5;// 3 + 2;
            int stride = countAttr * sizeof(float);

            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vbo);

            _gl.VertexAttribPointer(0, 3, GL.GL_FLOAT, false, 20, new IntPtr(0 * sizeof(float)));
            _gl.EnableVertexAttribArray(0);

            _gl.VertexAttribPointer(1, 2, GL.GL_FLOAT, false, 20, new IntPtr(3 * sizeof(float)));
            _gl.EnableVertexAttribArray(1);

            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vboByte);

            _gl.VertexAttribPointer(2, 1, GL.GL_FLOAT, false, 8, new IntPtr(0 * sizeof(int)));
            _gl.EnableVertexAttribArray(2);

            _gl.VertexAttribPointer(3, 1, GL.GL_FLOAT, false, 8, new IntPtr(1 * sizeof(int)));
            _gl.EnableVertexAttribArray(3);
        }

        /// <summary>
        /// Перезаписать полигоны, не создавая и не меняя длинну одной точки
        /// </summary>
        public void Reload(BufferFastFloat bufferFastFloat, BufferFast bufferFast)
        {
            int count = bufferFastFloat.Count * sizeof(float);
            _countVertices = count / _vertexSize;
            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vbo);
            _gl.BufferData(GL.GL_ARRAY_BUFFER, count, bufferFastFloat.ToBuffer(), GL.GL_STATIC_DRAW);
            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vboByte);
            _gl.BufferData(GL.GL_ARRAY_BUFFER, count, bufferFast.ToBuffer(), GL.GL_STATIC_DRAW);
            _gl.BindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, _ebo);
            _gl.BufferData(GL.GL_ELEMENT_ARRAY_BUFFER, QuadIndices(), GL.GL_STREAM_DRAW);
        }
    }
}
