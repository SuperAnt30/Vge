using WinGL.Util;

namespace Vge.Entity.Particle
{
    /// <summary>
    /// Сущность частички для отладки навигации пути или иных деталей, без гравитации, в воздухе кубик
    /// </summary>
    public class EntityDebugFX : EntityFX
    {
        private int _parameter;

        public EntityDebugFX() : base(EnumParticleDraw.Quad)
            => _gravity = 0;

        /// <summary>
        /// Инизциализация размера жизни и прочего, индивидуально для типа сущности частички
        /// </summary>
        protected override void _Init(int parameter)
        {
            base._Init(parameter);
            _maxAge = 200;
            _parameter = parameter;
        }

        /// <summary>
        /// Запуск сущности после всех инициализаций, как правило только на сервере
        /// </summary>
        public override void InitRun(Vector3 pos, Vector3 motion)
        {
            PosPrevX = PosX = pos.X;
            PosPrevY = PosY = pos.Y;
            PosPrevZ = PosZ = pos.Z;

            if (_parameter < 16)
            {
                Color = new Vector4(Gi.ColorRed[_parameter], 
                    Gi.ColorGreen[_parameter], Gi.ColorBlue[_parameter], 1);
            }
            else
            {
                float c = .5f + _rand.NextFloat() * .5f;
                Color = new Vector4(c, c, c, 1);
            }
        }
    }
}
