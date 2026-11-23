using Vge.Entity.Particle;
using WinGL.Util;

namespace Mvk2.Entity.Particle
{
    /// <summary>
    /// Сущность частички, языки пламени с дымом
    /// </summary>
    public class EntityFlameSmokeFX : EntityFX
    {
        /// <summary>
        /// Начальный размер
        /// </summary>
        private float _sizeBegin;
        /// <summary>
        /// Начальный цвет
        /// </summary>
        private float _colorBegin;

        public EntityFlameSmokeFX() : base(EnumParticleDraw.Sprite)
            => _gravity = -.005f;

        /// <summary>
        /// Инизциализация размера жизни и прочего, индивидуально для типа сущности частички
        /// </summary>
        protected override void _Init(int parameter)
        {
            base._Init(parameter);
           _maxAge = 60 + _rand.Next(90);
        }

        /// <summary>
        /// Запуск сущности после всех инициализаций, как правило только на сервере
        /// </summary>
        public override void InitRun(Vector3 pos, Vector3 motion)
        {
            PosPrevX = PosX = pos.X;
            PosPrevY = PosY = pos.Y;
            PosPrevZ = PosZ = pos.Z;

            float y = motion.Y;

            motion.X += (_rand.Next(200) - 100) * .004f;
           // motion.Y += (_rand.Next(200) - 100) * .002f;
            motion.Z += (_rand.Next(200) - 100) * .004f;
            float r = (_rand.NextFloat() + _rand.NextFloat() + 1f) * .036f;
            motion = motion / Glm.Distance(motion) * r;
            //  motion.Y += .1f;
            motion.Y = Mth.Abs(y) + .02f;
            motion *= .2f;
            Physics.MotionX = motion.X;
            Physics.MotionY = motion.Y;
            Physics.MotionZ = motion.Z;

            _sizeBegin = Scale = (.125f + _rand.NextFloat() * .25f) * 4;
            _colorBegin = _rand.NextFloat() * 2f;
            if (_colorBegin > 1f) _colorBegin = 1f;

            Vector3 c = new Vector3(1, _rand.NextFloat(), _rand.NextFloat() * .5f).Normalize();
            //c = c * (_rand.NextFloat() * .5f + .5f);
            //Color = new Vector4(1, c.Y, c.Z, 1);

            Color = new Vector4(1, 1, 0, 1);
            Uv = new Vector4(0, 0, .03125f, .03125f);
        }


        protected override void _Update()
        {
            float max = _maxAge;

            float f = _age / max;

            float v = _age * 16 / _maxAge * .03125f ;
            Uv = new Vector4(v, 0, v + .03125f, .03125f);
            f = 1 - f;
            f *= f;
            f = 1 - f;

            if (f < .25f)
            {
                // 0 - 0.25
                // Смена с жёлтого на красный плюс увеличение размера
                //Scale = _s * (1 + f * 8f);
                Scale = _sizeBegin * (f * 16f);
                //Color = new Vector4(1, f * 4f, 0, 1);// 1 - f * 2f);
                Color = new Vector4(1, 1 - f * 1.5f, 0, 1);// 1 - f * 2f);
            }
            else if (f < .5f)
            {
                // 0.25 - 0.5
                // Смена с красного большого, на мелкий чёрный
                Scale = _sizeBegin * (10 * (.65f - f));
                f = (.5f - f) * 4f;
                f *= _colorBegin;
                Color = new Vector4(f, .5f * f, 0, 1);// .5f);
            }
            //else if (f < .75f)
            //{
            //    // 0.5 - 0.75
            //    f = (f - .5f) * 4f; // 1 => 0
            //    Color = new Vector4(f, f, f, 1);
            //}
            else
            {
                // 0.5 - 1.0
                // Цвет не меняем, только с чёрно на белый смена
                //f = (1 - f) * 4f;
                //Color = new Vector4(1, 1, 1, f);
                f = (f - .5f) * 2f;
                f *= _colorBegin;
                Color = new Vector4(f, f, f, 1);// .5f - f / 2f);
            }
        }
    }
}
