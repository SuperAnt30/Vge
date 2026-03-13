using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Render;
using WinGL.OpenGL;

namespace Vge.Renderer.World.Entity
{
    /// <summary>
    /// Объект рисует объёмную частичку
    /// </summary>
    public class ParticleRender : IEntityRender, IDisposable
    {
        /// <summary>
        /// Сетка частички покуда линии
        /// </summary>
        private readonly MeshParticle _mesh;

        public ParticleRender(GL gl) => _mesh = new MeshParticle(gl);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reload(float[] buffer) => _mesh.Reload(buffer);

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
