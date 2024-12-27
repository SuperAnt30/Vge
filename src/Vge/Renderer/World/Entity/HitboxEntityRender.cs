using System;
using System.Collections.Generic;
using Vge.Entity;
using WinGL.OpenGL;
using WinGL.Util;

namespace Vge.Renderer.World.Entity
{
    /// <summary>
    /// Объект рисует римку сущности по его рамеру
    /// </summary>
    public class HitboxEntityRender : IDisposable
    {
        /// <summary>
        /// Сетка курсора
        /// </summary>
        private readonly MeshLine _mesh;

        private readonly EntityBase _entity;

        public HitboxEntityRender(GL gl, EntityBase entity)
        {
            _mesh = new MeshLine(gl);
            _entity = entity;
        }

        /// <summary>
        /// Метод для прорисовки
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public void Draw(float timeIndex)
        {
            float width = _entity.Width;
            float height = _entity.Height;
            Vector3 raycust = Glm.Ray(_entity.GetRotationFrameYaw(timeIndex), _entity.GetRotationFramePitch(timeIndex)) * 5;

            List<float> list = new List<float>(MeshLine.CubeLine(-width, 0, -width, width, height, width, 1, 1, 1, 1));
            list.AddRange(new float[] {
                0, _entity.Eye, 0, 1, .5f, .5f, 1,
                raycust.X, raycust.Y + _entity.Eye, raycust.Z, 1, .5f, .5f, 1
            });
            _mesh.Reload(list.ToArray());
            _mesh.Draw();
        }

        public void Dispose() => _mesh.Dispose();
    }
}
