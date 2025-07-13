using System;
using Vge.Entity.Render;
using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Объект сетки для сущностей с текстурой
    /// </summary>
    public class MeshEntity : Mesh
    {
        /// <summary>
        /// Дополнительный буфер для int данных
        /// </summary>
        private uint _vboInt;

        public MeshEntity(GL gl) : base(gl) { }

        protected override void _InitVbo()
        {
            uint[] id = new uint[2];
            _gl.GenBuffers(2, id);
            _vbo = id[0];
            _vboInt = id[1];
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

            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vboInt);

            // layout(location = 2) in int v_jointId;
            _gl.VertexAttribPointer(2, 1, GL.GL_FLOAT, false, 8, new IntPtr(0 * sizeof(int)));
            _gl.EnableVertexAttribArray(2);

            // layout(location = 3) in int v_clothId;
            _gl.VertexAttribPointer(3, 1, GL.GL_FLOAT, false, 8, new IntPtr(1 * sizeof(int)));
            _gl.EnableVertexAttribArray(3);
        }


        public override void Delete()
        {
            _gl.DeleteVertexArrays(1, new uint[] { _vao });
            _gl.DeleteBuffers(2, new uint[] { _vbo, _vboInt });
            _gl.DeleteBuffers(1, new uint[] { _ebo });
        }

        /// <summary>
        /// Перезаписать полигоны
        /// </summary>
        public void Reload(VertexEntityBuffer buffers)
        {
            _countVertices = buffers.GetCountVertices();

            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vbo);
            _gl.BufferData(GL.GL_ARRAY_BUFFER, buffers.BufferFloat, GL.GL_STATIC_DRAW);
            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vboInt);
            _gl.BufferData(GL.GL_ARRAY_BUFFER, buffers.BufferInt, GL.GL_STATIC_DRAW);
            _gl.BindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, _ebo);
            _gl.BufferData(GL.GL_ELEMENT_ARRAY_BUFFER, _QuadIndices(), GL.GL_STATIC_DRAW);
        }

        /// <summary>
        /// Перезаписать полигоны слоёв
        /// </summary>
        public void ReloadLayers(VertexLayersBuffer buffers)
        {
            _countVertices = buffers.CountVertices;

            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vbo);
            _gl.BufferData(GL.GL_ARRAY_BUFFER, buffers.ToArrayFloat(), GL.GL_DYNAMIC_DRAW);
            _gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vboInt);
            _gl.BufferData(GL.GL_ARRAY_BUFFER, buffers.ToArrayInt(), GL.GL_DYNAMIC_DRAW);
            _gl.BindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, _ebo);
            _gl.BufferData(GL.GL_ELEMENT_ARRAY_BUFFER, _QuadIndices(), GL.GL_DYNAMIC_DRAW);
        }
    }
}
