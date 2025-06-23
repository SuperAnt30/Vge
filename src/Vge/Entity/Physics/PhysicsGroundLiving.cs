//#define TPS20
using System.Collections.Generic;
using Vge.Util;
using Vge.World;
using WinGL.Util;

namespace Vge.Entity.Physics
{
    /// <summary>
    /// Физика по земле для сущностей которые имеют силу для перемещения
    /// </summary>
    public class PhysicsGroundLiving : PhysicsGround
    {
        /// <summary>
        /// Ускорение при прыжке в высоту
        /// </summary>
        private const float _airborneJumpInHeight = .3013f;
        /// <summary>
        /// Повторный прыжок через количество тиков
        /// </summary>
        private const byte _reJump = 15;
        /// <summary>
        /// Ускорение при прыжке с бегом в длину
        /// </summary>
        private const float _airborneJumpInLength = .2f;
        /// <summary>
        /// Скорость бега
        /// </summary>
        private const float _sprintSpeed = .3f;
        /// <summary>
        /// Скорость подкрадывания
        /// </summary>
        private const float _sneakSpeed = .3f;

        /// <summary>
        /// Живая сущность, игрок или моб
        /// </summary>
        private readonly EntityLiving _entityLiving;
        /// <summary>
        /// Количество тактов для запрета повторного прыжка
        /// </summary>
        private int _jumpTicks = 0;

        /// <summary>
        /// Физика для сущности которая имеет силу для перемещения
        /// </summary>
        public PhysicsGroundLiving(CollisionBase collision, EntityLiving entity)
            : base(collision, entity) => _entityLiving = entity;


        #region Living

        /// <summary>
        /// Проверяем наличие прыжка для живой сущности
        /// </summary>
        protected override void _LivingUpdateJump()
        {
            // счётчик прыжка
            if (_jumpTicks > 0) _jumpTicks--;

            bool isSneakingPrev = _entityLiving.IsSneaking();

            if (Movement.Jump)
            {
                if (Entity.OnGround && _jumpTicks == 0)
                {
                    _jumpTicks = _reJump;
                    MotionY = _airborneJumpInHeight;
                    if (isSneakingPrev)
                    {
                        // Если прыжок с бегом, то скорость увеличивается
                        MotionX += Glm.Sin(_entityLiving.RotationYaw) * _airborneJumpInLength;
                        MotionZ -= Glm.Cos(_entityLiving.RotationYaw) * _airborneJumpInLength;
                    }
                }
            }
            else
            {
                _jumpTicks = 0;
            }

            // Обновить положение сидя
            bool isSneaking = _entityLiving.IsSneaking();
            if (Entity.OnGround && Movement.Sneak && !isSneaking)
            {
                // Только на земле, и в прошлом такте не сидел
                _entityLiving.SetSneaking(true);
                _entityLiving.Sitting();
                IsPoseChange = true;
            }
            // Если хотим встать
            else if (!Movement.Sneak && isSneaking)
            {
                // Проверка коллизии вверхней части при положении стоя
                _entityLiving.SetSneaking(false);
                _entityLiving.Standing();
                IsPoseChange = true;
            }

            // Sprinting
            bool isSprinting = Movement.Sprinting && Movement.Forward && !isSneakingPrev;
            if (_entityLiving.IsSprinting() != isSprinting)
            {
                _entityLiving.SetSprinting(isSprinting);
                // Тут надо сменить анимацию 
                IsPoseChange = true;
            }
        }

        /// <summary>
        /// Определяем и передаём скорость перемещения для живой сущности
        /// </summary>
        protected override float _LivingUpdateSpeed()
        {
            float speed = Mth.Max(_speed * Mth.Abs(Movement.GetMoveStrafe()),
                        _speed * Mth.Abs(Movement.GetMoveForward()));
            if (Movement.Sneak) speed *= _sneakSpeed;
            else if (Movement.Sprinting) speed += speed * _sprintSpeed;
            return speed;
        }

        /// <summary>
        /// Проверяем наличие ускорения для живой сущности, возвращает скорость
        /// </summary>
        protected override float _LivingUpdateSprinting()
        {
            float acceleration = _airborneAcceleration;
            if (Movement.Sprinting)
            {
                acceleration += acceleration * _sprintSpeed;
            }
            return acceleration;
        }

        /// <summary>
        /// Если имеется задаём вектор передвижени для живой сущности
        /// </summary>
        protected override void _LivingUpdateMotion(float acceleration)
        {
            Vector2 motion = Sundry.MotionAngle(
                    Movement.GetMoveStrafe() * .98f,
                    Movement.GetMoveForward() * .98f,
                    acceleration, _entityLiving.RotationYaw);
            MotionX += motion.X;
            MotionZ += motion.Y;
        }

        /// <summary>
        /// Сидит ли сущность
        /// </summary>
        protected override bool _IsMovementSneak() => Movement.Sneak;

        #endregion
    }
}
