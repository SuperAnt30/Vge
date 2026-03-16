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
        /// Сетка частички
        /// </summary>
        private readonly Mesh _mesh;

        public ParticleRender(GL gl, bool texture)
        {
            if (texture)
            {
                _mesh = new MeshParticleTexture(gl);
            }
            else
            {
                _mesh = new MeshParticle(gl);
            }
        }

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
