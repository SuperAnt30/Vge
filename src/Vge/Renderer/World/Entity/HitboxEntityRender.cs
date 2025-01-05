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

        public HitboxEntityRender(GL gl)
        {
            _mesh = new MeshLine(gl);
        }

        /// <summary>
        /// Метод для прорисовки
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public void Draw(float timeIndex, EntityBase entity)
        {
            float width = entity.Width;
            float height = entity.Height;
            Vector3 raycust = Glm.Ray(entity.GetRotationFrameYaw(timeIndex), entity.GetRotationFramePitch(timeIndex)) * 1.6f;

            List<float> list = new List<float>(MeshLine.CubeLine(-width, 0, -width, width, height, width, 1, 1, 1, 1));
            list.AddRange(new float[] {
                0, entity.Eye, 0, 1, .5f, .5f, 1,
                raycust.X, raycust.Y + entity.Eye, raycust.Z, 1, .5f, .5f, 1
            });
            _mesh.Reload(list.ToArray());
            _mesh.Draw();
        }

        public void Dispose() => _mesh.Dispose();
    }
}
