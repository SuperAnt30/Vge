using Vge.Entity.Particle;
using WinGL.Util;

namespace Mvk2.Entity.Particle
{
    /// <summary>
    /// Сущность частички, пламя
    /// </summary>
    public class EntityFlameFX : EntityFX
    {
        /// <summary>
        /// Начальный масштаб частицы
        /// </summary>
        private float _flameScale;

        public EntityFlameFX() : base(EnumParticleDraw.Sprite)
            => _gravity = 0;

        /// <summary>
        /// Инизциализация размера жизни и прочего, индивидуально для типа сущности частички
        /// </summary>
        protected override void _Init(int parameter)
        {
            base._Init(parameter);
            _maxAge = 20 + _rand.Next(60);
            _noClip = true;
        }

        /// <summary>
        /// Запуск сущности после всех инициализаций, как правило только на сервере
        /// </summary>
        public override void InitRun(Vector3 pos, Vector3 motion)
        {
            PosPrevX = PosX = pos.X;
            PosPrevY = PosY = pos.Y;
            PosPrevZ = PosZ = pos.Z;

            // 16 pix
            //_flameScale = Scale = 1.2f + _rand.NextFloat() * .6f; // Для дыма спрайта

            // 8 pix
            _flameScale = Scale = 0.8f + _rand.NextFloat() * .4f; // Для дыма спрайта

            //Vector3 c = new Vector3(1, _rand.NextFloat(), _rand.NextFloat() * .5f).Normalize();
            //c = c * (_rand.NextFloat() * .5f + .5f);
            //Color = new Vector4(1, c.Y, c.Z, 1);

            Color = new Vector4(1);

            //float c = .5f + _rand.NextFloat() * .5f;
            //Color = new Vector4(1, c, c, 1);
            //Uv = new Vector4(0.4375f, 0, .5f, 0.0625f);
            //Uv = new Vector4(0, .0625f, .0625f, .125f);

            //Uv = new Vector4(0, .03125f, .03125f, .0625f);

            // 16 pix
            //Uv = new Vector4(0, .0625f, .0625f, .125f);

            // 8 pix
            Uv = new Vector4(0, .03125f, .03125f, .0625f);
        }

        //protected override void _Update()
        //{
        //    Scale = _flameScale * (1 + (Glm.Cos(_age * .2f) + 2f) * .16f);// .32f);

        //    float v = _age * 16 / _maxAge * .03125f;
        //    Uv = new Vector4(v, .03125f, v + .03125f, .0625f);

        //    //int i = 8 * _age  / _maxAge;
        //    //float v = _age * 16 / _maxAge * .0625f;
        //    //Uv = new Vector4(v, .0625f, v + .0625f, .125f);
        //}

        /// <summary>
        /// Игровой такт на сервере
        /// </summary>
        public override void Update()
        {
            if (_age++ > _maxAge)
            {
                SetDead();
            }
            else
            {
                Scale = _flameScale * (1 + (Glm.Cos(_age * .2f) + 2f) * .16f);
                float v = _age * 16 / _maxAge * .03125f;
                Uv = new Vector4(v, .03125f, v + .03125f, .0625f);
            }
        }
    }
}
