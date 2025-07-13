using System;
using System.Runtime.CompilerServices;
using Vge.Entity;
using WinGL.OpenGL;

namespace Vge.Renderer.World.Entity
{
    /// <summary>
    /// Объект рисует сущности
    /// </summary>
    public class EntityRender : IDisposable
    {
        /// <summary>
        /// Сетка сущности
        /// </summary>
        private readonly MeshEntity _mesh;

        public EntityRender(GL gl, ResourcesEntity resourcesEntity)
        {
            _mesh = new MeshEntity(gl);
            _mesh.Reload(resourcesEntity.BufferMesh);
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
