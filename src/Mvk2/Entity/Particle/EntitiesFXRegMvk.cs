using Vge.Entity.Particle;

namespace Mvk2.Entity.Particle
{
    /// <summary>
    /// Регистрация моделей сущностей эффектов для Малювеки 2
    /// </summary>
    public sealed class EntitiesFXRegMvk
    {
        /// <summary>
        /// Индекс частички дыма
        /// </summary>
        public static ushort SmokeId { get; private set; }
        /// <summary>
        /// Индекс частички огня
        /// </summary>
        public static ushort FlameId { get; private set; }
        /// <summary>
        /// Индекс частички капельки
        /// </summary>
        public static ushort DropletId { get; private set; }
        /// <summary>
        /// Индекс частички языка пламени с дымом
        /// </summary>
        public static ushort FlameSmokeId { get; private set; }

        public static void Initialization()
        {
            EntitiesFXReg.RegisterEntityFXClass("Droplet", typeof(EntityDropletFX));
            EntitiesFXReg.RegisterEntityFXClass("Smoke", typeof(EntitySmokeFX));
            EntitiesFXReg.RegisterEntityFXClass("Flame", typeof(EntityFlameFX));
            EntitiesFXReg.RegisterEntityFXClass("FlameSmoke", typeof(EntityFlameSmokeFX));

        }

        /// <summary>
        /// Инициализация Id
        /// </summary>
        public static void InitId()
        {
            EntitiesFXReg.InitId();
            string alias;
            for (ushort i = 0; i < Ce.EntitiesFX.Count; i++)
            {
                alias = Ce.EntitiesFX.EntitiesFXAlias[i];
                if (alias == "Smoke") SmokeId = i;
                else if (alias == "Droplet") DropletId = i;
                else if (alias == "Flame") FlameId = i;
                else if (alias == "FlameSmoke") FlameSmokeId = i;
            }
        }
    }
}
