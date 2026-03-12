using Vge.Renderer.World;

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

        public EntityRenderParticle(EntityBase entity, ParticlesRenderer particles)
            : base(entity) => _particles = particles;

        /// <summary>
        /// Метод для прорисовки
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="deltaTime">Дельта последнего кадра в mc</param>
        public override void Draw(float timeIndex, float deltaTime)
        {
            if (_entityRender == null)
            {
                _entityRender = _particles.GetParticleRender(Entity.IndexEntity);
            }

            // Заносим в шейдор
            _particles.Render.ShLine.SetUniform3("pos",
                Entity.GetPosFrameX(timeIndex) - _particles.Player.PosFrameX,
                Entity.GetPosFrameY(timeIndex) - _particles.Player.PosFrameY,
                Entity.GetPosFrameZ(timeIndex) - _particles.Player.PosFrameZ);

            // Рисуем основную сетку сущности
            _entityRender.MeshDraw();
        }
    }
}
