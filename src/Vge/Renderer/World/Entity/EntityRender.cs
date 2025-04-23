using System;
using System.Collections.Generic;
using Vge.Entity;
using Vge.Json;
using Vge.Util;
using Vge.World.Block;
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
            //List<float> list2 = new List<float>(Cube(-.3f, 0, -.3f, .3f, 1.8f, .3f, 0, 0, .015625f, .015625f, 1, 1, 1, 1, 0));
            //list2.AddRange(Cube(-.5f, 0f, -.5f, .5f, .4f, .5f, 0, 0, .015625f, .015625f, 1, 1, 0, 1, 1));

            JsonRead jsonRead = new JsonRead(Options.PathBlocks + "Chicken.bbmodel");

            //jsonRead.Compound.GetString()
            List<float> list2 = new List<float>();
            Cube(list2, -5, 0, -5, 5, 16, 5, 0, 1, 1, 1, 0);
            Cube(list2, -8, -2, -16, 8, 0, 0, 0, 1, 1, 0, 1);

            //list2.AddRange(Cube(-.5f, -.1f, -1f, .5f, 0, 0, 0, 0, .015625f, .015625f, 1, 1, 0, 1, 1));
            _mesh.Reload(list2.ToArray());

            //List<float> list2 = new List<float>(Cube(-.3f, 0, -.3f, .3f, 1f, .3f, 0, 0, .015625f, .015625f, 1, 1, 1, 1, 0));
            //list2.AddRange(Cube(-.5f, -.1f, -1f, .5f, 0, 0, 0, 0, .015625f, .015625f, 1, 1, 0, 1, 1));
            //_mesh.Reload(list2.ToArray());
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
        /// Прямоугольный параллелепипед
        /// </summary>
        public static void Cube(List<float> list, int x1, int y1, int z1,
            int x2, int y2, int z2,
            int numberTexture, float r, float g, float b, float id)
        {
            QuadSide quadSide;
            for (int i = 0; i < 6; i++)
            {
                quadSide = new QuadSide();
                quadSide.SetSide((Pole)i, true, x1, y1, z1, x2, y2, z2);
                quadSide.SetTexture(numberTexture);

                for (int j = 0; j < 4; j++)
                {
                    list.AddRange(new float[] {
                        quadSide.Vertex[j].X, quadSide.Vertex[j].Y, quadSide.Vertex[j].Z,
                        quadSide.Vertex[j].U, quadSide.Vertex[j].V,
                        r, g, b, 1, id
                    });
                }
            }
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
