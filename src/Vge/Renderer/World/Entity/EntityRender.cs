using System;
using System.Collections.Generic;
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

        public EntityRender(GL gl)
        {
            _mesh = new MeshEntity(gl);

            // TODO::SceletAnim #1 Тут ставим кубы в ноль, вращения
            List<float> list2 = new List<float>(Cube(-.3f, 0, -.3f, .3f, 1.8f, .3f, 0, 0, .015625f, .015625f, 1, 1, 1, 1, 0));
            list2.AddRange(Cube(-.5f, 0f, -.5f, .5f, .4f, .5f, 0, 0, .015625f, .015625f, 1, 1, 0, 1, 1));
            _mesh.Reload(list2.ToArray());
        }

        /// <summary>
        /// Метод для прорисовки
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public void Draw(float timeIndex, EntityBase entity)
        {
            _mesh.Draw();
        }

        /// <summary>
        /// Прямоугольный параллелепипед из линий
        /// </summary>
        public static float[] Cube(float x1, float y1, float z1, 
            float x2, float y2, float z2,
            float u1, float v1, float u2, float v2, float r, float g, float b, float a, float id)
        {
            return new float[]
            {
                // Квад низа
                x1, y1, z1, u1, v1, r, g, b, a, id,
                x2, y1, z1, u2, v1, r, g, b, a, id,
                x2, y1, z2, u2, v2, r, g, b, a, id,
                x1, y1, z2, u1, v2, r, g, b, a, id,

                // Квад вверха
                x1, y2, z1, u1, v1, r, g, b, a, id,
                x1, y2, z2, u1, v2, r, g, b, a, id,
                x2, y2, z2, u2, v2, r, g, b, a, id,
                x2, y2, z1, u2, v1, r, g, b, a, id,

            };
        }

        public void Dispose() => _mesh.Dispose();
    }
}
