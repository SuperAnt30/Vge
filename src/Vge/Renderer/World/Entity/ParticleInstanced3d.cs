using System;
using WinGL.OpenGL;

namespace Vge.Renderer.World.Entity
{
    /// <summary>
    /// Инстансинг 3d частиц: цветной кубик без текстуры, с затемнением сторон.
    /// Шейдер EntityParticle3d. Экземпляр: 12 float
    /// [0..3]  позиция относительно камеры xyz, масштаб
    /// [4..7]  цвет rgba
    /// [8..11] свет блока, свет неба, param, резерв
    /// </summary>
    public class ParticleInstanced3d : ParticleInstancedBase
    {
        /// <summary>
        /// Индексов на кубик: 6 сторон × 2 треугольника × 3
        /// </summary>
        private const int _countIndex = 36;

        private readonly uint _ebo;

        /// <summary>
        /// Кубик: X и Z от w1 до w2, Y от h1 до h2 (те же значения, что были в _Cube)
        /// </summary>
        public ParticleInstanced3d(GL gl, int capacity) : base(gl, capacity, 12, 2)
        {
            uint[] id = new uint[1];
            gl.GenBuffers(1, id); _ebo = id[0];

            gl.BindVertexArray(_vao);

            // x, y, z, затемнение стороны
            gl.BindBuffer(GL.GL_ARRAY_BUFFER, _vboMesh);
            gl.BufferData(GL.GL_ARRAY_BUFFER, _Cube(-.125f, 0, .125f, .25f), GL.GL_STATIC_DRAW);
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, GL.GL_FLOAT, false, 4 * sizeof(float), IntPtr.Zero);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 1, GL.GL_FLOAT, false, 4 * sizeof(float), new IntPtr(3 * sizeof(float)));

            // Индексы запоминаются в VAO
            gl.BindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, _ebo);
            gl.BufferData(GL.GL_ELEMENT_ARRAY_BUFFER, _CubeIndices(), GL.GL_STATIC_DRAW);

            gl.BindVertexArray(0);
        }

        protected override void _DrawInstanced(int count)
            => gl.DrawElementsInstanced(GL.GL_TRIANGLES, _countIndex, GL.GL_UNSIGNED_INT, IntPtr.Zero, count);

        public override void Dispose()
        {
            gl.DeleteBuffers(1, new uint[] { _ebo });
            base.Dispose();
        }

        /// <summary>
        /// Кубик: x, y, z, затемнение. 6 сторон по 4 вершины
        /// </summary>
        private static float[] _Cube(float w1, float h1, float w2, float h2)
        {
            float s1 = Gi.DarkeningSideB;
            float s2 = Gi.DarkeningSideA;
            float s3 = Gi.DarkeningSideDown;

            return new float[]
            {
                // Верх
                w1, h2, w2, 1,
                w2, h2, w2, 1,
                w2, h2, w1, 1,
                w1, h2, w1, 1,
                // Низ
                w1, h1, w1, s3,
                w2, h1, w1, s3,
                w2, h1, w2, s3,
                w1, h1, w2, s3,
                // Юг
                w1, h1, w2, s1,
                w2, h1, w2, s1,
                w2, h2, w2, s1,
                w1, h2, w2, s1,
                // Север
                w2, h1, w1, s1,
                w1, h1, w1, s1,
                w1, h2, w1, s1,
                w2, h2, w1, s1,
                // Восток
                w2, h1, w2, s2,
                w2, h1, w1, s2,
                w2, h2, w1, s2,
                w2, h2, w2, s2,
                // Запад
                w1, h1, w1, s2,
                w1, h1, w2, s2,
                w1, h2, w2, s2,
                w1, h2, w1, s2
            };
        }

        /// <summary>
        /// Индексы кубика: каждая сторона из 4 вершин — два треугольника (0,1,2) и (0,2,3)
        /// </summary>
        private static int[] _CubeIndices()
        {
            int[] indices = new int[_countIndex];
            for (int side = 0; side < 6; side++)
            {
                int v = side * 4;
                int i = side * 6;
                indices[i] = v;
                indices[i + 1] = v + 1;
                indices[i + 2] = v + 2;
                indices[i + 3] = v;
                indices[i + 4] = v + 2;
                indices[i + 5] = v + 3;
            }
            return indices;
        }
    }
}
