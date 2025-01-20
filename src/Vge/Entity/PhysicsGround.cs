//#define TPS20
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
        /// Инерция в воздухе
        /// </summary>
        private readonly float _airborneInertia;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collision"></param>
        /// <param name="entity"></param>
        /// <param name="inputMovement">Используется ли у сущности силы действия перемещения</param>
        public PhysicsGround(CollisionBase collision, EntityBase entity, bool inputMovement = true) 
            : base(collision, entity)
        {
            _airborneInertia = inputMovement ? .91f : _airDrag;
        }

        /// <summary>
        /// Обновить данные в такте игры
        /// </summary>
        public override void LivingUpdate()
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
                speed = Mth.Max(speed * Mth.Abs(Movement.GetMoveStrafe()), 
                    speed * Mth.Abs(Movement.GetMoveForward()));
                if (Movement.Sneak) speed *= _sneakSpeed;
                else if (Movement.Sprinting) speed += speed * _sprintSpeed;

                // Ускорение
                acceleration = speed * 0.16277136f / (inertia * inertia * inertia);
            }
            else
            {
                // трение блока в воздухе
                inertia = _airborneInertia;
                // Ускорение
                acceleration = _airborneAcceleration;
                if (Movement.Sprinting) acceleration += acceleration * _sprintSpeed;
            }

            Vector2 motion = Sundry.MotionAngle(
                Movement.GetMoveStrafe() * .98f,
                Movement.GetMoveForward() * .98f,
                acceleration, Entity.RotationYaw);
            MotionX += motion.X;
            MotionZ += motion.Y;

            //System.Console.Write("BMX:");
            //System.Console.Write(MotionX);
            // Проверка кализии
            _CheckMoveCollidingEntity();
            //System.Console.Write(" EMX:");
            //System.Console.WriteLine(MotionX);

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

            //if (Entity.OnGround && MotionY < -_gravity)
            //{
            //    MotionY *= _hz;
            //}
        }
    }
}
