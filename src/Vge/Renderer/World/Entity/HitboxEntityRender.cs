using System;
using System.Runtime.CompilerServices;
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
            _bufferLiving[178] = _bufferLiving[171] = 1;
            _bufferLiving[179] = _bufferLiving[172] = .5f;
            _bufferLiving[180] = _bufferLiving[173] = .5f;
            _bufferLiving[181] = _bufferLiving[174] = 1;
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

            if (entity is EntityLiving entityLiving)
            {
                _CubeLine(_bufferLiving, -width, 0, -width, width, height, width, 1, gb, gb, 1);
                Vector3 raycust = Glm.Ray(entityLiving.GetRotationFrameYaw(timeIndex), 
                    entityLiving.GetRotationFramePitch(timeIndex)) * 1.6f;
                _bufferLiving[169] = entityLiving.SizeLiving.GetEye();
                _bufferLiving[175] = raycust.X;
                _bufferLiving[176] = raycust.Y + entityLiving.SizeLiving.GetEye();
                _bufferLiving[177] = raycust.Z;
                _mesh.Reload(_bufferLiving);
            }
            else
            {
                _CubeLine(_buffer, -width, 0, -width, width, height, width, 1, gb, gb, 1);
                _mesh.Reload(_buffer);
            }
            _mesh.Draw();
        }

        /// <summary>
        /// Прямоугольный параллелепипед из линий
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _CubeLine(float[] buffer, float x1, float y1, float z1,
            float x2, float y2, float z2, float r, float g, float b, float a)
        {
            float[] items = MeshLine.CubeLine(x1, y1, z1, x2, y2, z2, r, g, b, a);
            Array.Copy(items, 0, buffer, 0, items.Length);
        }

        public void Dispose() => _mesh.Dispose();
    }
}
