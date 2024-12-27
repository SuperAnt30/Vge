using Vge.Util;
using Vge.World;
using WinGL.Util;

namespace Vge.Entity
{
    /// <summary>
    /// Физика по земле
    /// </summary>
    public class PhysicsGround : PhysicsBase
    {
        /// <summary>
        /// Количество тактов для запрета повторного прыжка
        /// </summary>
        private int _jumpTicks = 0;

        public PhysicsGround(CollisionBase collision, EntityBase entity) 
            : base(collision, entity) { }

        /// <summary>
        /// Обновить данные в такте игры
        /// </summary>
        public override void LivingUpdate()
        {
            // счётчик прыжка
            if (_jumpTicks > 0) _jumpTicks--;

            // Если мелочь убираем
            if (Mth.Abs(MotionX) < .005f) MotionX = 0;
            if (Mth.Abs(MotionY) < .005f) MotionY = 0;
            if (Mth.Abs(MotionZ) < .005f) MotionZ = 0;

            if (Movement.Jump)
            {
                if (Entity.OnGround && _jumpTicks == 0)
                {
                    _jumpTicks = 10;
                    MotionY = .304f;// .608f;// .84f; при 20 tps
                    if (Movement.Sprinting)
                    {
                        // Если прыжок с бегом, то скорость увеличивается
                        MotionX += Glm.Sin(Entity.RotationYaw) * .2f; // .4f;
                        MotionZ -= Glm.Cos(Entity.RotationYaw) * .2f; // .4f;
                    }
                }
            }
            else
            {
                _jumpTicks = 0;
            }

            // Трение
            float friction = .04f;
            // Изучаем корректировки по трению где идём
            float study = 0.91f; // для воздух
            // Трение с блоком
            if (Entity.OnGround)
            {
                // трение воздуха
                study *= (.6f); // блок под ногами

                // корректировка скорости, с трением
                //friction = GetAIMoveSpeed(strafe, forward) * param;

                float speed = .0335f;// .067f;// .133f; // .2f при 20 tps
                speed = Mth.Max(speed * Mth.Abs(Movement.GetMoveStrafe()), 
                    speed * Mth.Abs(Movement.GetMoveForward()));
                if (Movement.Sneak) speed *= .3f;
                else if (Movement.Sprinting) speed *= 2.0f;
                friction = speed * 0.16277136f / (study * study * study);
            }

            Vector2 motion = Sundry.MotionAngle(
                Movement.GetMoveStrafe(), Movement.GetMoveForward(),
                friction, Entity.RotationYaw);
            MotionX += motion.X;
            MotionZ += motion.Y;
            s = motion.Y;

            // Проверка кализии
            _CheckMoveCollidingEntity();

            // Фиксируем перемещение
            IsMotionChange = MotionX != 0 || MotionY != 0 || MotionZ != 0;

            if (IsMotionChange)
            {
                Entity.PosX += MotionX;
                Entity.PosY += MotionY;
                Entity.PosZ += MotionZ;
                MotionHorizon = Glm.Distance(new Vector2(MotionX, MotionZ));
                if (MotionHorizon > s) s = MotionHorizon;
                MotionVertical = Mth.Abs(MotionY);
                Debug.Player = Entity.GetChunkPosition();
            }
            else
            {
                MotionHorizon = MotionVertical = 0;
            }

            // Параметр падение 
            MotionY -= .038f;// .077f;// .16f;  при 20 tps // minecraft .08f

            // Трение воздуха
            MotionX *= study * .98f;
            MotionY *= .98f;
            MotionZ *= study * .98f;
           // s = study;
        }

        private float s;

        public override string ToDebugString() => s.ToString("0.000");
    }
}
