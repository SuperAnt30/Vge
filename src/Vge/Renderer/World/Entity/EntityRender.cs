using System;
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
        /// Сетка сущности
        /// </summary>
        private readonly MeshEntity _mesh;

        public readonly uint Texture;

        /// <summary>
        /// Объект отвечающий за прорисовку
        /// </summary>
        public readonly RenderMain Render;
        /// <summary>
        /// Объект игрока для клиента
        /// </summary>
        public readonly PlayerClientOwner Player;

        public EntityRender(GL gl, RenderMain render, PlayerClientOwner player)
        {
            Render = render;
            Player = player;

            Texture = Render.SetTexture(Ce.ModelEntities.ModelEntitiesObjects[0].Textures[1]);

            _mesh = new MeshEntity(gl);
            // TODO::SceletAnim #1 Тут ставим кубы в ноль, вращения
            _mesh.Reload(Ce.ModelEntities.ModelEntitiesObjects[0].BufferMesh);
        }

        public void MeshDraw() => _mesh.Draw();

        public void Dispose() => _mesh.Dispose();
    }
}
