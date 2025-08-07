using System;
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

        // TODO::2025-07-23 отладка дистанции прыжка
        //Vector2 jumpBegin;
        //bool jump = false;

        //public override void LivingUpdate()
        //{
        //    base.LivingUpdate();
        //    if (jump && Entity.OnGround)
        //    {
        //        // Прыжок закончен
        //        jump = false;
        //        float dis = Glm.Distance(new Vector2(Entity.PosX, Entity.PosZ), jumpBegin);
        //        Console.WriteLine(dis);
        //    }
        //}

        /// <summary>
        /// Проверяем наличие прыжка для живой сущности
        /// </summary>
        protected override void _LivingUpdateJump()
        {
            // счётчик прыжка
            if (_jumpTicks > 0) _jumpTicks--;

            bool isSneakingPrev = _entityLiving.IsSneaking();
            bool isSprintingPrev = _entityLiving.IsSprinting();

            if (Movement.Jump)
            {
                if (Entity.OnGround && _jumpTicks == 0)
                {
                    //jump = true;
                    //Console.WriteLine("Jump");
                    //jumpBegin = new Vector2(Entity.PosX, Entity.PosZ);
                    _jumpTicks = Cp.ReJump;
                    MotionY = Cp.AirborneJumpInHeight;

                    if (isSprintingPrev)
                    {
                        // Если прыжок с бегом, то скорость увеличивается
                        MotionX += Glm.Sin(_entityLiving.RotationYaw) * Cp.AirborneJumpInLength;
                        MotionZ -= Glm.Cos(_entityLiving.RotationYaw) * Cp.AirborneJumpInLength;
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
            if (isSprintingPrev != isSprinting)
            {
                _entityLiving.SetSprinting(isSprinting);
                // Тут надо сменить анимацию 
                IsPoseChange = true;
            }
        }

        /// <summary>
        /// Определяем и передаём скорость перемещения для живой сущности.
        /// Крадётся используем позже, в _LivingUpdateMotion
        /// </summary>
        protected override float _LivingUpdateSpeed()
        {
            float speed = Cp.Speed * Movement.MoveSpeed;
            if (Movement.Sprinting)
            {
                // Если ускорение
                speed += speed * Cp.SprintSpeed;
            }
            return speed;
        }

        /// <summary>
        /// Проверяем наличие ускорения для живой сущности, возвращает скорость
        /// </summary>
        protected override float _LivingUpdateSprinting()
        {
            float acceleration = Cp.AirborneAcceleration;
            if (Movement.Sprinting)
            {
                // Если ускорение
                acceleration += acceleration * Cp.SprintSpeed;
            }
            return acceleration;
        }

        /// <summary>
        /// Если имеется задаём вектор передвижени для живой сущности
        /// </summary>
        protected override void _LivingUpdateMotion(float acceleration)
        {
            if (Movement.Sneak)
            {
                // Если крадёмся 
                acceleration *= Cp.SneakSpeed;
            }
            Vector2 motion = Sundry.MotionAngle(
                    Movement.MoveStrafe * .98f,
                    Movement.MoveForward * .98f,
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
