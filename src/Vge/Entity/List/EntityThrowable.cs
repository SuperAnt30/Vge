using Vge.Entity.Physics;
using Vge.Renderer.World.Entity;
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
        public EntityLiving EntityThrower { get; private set; }
        /// <summary>
        /// Сколько тиков жизни
        /// </summary>
        protected int _age = 0;
        /// <summary>
        /// Вес сущности
        /// </summary>
        protected float _weight;

        /// <summary>
        /// Для отладки прыгают всегда
        /// </summary>
        private int _jumpTime;

        /// <summary>
        /// Создаёт клиент
        /// </summary>
        public EntityThrowable(ushort indexEntity, EntitiesRenderer entities)
        {
            _InitSize();
            IndexEntity = indexEntity;
            Render = new EntityRenderClient(this, entities, IndexEntity);
        }

        /// <summary>
        /// Сущность метательная, создаёт сервер
        /// </summary>
        /// <param name="entityThrower">Кто метнул</param>
        /// <param name="speedThrower">Скорость метания</param>
        public EntityThrowable(ushort indexEntity, CollisionBase collision,
            EntityLiving entityThrower, int i, float speedThrower = .49f)
        {
            EntityThrower = entityThrower;
            _InitSize();
            IndexEntity = indexEntity;
            Render = new EntityRenderBase(this);
            _InitPhysics(collision);

            // с боку
            //PosX = entityThrower.PosX + Glm.Cos(entityThrower.RotationYaw);// * .4f;
            //PosZ = entityThrower.PosZ + Glm.Sin(entityThrower.RotationYaw);// * .4f;
            // спереди
            float f = (i & 15) * 1.24f;
            float f2 = (i >> 4) * 1.1f;
            PosX = entityThrower.PosX + Glm.Sin(entityThrower.RotationYaw) * f;
            PosZ = entityThrower.PosZ - Glm.Cos(entityThrower.RotationYaw) * f;
            PosY = entityThrower.PosY + entityThrower.Eye - .2f + f2;
            // вверх
            //PosX = entityThrower.PosX;
            //PosZ = entityThrower.PosZ;
            //PosY = entityThrower.PosY + entityThrower.Height + .2f;

            //Physics = new PhysicsGround(collision, this, .9f);
            //Physics.SetImpulse(.5f);

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

        protected virtual void _InitSize()
        {
            Width = .125f;
            Height = .25f;
            _weight = 25;
        }

        protected virtual void _InitPhysics(CollisionBase collision)
        {
            Physics = new PhysicsGround(collision, this, .9f);
        }

        /// <summary>
        /// Вес сущности для определения импулса между сущностями,
        /// У кого больше вес тот больше толкает или меньше потдаётся импульсу.
        /// </summary>
        public override float GetWeight() => _weight;

        /// <summary>
        /// Игровой такт на сервере
        /// </summary>
        public override void Update()
        {
            if (_age > 9000)//1800)
            {
                SetDead();
                return;
            }
            _age++;

            if (!Physics.IsPhysicSleep())
            {
                //Console.Write(PosX);
                //Console.Write(" ");
                //Console.Write(PosY);
                //Console.Write(" ");
                //Console.WriteLine(PosZ);

                // Расчитать перемещение в объекте физика
                Physics.LivingUpdate();

                if (IsPositionChange())
                {
                    //float x = -Physics.MotionX;
                    //float z = -Physics.MotionZ;

                    //RotationYaw = Glm.Atan2(z, x) - Glm.Pi90;
                    //RotationPitch = -Glm.Atan2(-Physics.MotionY, Mth.Sqrt(x * x + z * z));
                    //RotationPrevYaw = RotationYaw;
                    //RotationPrevPitch = RotationPitch;

                    UpdatePositionPrev();
                        
                    LevelMotionChange = 2;
                }
                else if (Physics.IsPhysicAlmostSleep())
                {
                    // Надо ещё 2 такта как минимум передать, чтоб клиент, зафиксировал сон
                    LevelMotionChange = 2;
                }
            }
            else
            {
                //if (_jumpTime++ > 150)
                //{
                //    Physics.MotionY = .5f;
                //    _jumpTime = 0;
                //    Physics.AwakenPhysics();
                //}
            }
           
        }

        /// <summary>
        /// Игровой такт на клиенте
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void UpdateClient(WorldClient world, float deltaTime)
        {
            UpdatePositionServer();
            Render.UpdateClient(world, deltaTime);
        }
    }
}
