using WinGL.Util;

namespace Vge.Entity.Particle
{
    /// <summary>
    /// Сущность частички, цветной кубик с гравитацией
    /// </summary>
    public class EntityPartColorFX : EntityFX
    {
        /// <summary>
        /// Цвет в битах хRBG
        /// (0x1000000) x = 1 цвет рандом
        /// </summary>
        private int _color;

        public EntityPartColorFX() : base(EnumParticleDraw.Cube)
            => _gravity = .25f;

        /// <summary>
        /// Инизциализация размера жизни и прочего, индивидуально для типа сущности частички
        /// </summary>
        protected override void _Init(int parameter)
        {
            base._Init(parameter);
            _color = parameter;
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
            float r = (_rand.NextFloat() + _rand.NextFloat() + 1f) * .036f;
            motion = motion / Glm.Distance(motion) * r;
            motion.Y += .1f;
            motion *= .5f;
            Physics.MotionX = motion.X;
            Physics.MotionY = motion.Y;
            Physics.MotionZ = motion.Z;

            Scale = .25f + _rand.NextFloat() * .125f;

            if (_color >> 24 == 0)
            {
                Color = new Vector4(
                    (_color >> 16) / 255f,
                    ((_color >> 8) & 255) / 255f,
                    (_color & 255) / 255f,
                    1);
            }
            else
            {
                // Тут мы не должны появится!
                Color = new Vector4(_rand.NextFloat(), _rand.NextFloat(), _rand.NextFloat(), 1);
            }
        }
    }
}
