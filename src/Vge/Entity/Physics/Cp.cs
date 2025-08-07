//#define TPS20
#define TPS30sm50

namespace Vge.Entity.Physics
{
    /// <summary>
    /// Константы для физики (PhysicsConst)
    /// </summary>
    public sealed class Cp
    {
#if TPS20

#region TPS 20 1 метр блок как в minecraft
          
        /// <summary>
        /// Параметр падения
        /// </summary>
        public const float Gravity = .08f;
        /// <summary>
        /// Сопротивление воздуха
        /// </summary>
        public const float AirDrag = .98f;
        /// <summary>
        /// Ускорение в воздухе
        /// </summary>
        public const float AirborneAcceleration = .02f;
        /// <summary>
        /// Скорость
        /// </summary>
        public const float Speed = .1f;
        /// <summary>
        /// Ускорение при прыжке в высоту
        /// </summary>
        public const float AirborneJumpInHeight = .42f;
        /// <summary>
        /// Повторный прыжок через количество тиков
        /// </summary>
        public const byte ReJump = 10;
        /// <summary>
        /// Ускорение при прыжке с бегом в длину
        /// </summary>
        public const float AirborneJumpInLength = .2f;
        /// <summary>
        /// Коэффициент для отладки перемещения скорости в м/с
        /// </summary>
        public const float DebugKoef = Ce.Tps; // =20;

#endregion

#elif TPS30sm50

#region TPS 30 50 см блок

        /// <summary>
        /// Параметр падения
        /// </summary>
        public const float Gravity = .078f;
        /// <summary>
        /// Сопротивление воздуха
        /// </summary>
        public const float AirDrag = .9869f;
        /// <summary>
        /// Ускорение в воздухе
        /// </summary>
        public const float AirborneAcceleration = .02667f;
        /// <summary>
        /// Скорость
        /// </summary>
        public const float Speed = .1333f;
        /// <summary>
        /// Ускорение при прыжке в высоту
        /// </summary>
        public const float AirborneJumpInHeight = .6026f;
        /// <summary>
        /// Повторный прыжок через количество тиков
        /// </summary>
        public const byte ReJump = 15;
        /// <summary>
        /// Ускорение при прыжке с бегом в длину
        /// </summary>
        public const float AirborneJumpInLength = .354f;
        /// <summary>
        /// Коэффициент для отладки перемещения скорости в м/с
        /// </summary>
        public const float DebugKoef = Ce.Tps * .5f; // 30 * .5 = 15;

        #endregion

#else

#region 30 TPS 1 метр блок

        /// <summary>
        /// Параметр падения
        /// </summary>
        public const float Gravity = .039f;
        /// <summary>
        /// Сопротивление воздуха
        /// </summary>
        public const float AirDrag = .9869f;
        /// <summary>
        /// Ускорение в воздухе
        /// </summary>
        public const float AirborneAcceleration = .01333f;
        /// <summary>
        /// Скорость
        /// </summary>
        public const float Speed = .06667f;
        /// <summary>
        /// Ускорение при прыжке в высоту
        /// </summary>
        public const float AirborneJumpInHeight = .3013f;
        /// <summary>
        /// Повторный прыжок через количество тиков
        /// </summary>
        public const byte ReJump = 15;
        /// <summary>
        /// Ускорение при прыжке с бегом в длину
        /// </summary>
        public const float AirborneJumpInLength = .177f;
        /// <summary>
        /// Коэффициент для отладки перемещения скорости в м/с
        /// </summary>
        public const float DebugKoef = Ce.Tps; // =30;

#endregion 

#endif

        /// <summary>
        /// Сопротивление воздуха с силой, кто может перемещаться самостоятельно
        /// </summary>
        public const float AirDragWithForce = .91f;
        /// <summary>
        /// Отскок от гравитации горизонта
        /// </summary>
        public const float GravityRebound = Gravity * 2f;
        /// <summary>
        /// Скользкость по умолчанию
        /// </summary>
        public const float DefaultSlipperiness = .6f;
        /// <summary>
        /// Скорость бега
        /// </summary>
        public const float SprintSpeed = .3f;
        /// <summary>
        /// Скорость подкрадывания
        /// </summary>
        public const float SneakSpeed = .3f;
        /// <summary>
        /// Скорость смещения в сторону, по умолчанию 1.0, перед корнем в физике
        /// </summary>
        public const float StrafeSpeed = .75f;
        /// <summary>
        /// Скорость назад, по умолчанию 1.0, перед корнем в физике
        /// </summary>
        public const float BackSpeed = .8f;
    }
}
