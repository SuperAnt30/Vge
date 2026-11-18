using Vge.Entity.Particle;
using Vge.Renderer.World;
using Vge.Renderer.World.Entity;

namespace Vge.Entity.Render
{
    /// <summary>
    /// Объект рендера частичек (эффектов), рендера, только для клиента
    /// </summary>
    public class EntityRenderParticle : EntityRenderAbstract
    {
        /// <summary>
        /// Объект рендера всех частичек
        /// </summary>
        protected readonly ParticlesRenderer _particles;
        /// <summary>
        /// Объект сузности частички
        /// </summary>
        private readonly EntityFX _entityParticle;

        public EntityRenderParticle(EntityFX entityParticle, ParticlesRenderer particles)
            : base(entityParticle)
        {
            _entityParticle = entityParticle;
            _particles = particles;
        }

        public void WriteInstance(ParticleInstanced2d instanced, float timeIndex)
        {
            int o = instanced.Next();
            if (o < 0) return;
            float[] d = instanced.Data;

            // xyz — позиция относительно камеры, w — масштаб
            d[o] = _entityParticle.GetPosFrameX(timeIndex) - _particles.Player.PosFrameX;
            d[o + 1] = _entityParticle.GetPosFrameY(timeIndex) - _particles.Player.PosFrameY;
            d[o + 2] = _entityParticle.GetPosFrameZ(timeIndex) - _particles.Player.PosFrameZ;
            d[o + 3] = _entityParticle.Scale;
            // Цвет
            d[o + 4] = _entityParticle.Color.X;
            d[o + 5] = _entityParticle.Color.Y;
            d[o + 6] = _entityParticle.Color.Z;
            d[o + 7] = _entityParticle.Color.W;
            // UV в атласе (без спрайта не используется, шейдер смотрит на param)
            if (_entityParticle.IsSprite)
            {
                d[o + 8] = _entityParticle.Uv.X;
                d[o + 9] = _entityParticle.Uv.Y;
                d[o + 10] = _entityParticle.Uv.Z;
                d[o + 11] = _entityParticle.Uv.W;
            }
            // Свет и параметры
            d[o + 12] = _lightBlock;
            d[o + 13] = _lightSky;
            d[o + 14] = _entityParticle.Param;
            d[o + 15] = 0;
        }

        public void WriteInstance(ParticleInstanced3d instanced, float timeIndex)
        {
            int o = instanced.Next();
            if (o < 0) return;
            float[] d = instanced.Data;

            // xyz — позиция относительно камеры, w — масштаб
            d[o] = _entityParticle.GetPosFrameX(timeIndex) - _particles.Player.PosFrameX;
            d[o + 1] = _entityParticle.GetPosFrameY(timeIndex) - _particles.Player.PosFrameY;
            d[o + 2] = _entityParticle.GetPosFrameZ(timeIndex) - _particles.Player.PosFrameZ;
            d[o + 3] = _entityParticle.Scale;
            // Цвет
            d[o + 4] = _entityParticle.Color.X;
            d[o + 5] = _entityParticle.Color.Y;
            d[o + 6] = _entityParticle.Color.Z;
            d[o + 7] = _entityParticle.Color.W;
            // Свет и параметры
            d[o + 8] = _lightBlock;
            d[o + 9] = _lightSky;
            d[o + 10] = _entityParticle.Param;
            d[o + 11] = 0;
        }
    }
}
