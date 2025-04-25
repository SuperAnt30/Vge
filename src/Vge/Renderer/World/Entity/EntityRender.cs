using System;
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

        public readonly uint Texture;

        public EntityRender(GL gl, uint texture)
        {
            Texture = texture;

            _mesh = new MeshEntity(gl);
            // TODO::SceletAnim #1 Тут ставим кубы в ноль, вращения
            _mesh.Reload(Ce.ModelEntities.ModelEntitiesObjects[0].Buffer);
        }

        /// <summary>
        /// Метод для прорисовки
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public void Draw(float timeIndex, EntityBase entity)
        {
            _mesh.Draw();
        }

        public void Dispose() => _mesh.Dispose();
    }
}
