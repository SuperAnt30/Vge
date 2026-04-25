using WinGL.Util;

namespace Vge.Entity.Particle
{
    /// <summary>
    /// Сущность частички, капельки
    /// </summary>
    public class EntityDropletFX : EntityFX
    {
        public EntityDropletFX() : base(EnumParticleDraw.Quad)
            => _gravity = -.004f;

        /// <summary>
        /// Инизциализация размера жизни и прочего, индивидуально для типа сущности частички
        /// </summary>
        protected override void _Init(int parameter)
        {
            base._Init(parameter);
            _maxAge = 200 + _rand.Next(90);
        }

        /// <summary>
        /// Запуск сущности после всех инициализаций, как правило только на сервере
        /// </summary>
        public override void InitRun(Vector3 pos, Vector3 motion)
        {
            PosPrevX = PosX = pos.X;
            PosPrevY = PosY = pos.Y;
            PosPrevZ = PosZ = pos.Z;

            motion.X += (_rand.Next(200) - 100) * .004f;
            motion.Y += (_rand.Next(200) - 100) * .004f;
            motion.Z += (_rand.Next(200) - 100) * .004f;
            float r = (_rand.NextFloat() + _rand.NextFloat() + 1f) * .06f;
            motion = motion / Glm.Distance(motion) * r;
            motion.Y += 1f;

            
            //motion = motion * .1f + motion;
            //motion.Y += _ageAdd / 1000f;

            motion.X = motion.X * .1f;
            motion.Z = motion.Z * .1f;
            motion.Y = motion.Y * .025f;// + motion * .1f;

            Physics.MotionX = motion.X;
            Physics.MotionY = motion.Y;
            Physics.MotionZ = motion.Z;

            Scale = .25f + _rand.NextFloat() * .5f; // Для маленький квадов без текстуры
            float c = .5f + _rand.NextFloat() * .5f;
            Color = new Vector4(c, c, c, 1);
        }
    }
}
