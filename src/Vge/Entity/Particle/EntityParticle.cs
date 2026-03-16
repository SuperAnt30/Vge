using System.Runtime.CompilerServices;
using Vge.Entity.Physics;
using Vge.Entity.Render;
using Vge.Entity.Sizes;
using Vge.Renderer.World;
using Vge.Util;
using Vge.World;
using WinGL.Util;

namespace Vge.Entity.Particle
{
    /// <summary>
    /// Абстрактный объект сущность частицы
    /// </summary>
    public class EntityParticle : EntityBase
    {
        /// <summary>
        /// Тип прорисовки частички
        /// </summary>
        public readonly EnumParticleDraw TypeDraw;
        /// <summary>
        /// Является ли частичка объёмной 3д
        /// </summary>
        public readonly bool IsCube;
        /// <summary>
        /// Имеется ли текстура, только для 2д спрайта
        /// </summary>
        public readonly bool IsSprite;

        /// <summary>
        /// Цвет частички
        /// </summary>
        public Vector4 Color { get; protected set; } = new Vector4(1);
        /// <summary>
        /// Масштаб частички
        /// </summary>
        public float Scale { get; protected set; } = 1f;
        /// <summary>
        /// Смещение спрайта
        /// </summary>
        public Vector2 OffsetUv { get; protected set; }

        /// <summary>
        /// Сколько тиков жизни
        /// </summary>
        protected int _age = 0;
        /// <summary>
        /// Объект генератора случайх чисел
        /// </summary>
        protected Rand _rand;

        public EntityParticle(EnumParticleDraw typeDraw)
        {
            TypeDraw = typeDraw;
            IsCube = typeDraw == EnumParticleDraw.Cube;
            IsSprite = typeDraw == EnumParticleDraw.Sprite;
        }

        /// <summary>
        /// Инициализация для клиента
        /// </summary>
        public void Init(ushort index, ParticlesRenderer particles, CollisionBase collision)
        {
            IndexEntity = index;
            Size = new SizeEntityPoint(this, 1);
            Physics = new PhysicsBallistics(collision, this);
            NoClip = true;
            Physics.SetGravity(.25f);
            Render = new EntityRenderParticle(this, particles);
        }

        /// <summary>
        /// Запуск сущности после всех инициализаций, как правило только на сервере
        /// </summary>
        public virtual void InitRun(Rand rand, Vector3 pos, Vector3 motion)
        {
            _rand = rand;
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

            if (IsCube)
            {
                Scale = .25f + rand.NextFloat() * .25f;
            }

         
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
            if (_age > 400)
            {
                SetDead();
                return;
            }

            //if (!IsVolumetricForm())
            //{
            //    Color = new Vector4(_rand.NextFloat(), _rand.NextFloat(), _rand.NextFloat(), 1);
            //}

            //Scale *= .95f;

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
            if (Physics.CaughtInBlock > 0 || Physics.CaughtInEntity > 2)
            {
                SetDead();
            }
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
