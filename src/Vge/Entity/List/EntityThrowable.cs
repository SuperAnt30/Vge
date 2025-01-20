using Vge.Util;
using Vge.World;
using WinGL.Util;

namespace Vge.Entity.List
{
    /// <summary>
    /// Сущность метательная
    /// </summary>
    public class EntityThrowable : EntityBase
    {
        /// <summary>
        /// Кто метнул предмет
        /// </summary>
        public EntityBase EntityThrower { get; private set; }
        /// <summary>
        /// Сколько тиков жизни
        /// </summary>
        private int _age = 0;

        public EntityThrowable()
        {
            Width = .125f;
            Height = .25f;
        }

        /// <summary>
        /// Сущность метательная
        /// </summary>
        /// <param name="entityThrower">Кто метнул</param>
        /// <param name="speedThrower">Скорость метания</param>
        public EntityThrowable(CollisionBase collision, EntityBase entityThrower, float speedThrower = .9f)
        {
            EntityThrower = entityThrower;
            Width = .125f;
            Height = .25f;

            PosX = entityThrower.PosX + Glm.Cos(entityThrower.RotationYaw) * .32f;
            PosZ = entityThrower.PosZ + Glm.Sin(entityThrower.RotationYaw) * .32f;
            PosY = entityThrower.PosY + entityThrower.Eye - .2f;

            Physics = new PhysicsGround(collision, this, false);
            //Physics.Movement.Forward = true;
            //Physics.Movement.Sprinting = true;
            float pitchxz = Glm.Cos(entityThrower.RotationPitch);
            Physics.MotionX = Glm.Sin(entityThrower.RotationYaw) * pitchxz * speedThrower;
            Physics.MotionY = Glm.Sin(entityThrower.RotationPitch) * speedThrower;
            Physics.MotionZ = -Glm.Cos(entityThrower.RotationYaw) * pitchxz * speedThrower;

            //float f1 = rand.NextFloat() * .02f;
            //float f2 = rand.NextFloat() * glm.pi360;
            //motion.x += glm.cos(f2) * f1;
            //motion.z += glm.sin(f2) * f1;
            //Motion = motion;
        }

        public override void Update()
        {
            if (Physics != null)
            {
                if (_age > 120)
                {
                    SetDead();
                    return;
                }
                _age++;

                // Расчитать перемещение в объекте физика
                Physics.LivingUpdate();

                if (OnGround || !Physics.IsMotionChange)
                {
                    Physics.Movement.Forward = false;
                    Physics.Movement.Sprinting = false;
                }
                if (IsPositionChange())
                {
                    float x = -Physics.MotionX;
                    float z = -Physics.MotionZ;
                    RotationYaw = Glm.Atan2(z, x) - Glm.Pi90;
                    RotationPitch = -Glm.Atan2(-Physics.MotionY, Mth.Sqrt(x * x + z * z));

                    // RotationYaw = Glm.WrapAngleToPi(RotationYaw + Glm.Pi45);
                    PosPrevX = PosX;
                    PosPrevY = PosY;
                    PosPrevZ = PosZ;
                    RotationPrevYaw = RotationYaw;
                    RotationPrevPitch = RotationPitch;

                }
            }
        }
    }
}
