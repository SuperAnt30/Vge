using Vge.World;

namespace Vge.Entity.Physics
{
    /// <summary>
    /// Физика баллистики, быстро летающие предметы, через RayCust
    /// Использует у текущей сущности метод Entity.OnImpact при попадании, она должна решать, что делать дальше.
    /// </summary>
    public class PhysicsBallistics : PhysicsBase
    {
        /// <summary>
        /// Физика для предмета которые не имеет силы для перемещения но может имет отскок от предметов
        /// </summary>
        public PhysicsBallistics(CollisionBase collision, EntityBase entity)
            : base(collision, entity, 0) { }

        /// <summary>
        /// Обновить данные в такте игры
        /// </summary>
        public override void LivingUpdate()
        {
            // Лимит по максимальному импульсу
            _ImpulseLimit();

            // Проверка каллизии
            _CheckMoveCollidingPoint();

            // Если мелочь убираем
            _ResetMinimumMotion();

            // Фиксируем перемещение
            IsMotionChange = MotionX != 0 || MotionY != 0 || MotionZ != 0;

            if (IsMotionChange)
            {
                Entity.PosX += MotionX;
                Entity.PosY += MotionY;
                Entity.PosZ += MotionZ;
            }

            // Параметр падение 
            MotionY -= Cp.Gravity;

            // Инерция
            MotionX *= Cp.AirDrag;
            MotionY *= Cp.AirDrag;
            MotionZ *= Cp.AirDrag;
        }
    }
}
