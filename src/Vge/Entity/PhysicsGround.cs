//#define TPS20
using System.Collections.Generic;
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
#if TPS20
        // TPS 20 
          
        /// <summary>
        /// Параметр падения
        /// </summary>
        private const float _gravity = .08f;
        /// <summary>
        /// Сопротивление воздуха
        /// </summary>
        private const float _airDrag = .98f;
        /// <summary>
        /// Ускорение в воздухе
        /// </summary>
        private const float _airborneAcceleration = .02f;
        /// <summary>
        /// Скорость
        /// </summary>
        private const float _speed = .1f;
        /// <summary>
        /// Ускорение при прыжке в высоту
        /// </summary>
        private const float _airborneJumpInHeight = .42f;
        /// <summary>
        /// Повторный прыжок через количество тиков
        /// </summary>
        private const byte _reJump = 10;

#else

        // 30 TPS
        /// <summary>
        /// Параметр падения
        /// </summary>
        private const float _gravity = .039f;
        /// <summary>
        /// Сопротивление воздуха
        /// </summary>
        private const float _airDrag = .9869f;
        /// <summary>
        /// Ускорение в воздухе
        /// </summary>
        private const float _airborneAcceleration = .01333f;
        /// <summary>
        /// Скорость
        /// </summary>
        private const float _speed = .0667f;
        /// <summary>
        /// Ускорение при прыжке в высоту
        /// </summary>
        private const float _airborneJumpInHeight = .3013f;
        /// <summary>
        /// Повторный прыжок через количество тиков
        /// </summary>
        private const byte _reJump = 15;

#endif
        /// <summary>
        /// Отскок от гравитации горизонта
        /// </summary>
        public const float GravityRebound = _gravity * 2f;

        /// <summary>
        /// Скользкость по умолчанию
        /// </summary>
        private const float _defaultSlipperiness = .6f;
        /// <summary>
        /// Скорость бега
        /// </summary>
        private const float _sprintSpeed = .3f;
        /// <summary>
        /// Скорость подкрадывания
        /// </summary>
        private const float _sneakSpeed = .3f;
        /// <summary>
        /// Ускорение при прыжке с бегом в длину
        /// </summary>
        private const float _airborneJumpInLength = .2f;

        /// <summary>
        /// Количество тактов для запрета повторного прыжка
        /// </summary>
        private int _jumpTicks = 0;
        /// <summary>
        /// Не прыгаем (момент взлёта)
        /// </summary>
        private bool _isNotJump;
        /// <summary>
        /// Высота авто прыжка
        /// </summary>
        private float _heightAutoJump;
        /// <summary>
        /// Имеется ли авто прыжок
        /// </summary>
        private bool _isAutoJump;

        /// <summary>
        /// Инерция в воздухе
        /// </summary>
        private readonly float _airborneInertia;

        /// <summary>
        /// Физика для сущности которая имеет силу для перемещения
        /// </summary>
        public PhysicsGround(CollisionBase collision, EntityBase entity) 
            : base(collision, entity) => _airborneInertia = .91f;

        /// <summary>
        /// Физика для предмета которые не имеет силы для перемещения но может имет отскок от предметов
        /// </summary>
        /// <param name="rebound">Коэффициент отскока, 0 нет отскока, 1 максимальный</param>
        public PhysicsGround(CollisionBase collision, EntityBase entity, float rebound)
            : base(collision, entity, rebound) => _airborneInertia = _airDrag;

        /// <summary>
        /// Задать высоту автопрыжка, если 0 нет авто прыжка
        /// </summary>
        public PhysicsGround SetHeightAutoJump(float height)
        {
            _heightAutoJump = height;
            _isAutoJump = height != 0;
            return this;
        }

        /// <summary>
        /// Обновить данные в такте игры
        /// </summary>
        public override void LivingUpdate()
        {
            // Если имеется сила для движения, тогда проверяем наличие прыжка
            if (_isForceForMovement)
            {
                // счётчик прыжка
                if (_jumpTicks > 0) _jumpTicks--;

                if (Movement.Jump)
                {
                    if (Entity.OnGround && _jumpTicks == 0)
                    {
                        _jumpTicks = _reJump;
                        MotionY = _airborneJumpInHeight;
                        if (Movement.Sprinting)
                        {
                            // Если прыжок с бегом, то скорость увеличивается
                            MotionX += Glm.Sin(Entity.RotationYaw) * _airborneJumpInLength;
                            MotionZ -= Glm.Cos(Entity.RotationYaw) * _airborneJumpInLength;
                        }
                    }
                }
                else
                {
                    _jumpTicks = 0;
                }
            }

            // Ускорение
            float acceleration;
            // Параметр инерции
            float inertia;
            // Трение с блоком
            if (Entity.OnGround)
            {
                // трение блока под ногами
                inertia = _airborneInertia * _defaultSlipperiness; // блок под ногами

                // корректировка скорости, с трением
                //friction = GetAIMoveSpeed(strafe, forward) * param;

                // Скорость
                float speed = _speed;
                // Если имеется сила для движения, тогда корректируем наличие скорости
                if (_isForceForMovement)
                {
                    speed = Mth.Max(speed * Mth.Abs(Movement.GetMoveStrafe()),
                        speed * Mth.Abs(Movement.GetMoveForward()));
                    if (Movement.Sneak) speed *= _sneakSpeed;
                    else if (Movement.Sprinting) speed += speed * _sprintSpeed;
                }

                // Ускорение
                acceleration = speed * 0.16277136f / (inertia * inertia * inertia);
            }
            else
            {
                // трение блока в воздухе
                inertia = _airborneInertia;
                // Ускорение
                acceleration = _airborneAcceleration;
                if (_isForceForMovement && Movement.Sprinting) acceleration += acceleration * _sprintSpeed;
            }
            // Если имеется сила для движения, задаём вектор передвижения
            if (_isForceForMovement)
            {
                Vector2 motion = Sundry.MotionAngle(
                    Movement.GetMoveStrafe() * .98f,
                    Movement.GetMoveForward() * .98f,
                    acceleration, Entity.RotationYaw);
                MotionX += motion.X;
                MotionZ += motion.Y;
            }

            // Проверка каллизии
            _CheckMoveCollidingEntity();

            // Если мелочь убираем
            if (Mth.Abs(MotionX) < .005f) MotionX = 0;
            if (Mth.Abs(MotionY) < .005f) MotionY = 0;
            if (Mth.Abs(MotionZ) < .005f) MotionZ = 0;

            // Фиксируем перемещение
            IsMotionChange = MotionX != 0 || MotionY != 0 || MotionZ != 0;

            if (IsMotionChange)
            {
                Entity.PosX += MotionX;
                Entity.PosY += MotionY;
                Entity.PosZ += MotionZ;
                MotionHorizon = Glm.Distance(new Vector2(MotionX, MotionZ));
                MotionVertical = Mth.Abs(MotionY);
                Debug.Player = Entity.GetChunkPosition();

                //System.Console.Write("Y:");
                //System.Console.Write(Entity.PosY);
                //System.Console.Write(" X:");
                //System.Console.Write(Entity.PosX);
                //System.Console.Write(" MY:");
                //System.Console.Write(MotionY);
                //System.Console.Write(" MX:");
                //System.Console.WriteLine(MotionX);
            }
            else
            {
                MotionHorizon = MotionVertical = 0;
            }
            // Параметр падение 
            MotionY -= _gravity; // minecraft .08f

            // Инерция
            MotionX *= inertia;
            MotionY *= _airDrag;
            MotionZ *= inertia;
        }

        /// <summary>
        /// Фиксация возможен ли авто прыжок
        /// </summary>
        protected override void _AutoNotJump(float y)
        {
            if (_isAutoJump)
            {
                // Не прыгаем (момент взлёта)
                _isNotJump = Entity.OnGround || MotionY != y && MotionY < 0f;
            }
        }
        /// <summary>
        /// Авто прыжок
        /// </summary>
        protected override void _AutoJump(AxisAlignedBB boundingBox, ref float x, ref float y, ref float z)
        {
            // Запуск проверки авто прыжка
            if (_isAutoJump && _isNotJump && (MotionX != x || MotionZ != z))
            {
                // Кэш для откада, если авто прыжок не допустим
                float monCacheX = x;
                float monCacheY = y;
                float monCacheZ = z;

                float heightAutoJump = _heightAutoJump;
                // Если сидим авто прыжок в двое ниже
                if (_isForceForMovement && Movement.Sneak)
                {
                    heightAutoJump *= 0.5f;
                }

                y = heightAutoJump;
                List<AxisAlignedBB> aabbs = Collision.GetCollidingBoundingBoxes(boundingBox.AddCoordBias(MotionX, y, MotionZ), Entity.Id);
                AxisAlignedBB aabbEntity2 = boundingBox.Clone();
                AxisAlignedBB aabb = aabbEntity2.AddCoordBias(MotionX, 0, MotionZ);

                // Находим смещение по Y
                float y2 = y;
                foreach (AxisAlignedBB axis in aabbs) y2 = axis.CalculateYOffset(aabb, y2);
                aabbEntity2 = aabbEntity2.Offset(0, y2, 0);

                // Находим смещение по X
                float x2 = MotionX;
                foreach (AxisAlignedBB axis in aabbs) x2 = axis.CalculateXOffset(aabbEntity2, x2);
                aabbEntity2 = aabbEntity2.Offset(x2, 0, 0);

                // Находим смещение по Z
                float z2 = MotionZ;
                foreach (AxisAlignedBB axis in aabbs) z2 = axis.CalculateZOffset(aabbEntity2, z2);
                aabbEntity2 = aabbEntity2.Offset(0, 0, z2);

                AxisAlignedBB aabbEntity3 = boundingBox.Clone();

                // Находим смещение по Y
                float y3 = y;
                foreach (AxisAlignedBB axis in aabbs) y3 = axis.CalculateYOffset(aabbEntity3, y3);
                aabbEntity3 = aabbEntity3.Offset(0, y3, 0);

                // Находим смещение по X
                float x3 = MotionX;
                foreach (AxisAlignedBB axis in aabbs) x3 = axis.CalculateXOffset(aabbEntity3, x3);
                aabbEntity3 = aabbEntity3.Offset(x3, 0, 0);

                // Находим смещение по Z
                float z3 = MotionZ;
                foreach (AxisAlignedBB axis in aabbs) z3 = axis.CalculateZOffset(aabbEntity3, z3);
                aabbEntity3 = aabbEntity3.Offset(0, 0, z3);

                y = -heightAutoJump;

                if (x2 * x2 + z2 * z2 > x3 * x3 + z3 * z3)
                {
                    x = x2;
                    z = z2;
                    // Находим итоговое смещение по Y
                    foreach (AxisAlignedBB axis in aabbs) y = axis.CalculateYOffset(aabbEntity2, y);
                }
                else
                {
                    x = x3;
                    z = z3;
                    // Находим итоговое смещение по Y
                    foreach (AxisAlignedBB axis in aabbs) y = axis.CalculateYOffset(aabbEntity3, y);
                }

                if (monCacheX * monCacheX + monCacheZ * monCacheZ >= x * x + z * z)
                {
                    // Нет авто прыжка, откатываем значение обратно
                    x = monCacheX;
                    y = monCacheY;
                    z = monCacheZ;
                }
                else
                {
                    // Авто прыжок
                    Entity.PosY += y + heightAutoJump;
                    y = 0;
                }
            }
        }
    }
}
