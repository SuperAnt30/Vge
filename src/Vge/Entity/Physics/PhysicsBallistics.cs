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
        /// false - учитываем все блоки, без атрибуты NoCollision
        /// </summary>
        private readonly bool _collidable;

        /// <summary>
        /// Физика для предмета которые не имеет силы для перемещения но может имет отскок от предметов
        /// </summary>
        /// <param name = "collidable" > false - учитываем все блоки, без атрибуты NoCollision</param>
        public PhysicsBallistics(CollisionBase collision, EntityBase entity, bool collidable)
            : base(collision, entity, 0) => _collidable = collidable;

        /// <summary>
        /// Обновить данные в такте игры
        /// </summary>
        public override void LivingUpdate()
        {
            // Лимит по максимальному импульсу
            _ImpulseLimit();

            // Проверка каллизии
            _CheckMoveCollidingPoint(_collidable);

            // Если мелочь убираем
            ResetMinimumMotion();

            // Фиксируем перемещение
            IsMotionChange = MotionX != 0 || MotionY != 0 || MotionZ != 0;

            if (IsMotionChange)
            {
                Entity.PosX += MotionX;
                Entity.PosY += MotionY;
                Entity.PosZ += MotionZ;
            }

            // Параметр падение 
            MotionY -= _gravity;

            // Инерция
            MotionX *= Cp.AirDrag;
            MotionY *= Cp.AirDrag;
            MotionZ *= Cp.AirDrag;
        }
    }
}
