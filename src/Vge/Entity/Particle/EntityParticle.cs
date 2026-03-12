using System.Runtime.CompilerServices;
using Vge.Entity.Physics;
using Vge.Entity.Render;
using Vge.Entity.Sizes;
using Vge.Renderer.World;
using Vge.Renderer.World.Entity;
using Vge.Util;
using Vge.World;
using WinGL.Util;

namespace Vge.Entity.Particle
{
    /// <summary>
    /// Сущность эффектов частиц
    /// </summary>
    public /*abstract*/ class EntityParticle : EntityBase
    {
        /// <summary>
        /// Сколько тиков жизни
        /// </summary>
        protected int _age = 0;

        /// <summary>
        /// Инициализация для клиента
        /// </summary>
        public void Init(ParticlesRenderer particles, CollisionBase collision)
        {
            Render = new EntityRenderParticle(this, particles);
            Size = new SizeEntityPoint(this, 1);
            Physics = new PhysicsBallistics(collision, this);
            NoClip = true;
            Physics.SetGravity(.25f);
        }

        /// <summary>
        /// Запуск сущности после всех инициализаций, как правило только на сервере
        /// </summary>
        public virtual void InitRun(Rand rand, Vector3 pos, Vector3 motion)
        {
            PosPrevX = PosX = pos.X;
            PosPrevY = PosY = pos.Y;
            PosPrevZ = PosZ = pos.Z;

            motion.X += (rand.Next(200) - 100) * .004f;
            motion.Y += (rand.Next(200) - 100) * .004f;
            motion.Z += (rand.Next(200) - 100) * .004f;
            float r = (rand.NextFloat() + rand.NextFloat() + 1f) * .036f;
            motion = motion / Glm.Distance(motion) * r;
            motion.Y += .1f;

            Physics.MotionX = motion.X;
            Physics.MotionY = motion.Y;
            Physics.MotionZ = motion.Z;

            //// ВРЕМЕННО, бросок сущности!
            //float speedThrower = .49f;
            //// спереди
            //float f3 = (int)(i / 100) * 1.54f;
            //i = i % 100;
            //float f = (i & 15) * 1.54f + 1;
            //float f2 = (i >> 4) * 1.3f;
            //PosX = entityThrower.PosX + Glm.Sin(entityThrower.RotationYaw) * f
            //    - Glm.Cos(entityThrower.RotationYaw) * f3;
            //PosZ = entityThrower.PosZ - Glm.Cos(entityThrower.RotationYaw) * f
            //    + Glm.Sin(entityThrower.RotationYaw) * f3;
            //PosY = entityThrower.PosY + entityThrower.SizeLiving.GetEye() - .2f + f2;

            //float pitchxz = Glm.Cos(entityThrower.RotationPitch);
            //Physics.MotionX = Glm.Sin(entityThrower.RotationYaw) * pitchxz * speedThrower;
            //Physics.MotionY = Glm.Sin(entityThrower.RotationPitch) * speedThrower;
            //Physics.MotionZ = -Glm.Cos(entityThrower.RotationYaw) * pitchxz * speedThrower;
        }

        /// <summary>
        /// Вызывается, когда быстрая сущность сталкивается с блоком или сущностью.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void OnImpact(WorldBase world, MovingObjectPosition moving)
        {
            // Проверка перемещения со столкновением сущностей
            Physics.MotionX = moving.RayHit.X - PosX;
            Physics.MotionY = moving.RayHit.Y - PosY;
            Physics.MotionZ = moving.RayHit.Z - PosZ;
            Physics.ResetMinimumMotion();

            if (Physics.MotionX != 0) Physics.MotionX += Physics.MotionX > 0 ? -.005f : .005f;
            if (Physics.MotionY != 0) Physics.MotionY += Physics.MotionY > 0 ? -.005f : .005f;
            if (Physics.MotionZ != 0) Physics.MotionZ += Physics.MotionZ > 0 ? -.005f : .005f;
        }

        /// <summary>
        /// Игровой такт на сервере
        /// </summary>
        public override void Update()
        {
            
            if (_age > 45)
            {
                SetDead();
                return;
            }
            _age++;
            if (_age == 10)
            {
                NoClip = false;
            }
            if (IsPositionChange())
            {
                PosPrevX = PosX;
                PosPrevY = PosY;
                PosPrevZ = PosZ;
            }
            // Расчитать перемещение в объекте физика
            Physics.LivingUpdate();
        }

        /// <summary>
        /// Игровой такт на клиенте
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void UpdateClient(WorldClient world, float deltaTime)
        {
            Render.UpdateClient(world, deltaTime);
        }
    }
}
