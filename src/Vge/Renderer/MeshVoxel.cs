using System;
using Vge.Renderer.World;
using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект сетки для воксельного мира с цветом альфа без текстуры
    /// </summary>
    public class MeshVoxel : Mesh
    {
        /// <summary>
        /// Пометка изменения
        /// </summary>
        public bool IsModifiedRender = false;
        /// <summary>
        /// Статус обработки сетки
        /// </summary>
        public StatusMesh Status { get; private set; } = StatusMesh.Null;

        /// <summary>
        /// Буфер XYZ UV
        /// </summary>
        private readonly BufferSlot _bufferFloat = new BufferSlot();
        /// <summary>
        /// Буфер RBGLaFaP
        /// </summary>
        private readonly BufferSlot _bufferByte = new BufferSlot();

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

        //protected override void _InitEbo() { }

        ///// <summary>
        ///// Прорисовать меш с треугольными полигонами через EBO
        ///// </summary>
        //public override void Draw()
        //{
        //    if (_countVertices > 0)
        //    {
        //        Debug.MeshCount++;
        //        _gl.BindVertexArray(_vao);
        //        _gl.DrawArrays(GL.GL_TRIANGLES, 0, _countVertices);
        //    }
        //}

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
        //public void Reload(BufferFastFloat bufferFastFloat, BufferFastByte bufferFastByte)
        //{
        //    int count = bufferFastFloat.Count * sizeof(float);
        //    _countVertices = count / _vertexSize;
        //    _gl.BindVertexArray(_vao);
        //    _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vbo);
        //    _gl.BufferData(GL.GL_ARRAY_BUFFER, count, bufferFastFloat.ToBuffer(), GL.GL_STATIC_DRAW);
        //    _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vboByte);
        //    _gl.BufferData(GL.GL_ARRAY_BUFFER, bufferFastByte.Count, bufferFastByte.ToBuffer(), GL.GL_STATIC_DRAW);
        //    _gl.BindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, _ebo);
        //    _gl.BufferData(GL.GL_ELEMENT_ARRAY_BUFFER, QuadIndices(), GL.GL_STREAM_DRAW);
        //}

        /// <summary>
        /// Перезаписать полигоны, не создавая и не меняя длинну одной точки
        /// </summary>
        public void Reload()
        {
            // GL_STREAM_DRAW , GL_STATIC_DRAW, GL_DYNAMIC_DRAW
            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vbo);
            _gl.BufferData(GL.GL_ARRAY_BUFFER, _bufferFloat.Size, _bufferFloat.Buffer, GL.GL_DYNAMIC_DRAW);// GL.GL_STATIC_DRAW);
            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vboByte);
            _gl.BufferData(GL.GL_ARRAY_BUFFER, _bufferByte.Size, _bufferByte.Buffer, GL.GL_DYNAMIC_DRAW);
            _gl.BindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, _ebo);
            _gl.BufferData(GL.GL_ELEMENT_ARRAY_BUFFER, _QuadIndices(), GL.GL_DYNAMIC_DRAW);
        }

        public override void Delete()
        {
            _bufferFloat.Dispose();
            _bufferByte.Dispose();
            _gl.DeleteVertexArrays(1, new uint[] { _vao });
            _gl.DeleteBuffers(2, new uint[] { _vbo, _vboByte });
            _gl.DeleteBuffers(1, new uint[] { _ebo });
            Status = StatusMesh.Null;
        }

        /// <summary>
        /// Изменить статус на рендеринг
        /// </summary>
        public void StatusRendering()
        {
            IsModifiedRender = false;
            Status = StatusMesh.Rendering;
        }

        int _cv;
        /// <summary>
        /// Буфер внесён
        /// </summary>
        public void SetBuffer(VertexBuffer vertexBuffer)
        {
            _cv = vertexBuffer.GetCountVertices();

            if (_cv > 0)
            {
                _bufferFloat.Set(vertexBuffer.BufferFloat);
                _bufferByte.Set(vertexBuffer.BufferByte);
            }
            else
            {
                _bufferFloat.Clear();
                _bufferByte.Clear();
            }
                //_countPoligon = bufferByte.Count / 24;
            Status = StatusMesh.Binding;
        }

        /// <summary>
        /// Занести буфер в OpenGL 
        /// </summary>
        public void BindBuffer()
        {
            if (!_bufferFloat.Empty)//bufferData.body && countPoligon > 0)
            {
                _countVertices = _cv;
                Reload();
                _bufferFloat.Clear();
                _bufferByte.Clear();
                //BindBufferReload();
                //bufferData.Free();
                Status = StatusMesh.Wait;
            }
            else
            {
                Status = StatusMesh.Null;
            }
        }

        /// <summary>
        /// Статус обработки сетки
        /// </summary>
        public enum StatusMesh
        {
            /// <summary>
            /// Пустой
            /// </summary>
            Null,
            /// <summary>
            /// Ждём
            /// </summary>
            Wait,
            /// <summary>
            /// Процесс рендеринга
            /// </summary>
            Rendering,
            /// <summary>
            /// Процесс связывания сетки с OpenGL
            /// </summary>
            Binding
        }
    }
}
