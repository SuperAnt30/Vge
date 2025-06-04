using System;
using Vge.Entity;
using Vge.Management;
using WinGL.OpenGL;

namespace Vge.Renderer.World.Entity
{
    /// <summary>
    /// Объект рисует сущности
    /// </summary>
    public class EntityRender : IDisposable
    {
        /// <summary>
        /// Объект отвечающий за прорисовку
        /// </summary>
        public readonly RenderMain Render;
        /// <summary>
        /// Объект игрока для клиента
        /// </summary>
        public readonly PlayerClientOwner Player;
        /// <summary>
        /// Сетка сущности
        /// </summary>
        private readonly MeshEntity _mesh;

        public EntityRender(GL gl, RenderMain render, ModelEntity modelEntity, PlayerClientOwner player)
        {
            Render = render;
            Player = player;

            _mesh = new MeshEntity(gl);
            _mesh.Reload(modelEntity.BufferMesh);
        }

        public void MeshDraw() => _mesh.Draw();

        public void Dispose() => _mesh.Dispose(); 
    }
}
