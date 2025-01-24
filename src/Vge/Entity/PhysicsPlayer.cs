using Vge.Util;
using Vge.World;

namespace Vge.Entity
{
    /// <summary>
    /// Физика для игрока
    /// </summary>
    public class PhysicsPlayer : PhysicsGround
    {
        /// <summary>
        /// Шаг проверки смещения для защиты от падения, для метода _AdjustMovementForSneaking
        /// </summary>
        private const float _stepAmfs = .0625f;

        public PhysicsPlayer(CollisionBase collision, EntityBase entity) 
            : base(collision, entity)
        {
            SetHeightAutoJump(.2f);
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
                float x = MotionX;
                float z = MotionZ;

                while (x != 0 && Collision.GetCollidingBoundingBoxes(boundingBox.Offset(x, -1, 0), Entity.Id).Count == 0)
                {
                    if (x < _stepAmfs && x >= -_stepAmfs) x = 0f;
                    else if (x > 0f) x -= _stepAmfs;
                    else x += _stepAmfs;
                }
                while (z != 0 && Collision.GetCollidingBoundingBoxes(boundingBox.Offset(0, -1, z), Entity.Id).Count == 0)
                {
                    if (z < _stepAmfs && z >= -_stepAmfs) z = 0f;
                    else if (z > 0f) z -= _stepAmfs;
                    else z += _stepAmfs;
                }
                while (x != 0 && z != 0 && Collision.GetCollidingBoundingBoxes(boundingBox.Offset(x, -1, z), Entity.Id).Count == 0)
                {
                    if (x < _stepAmfs && x >= -_stepAmfs) x = 0f;
                    else if (x > 0f) x -= _stepAmfs;
                    else x += _stepAmfs;
                    if (z < _stepAmfs && z >= -_stepAmfs) z = 0f;
                    else if (z > 0f) z -= _stepAmfs;
                    else z += _stepAmfs;
                }

                MotionX = x;
                MotionZ = z;
            }
        }
    }
}
