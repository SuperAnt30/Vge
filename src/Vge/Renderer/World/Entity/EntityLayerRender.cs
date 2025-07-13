using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Render;

namespace Vge.Renderer.World.Entity
{
    /// <summary>
    /// Объект рисует слои конкретной сущности, если такие имеются
    /// </summary>
    public class EntityLayerRender : IDisposable
    {
        /// <summary>
        /// Сетка сущности
        /// </summary>
        private readonly MeshEntity _mesh;
        /// <summary>
        /// Объект рендера всех сущностей
        /// </summary>
        private readonly EntitiesRenderer _entities;
        
        public EntityLayerRender(EntitiesRenderer entities)
        {
            _entities = entities;
            _mesh = new MeshEntity(entities.GetOpenGL());
        }

        /// <summary>
        /// Пополнить сетку полигонами
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRangeBuffer(VertexEntityBuffer buffer)
            => _entities.Buffer.AddRange(buffer);

        /// <summary>
        /// Перезаписать полигоны
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reload()
        {
            _mesh.ReloadLayers(_entities.Buffer);
            _entities.Buffer.Clear();
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
