using System;
using Vge.Util;
using Vge.World;

namespace Vge.Entity.Physics
{
    /// <summary>
    /// Физика для игрока
    /// </summary>
    public class PhysicsPlayer : PhysicsGroundLiving
    {
        /// <summary>
        /// Шаг проверки смещения для защиты от падения, для метода _AdjustMovementForSneaking
        /// </summary>
        private const float _stepAmfs = .0625f;

        public PhysicsPlayer(CollisionBase collision, EntityLiving entity) 
            : base(collision, entity)
        {
            //SetHeightAutoJump(.2f);
            SetHeightAutoJump(2f);
        }

        /// <summary>
        /// Отрегулировать движение для подкрадывания
        /// </summary>
        protected override void _AdjustMovementForSneaking(AxisAlignedBB boundingBox)
        {
            // Тут защита падения и меняем xz
            // Защита от падения с края блока если сидишь и являешься игроком
            if (Entity.OnGround && Movement.Sneak)
            {
                // Если снизу до перемещения был блок, проверяем, если нет, значит стояли на сущности скорее всего
                bool isBlock = Collision.IsStaticBoundingBoxes(boundingBox.Offset(0, -.2f, 0));
                if (isBlock)
                {
                    float x = MotionX;
                    float z = MotionZ;

                    while (x != 0 && !Collision.IsStaticBoundingBoxes(boundingBox.Offset(x, -.2f, 0)))
                    {
                        if (x < _stepAmfs && x >= -_stepAmfs) x = 0f;
                        else if (x > 0f) x -= _stepAmfs;
                        else x += _stepAmfs;
                    }
                    while (z != 0 && !Collision.IsStaticBoundingBoxes(boundingBox.Offset(0, -.2f, z)))
                    {
                        if (z < _stepAmfs && z >= -_stepAmfs) z = 0f;
                        else if (z > 0f) z -= _stepAmfs;
                        else z += _stepAmfs;
                    }
                    while (x != 0 && z != 0 && !Collision.IsStaticBoundingBoxes(boundingBox.Offset(x, -.2f, z)))
                    {
                        if (x < _stepAmfs && x >= -_stepAmfs) x = 0f;
                        else if (x > 0f) x -= _stepAmfs;
                        else x += _stepAmfs;
                        if (z < _stepAmfs && z >= -_stepAmfs) z = 0f;
                        else if (z > 0f) z -= _stepAmfs;
                        else z += _stepAmfs;
                    }

                    if (MotionX != x || MotionZ != z)
                    {
                        MotionX = x;
                        MotionZ = z;
                    }
                }
            }
        }
    }
}
