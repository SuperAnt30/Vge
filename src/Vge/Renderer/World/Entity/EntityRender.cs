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
        /// <summary>
        /// Сетка Хитбокса
        /// </summary>
       // private readonly MeshLine _meshHitbox;
        /// <summary>
        /// Имеется ли луч для Hitbox
        /// </summary>
       // private readonly bool _ray;
        /// <summary>
        /// Буфер сетки, если луча нет
        /// </summary>
        //private readonly float[] _bufferHitbox;

        public EntityRender(GL gl, RenderMain render, ModelEntity modelEntity, PlayerClientOwner player)
        {
            Render = render;
            Player = player;
            // _ray = ray;

            _mesh = new MeshEntity(gl);
            _mesh.Reload(modelEntity.BufferMesh);
            //_meshHitbox = new MeshLine(gl, GL.GL_DYNAMIC_DRAW);
        }

        public void MeshDraw() => _mesh.Draw();

        public void Dispose()
        {
            _mesh.Dispose();
            //_meshHitbox.Dispose();
        }
    }
}
