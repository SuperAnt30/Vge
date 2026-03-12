using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Render;
using WinGL.OpenGL;

namespace Vge.Renderer.World.Entity
{
    /// <summary>
    /// Объект рисует частичку
    /// </summary>
    public class ParticleRender : IEntityRender, IDisposable
    {
        /// <summary>
        /// Сетка частички покуда линии
        /// </summary>
        private readonly MeshLine _mesh;

        public ParticleRender(GL gl)
        {
            _mesh = new MeshLine(gl, GL.GL_STATIC_DRAW);
            float width = 0.0625f;
            float height = 0.125f;
            float[] items = MeshLine.CubeLine(-width, 0, -width, width, height, width, 1, 1, 1, 1);
            float[] buffer = new float[168];
            Array.Copy(items, 0, buffer, 0, items.Length);
            _mesh.Reload(buffer);
            //_mesh = new MeshLine(_worldRenderer.GetOpenGL(), GL.GL_DYNAMIC_DRAW);
        }

        /// <summary>
        /// Прорисовать сетку
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MeshDraw() => _mesh.Draw();

        /// <summary>
        /// Выгрузить сетку с OpenGL
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _mesh.Dispose();
    }
}
