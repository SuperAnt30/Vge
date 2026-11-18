using System;
using WinGL.OpenGL;

namespace Vge.Renderer.World.Entity
{
    /// <summary>
    /// Инстансинг 2d частиц: квад, повёрнутый к камере, с текстурой или без.
    /// Шейдер EntityParticle2d. Экземпляр: 16 float
    /// [0..3]  позиция относительно камеры xyz, масштаб
    /// [4..7]  цвет rgba
    /// [8..11] uv в атласе x1, y1, x2, y2
    /// [12..15] свет блока, свет неба, param, резерв
    /// </summary>
    public class ParticleInstanced2d : ParticleInstancedBase
    {
        public ParticleInstanced2d(GL gl, int capacity) : base(gl, capacity, 16, 2)
        {
            // Квад: x, y, u, v, порядок для GL_TRIANGLE_STRIP (BL, BR, TL, TR)
            float[] quad = new float[]{
               -.125f, -.125f, 0, 1,
                .125f, -.125f, 1, 1,
               -.125f,  .125f, 0, 0,
                .125f,  .125f, 1, 0
            };

            gl.BindVertexArray(_vao);
            gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vboMesh);
            gl.BufferData(GL.GL_ARRAY_BUFFER, quad, GL.GL_STATIC_DRAW);
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 2, GL.GL_FLOAT, false, 4 * sizeof(float), IntPtr.Zero);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 2, GL.GL_FLOAT, false, 4 * sizeof(float), new IntPtr(2 * sizeof(float)));
            gl.BindVertexArray(0);
        }

        protected override void _DrawInstanced(int count)
            => gl.DrawArraysInstanced(GL.GL_TRIANGLE_STRIP, 0, 4, count);
    }
}
