using Vge.Entity.Sizes;
using Vge.Util;
using Vge.World;
using WinGL.Util;

namespace Vge.Entity.Physics
{
    /// <summary>
    /// Физика для полёта
    /// </summary>
    public class PhysicsFly : PhysicsBase
    {
        /// <summary>
        /// Живая сущность, игрок или моб
        /// </summary>
        private readonly EntityLiving _entityLiving;

        /// <summary>
        /// Физика без гравитации, для полёта 
        /// </summary>
        /// <param name="inputMovement">Используется ли у сущности силы действия перемещения</param>
        public PhysicsFly(CollisionBase collision, EntityLiving entity) 
            : base(collision, entity, true)
        {
            _entityLiving = entity;
        }
        

        /// <summary>
        /// Обновить данные в такте игры
        /// </summary>
        public override void LivingUpdate()
        {
            // Если нет перемещения по тактам, запускаем трение воздуха
            MotionX *= Cp.AirDrag;
            MotionZ *= Cp.AirDrag;

            // Если мелочь убираем
            ResetMinimumMotion();

            float speed = Cp.Speed;

            MotionY += Movement.MoveVertical * speed * Cp.VerticlSpeedFly;

            speed *= Cp.FactorSpeedFly;

            if (Movement.Sprinting) speed *= Cp.SprintSpeedFly;

            Vector2 motion = Sundry.MotionAngle(Movement.MoveStrafe, Movement.MoveForward, 
                speed, _entityLiving.RotationYaw);

            // Временно меняем перемещение если это надо
            MotionX += motion.X;
            MotionZ += motion.Y;

            // Проверка кализии
            IsMotionChange = MotionX != 0 || MotionY != 0 || MotionZ != 0;
            if (IsMotionChange)
            {
                _CheckMoveColliding((ISizeEntityBox)Entity.Size);

                // Фиксируем перемещение
                IsMotionChange = MotionX != 0 || MotionY != 0 || MotionZ != 0;
            }

            if (IsMotionChange)
            {
                Entity.PosX += MotionX;
                Entity.PosY += MotionY;
                Entity.PosZ += MotionZ;
                MotionHorizon = Glm.Distance(new Vector3(MotionX, 0, MotionZ));
                MotionVertical = Mth.Abs(MotionY);
                Debug.Player = Entity.GetChunkPosition();
            }
            else
            {
                MotionHorizon = MotionVertical = 0;
            }

            // Корректируем для следующего тика

            // Трение воздуха
            MotionX *= Cp.AirDragWithForce;
            MotionY *= Cp.AirDragWithForceVerticlFly;
            MotionZ *= Cp.AirDragWithForce;
        }
    }
}
