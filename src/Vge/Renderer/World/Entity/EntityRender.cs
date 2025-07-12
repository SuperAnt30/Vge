using System;
using Vge.Entity;
using Vge.Entity.Player;
using Vge.Entity.Render;
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

        // TODO::2025-07-12 Надо продумать про сетку слоёв!
        private readonly MeshEntity _mesh2;

        public EntityRender(GL gl, RenderMain render, ResourcesEntity modelEntity, PlayerClientOwner player)
        {
            Render = render;
            Player = player;

            _mesh = new MeshEntity(gl);
            _mesh2 = new MeshEntity(gl);
            _mesh.Reload(modelEntity.BufferMesh);
        }

        public void Reload2(VertexEntityBuffer buffer) => _mesh2.Reload(buffer);
        public void Mesh2Draw() => _mesh2.Draw();
        public void MeshDraw() => _mesh.Draw();

        public void Dispose()
        {
            _mesh.Dispose();
            _mesh2.Dispose();
        }
    }
}
