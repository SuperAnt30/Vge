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

        public ParticleRender(GL gl, bool texture, float[] buffer)
        {
            if (texture)
            {
                _mesh = new MeshParticleTexture(gl, buffer);
            }
            else
            {
                _mesh = new MeshParticle(gl, buffer);
            }
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
