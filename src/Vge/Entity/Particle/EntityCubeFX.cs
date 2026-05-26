using WinGL.Util;

namespace Vge.Entity.Particle
{
    /// <summary>
    /// Сущность частички куб без гравитации, в воздухе.
    /// Удобен для отладки навигации пути или иных деталей.
    /// </summary>
    public class EntityCubeFX : EntityFX
    {
        private int _parameter;
        private Vector4 _color;

        public EntityCubeFX() : base(EnumParticleDraw.Cube)
            => _gravity = 0;

        /// <summary>
        /// Инизциализация размера жизни и прочего, индивидуально для типа сущности частички
        /// </summary>
        /// <param name="parameter">2 параметра AAASSC = Age (0 - 4095), Scale (0 - 255), Color (0 - 15)</param>
        protected override void _Init(int parameter)
        {
            _maxAge = parameter >> 12;
            if (_maxAge == 0) _maxAge = 100;
            _parameter = parameter & 0xFFF;
        }

        /// <summary>
        /// Запуск сущности после всех инициализаций, как правило только на сервере
        /// </summary>
        public override void InitRun(Vector3 pos, Vector3 motion)
        {
            PosPrevX = PosX = pos.X;
            PosPrevY = PosY = pos.Y;
            PosPrevZ = PosZ = pos.Z;

            int scale = _parameter >> 4;
            if (scale == 0) scale = 4;
            Scale = scale * .25f;

            int color = _parameter & 15;
            _color = Color = new Vector4(Gi.ColorRed[color], 
                Gi.ColorGreen[color], Gi.ColorBlue[color], 1);
        }

        protected override void _Update()
        {
            float c = Glm.Cos(_age * .15708f) * .06125f;
            float colorX = _color.X + c;
            float colorY = _color.Y + c;
            float colorZ = _color.Z + c;
            if (colorX < 0) colorX = 0;
            if (colorY < 0) colorY = 0;
            if (colorZ < 0) colorZ = 0;
            if (colorX > 1) colorX = 1;
            if (colorY > 1) colorY = 1;
            if (colorZ > 1) colorZ = 1;
            Color = new Vector4(colorX, colorY, colorZ, 1);
        }
    }
}
