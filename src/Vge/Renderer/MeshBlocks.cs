using System;
using Vge.Renderer.World;
using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект сетки для воксельного мира с цветом альфа без текстуры
    /// </summary>
    public class MeshBlocks : Mesh
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

        public MeshBlocks(GL gl) : base(gl) { }

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
            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vbo);

            // layout(location = 0) in vec3 v_position;
            _gl.VertexAttribPointer(0, 3, GL.GL_FLOAT, false, 20, new IntPtr(0 * sizeof(float)));
            _gl.EnableVertexAttribArray(0);

            // layout(location = 1) in vec2 v_texCoord;
            _gl.VertexAttribPointer(1, 2, GL.GL_FLOAT, false, 20, new IntPtr(3 * sizeof(float)));
            _gl.EnableVertexAttribArray(1);

            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vboByte);

            // layout(location = 2) in int v_rgbl;
            _gl.VertexAttribPointer(2, 1, GL.GL_FLOAT, false, 12, new IntPtr(0 * sizeof(int)));
            _gl.EnableVertexAttribArray(2);

            // layout(location = 3) in int v_anim;
            _gl.VertexAttribPointer(3, 1, GL.GL_FLOAT, false, 12, new IntPtr(1 * sizeof(int)));
            _gl.EnableVertexAttribArray(3);

            // layout(location = 4) in int v_normal;
            _gl.VertexAttribPointer(4, 1, GL.GL_FLOAT, false, 12, new IntPtr(2 * sizeof(int)));
            _gl.EnableVertexAttribArray(4);
        }

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
        public void SetBuffer(VertexBlockBuffer vertexBuffer)
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
            Status = StatusMesh.Binding;
        }

        /// <summary>
        /// Занести буфер в OpenGL 
        /// </summary>
        public void BindBuffer()
        {
            if (!_bufferFloat.Empty)
            {
                _countVertices = _cv;
                Reload();
                _bufferFloat.Clear();
                _bufferByte.Clear();
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
