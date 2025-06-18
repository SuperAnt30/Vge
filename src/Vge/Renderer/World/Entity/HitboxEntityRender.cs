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

        /// <summary>
        /// Буфер сетки, 12 линий, на линию 14 float 
        /// </summary>
        private float[] _buffer = new float[168];
        /// <summary>
        /// Буфер сетки живой сущности, 13 линий
        /// </summary>
        private float[] _bufferLiving = new float[182];

        public HitboxEntityRender(GL gl)
        {
            _mesh = new MeshLine(gl, GL.GL_STREAM_DRAW);
        }

        /// <summary>
        /// Метод для прорисовки
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public void Draw(float timeIndex, EntityBase entity)
        {
            float width = entity.Size.GetWidth();
            float height = entity.Size.GetHeight();
            float gb = entity.IsPhysicSleepDebug() ? 0 : 1;
           // float gb = entity.LevelMotionChange == 0 ? 0 : 1;
            
            List<float> list = new List<float>(MeshLine.CubeLine(-width, 0, -width, width, height, width, 1, gb, gb, 1));

            if (entity is EntityLiving entityLiving)
            {
                Vector3 raycust = Glm.Ray(entityLiving.GetRotationFrameYaw(timeIndex), 
                    entityLiving.GetRotationFramePitch(timeIndex)) * 1.6f;
                list.AddRange(new float[] {
                0, entityLiving.Eye, 0, 1, .5f, .5f, 1,
                raycust.X, raycust.Y + entityLiving.Eye, raycust.Z, 1, .5f, .5f, 1
            });
            }
            _mesh.Reload(list.ToArray());
            _mesh.Draw();
        }

        public void Dispose() => _mesh.Dispose();
    }
}
