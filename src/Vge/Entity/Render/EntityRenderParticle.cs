using Vge.Entity.Particle;
using Vge.Renderer.World;
using WinGL.OpenGL;

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
        private readonly EntityParticle _entityParticle;
        /// <summary>
        /// Шейдер для частичек
        /// </summary>
        private readonly ShaderProgram _shader;

        public EntityRenderParticle(EntityParticle entityParticle, ParticlesRenderer particles)
            : base(entityParticle)
        {
            _entityParticle = entityParticle;
            _particles = particles;
            _shader = _particles.Render.ShsEntity.GetShaderParticle(entityParticle);
        }

        private void _Uniform(float timeIndex)
        {
            _shader.SetUniform3("pos",
                _entityParticle.GetPosFrameX(timeIndex) - _particles.Player.PosFrameX,
                _entityParticle.GetPosFrameY(timeIndex) - _particles.Player.PosFrameY,
                _entityParticle.GetPosFrameZ(timeIndex) - _particles.Player.PosFrameZ);
            _shader.SetUniform4("color", 
                _entityParticle.Color.X, _entityParticle.Color.Y,
                _entityParticle.Color.Z, _entityParticle.Color.W);
            _shader.SetUniform2("light", _lightBlock, _lightSky);
            _shader.SetUniform1("scale", _entityParticle.Scale);
            if (!_entityParticle.IsCube)
            {
                _shader.SetUniform1("param", _entityParticle.IsSprite ? 1 : 0);
            }
        }

        /// <summary>
        /// Метод для прорисовки
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="deltaTime">Дельта последнего кадра в mc</param>
        public override void Draw(float timeIndex, float deltaTime)
        {
            if (_entityRender == null)
            {
                _entityRender = _particles.GetParticleRender(_entityParticle.TypeDraw);
            }
            // шейдор
            _Uniform(timeIndex);
            // Рисуем основную сетку сущности
            _entityRender.MeshDraw();
        }

        public override void Dispose() => _entityRender?.Dispose();
    }
}
