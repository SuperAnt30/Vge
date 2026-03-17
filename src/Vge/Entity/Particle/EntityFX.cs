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
    public abstract class EntityFX : EntityBase
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
        /// Расположение спрайта текстуры x-u1 y-v1 z-u2 w-v2
        /// </summary>
        public Vector4 Uv { get; protected set; }
        /// <summary>
        /// Параметр для шейдора, bit флагов
        /// 1 bit - текстура для 2д
        /// 2 bit - свет не меняется от яркости блоков
        /// </summary>
        public int Param { get; protected set; }

        /// <summary>
        /// Текущее состояние, сколько прожила частица в тактах
        /// </summary>
        protected int _age = 0;
        /// <summary>
        /// Максимальная жизнь частицы в тактах
        /// </summary>
        protected int _maxAge;
        /// <summary>
        /// Гравитация частицы
        /// </summary>
        protected float _gravity;
        /// <summary>
        /// Объект генератора случайх чисел
        /// </summary>
        protected readonly Rand _rand;

        public EntityFX(EnumParticleDraw typeDraw, Rand rand)
        {
            TypeDraw = typeDraw;
            IsCube = typeDraw == EnumParticleDraw.Cube;
            IsSprite = typeDraw == EnumParticleDraw.Sprite;
            Param = IsSprite ? 1 : 0;
            _rand = rand;
            Scale = (100 + _rand.Next(100)) / 100f;
            _maxAge = 4 + _rand.Next(36);
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
            Physics.SetGravity(_gravity);
            Render = new EntityRenderParticle(this, particles);
        }

        /// <summary>
        /// Запуск сущности после всех инициализаций, как правило только на сервере
        /// </summary>
        public virtual void InitRun(Vector3 pos, Vector3 motion)
        {
            PosPrevX = PosX = pos.X;
            PosPrevY = PosY = pos.Y;
            PosPrevZ = PosZ = pos.Z;

            motion.X += (_rand.Next(200) - 100) * .004f;
            motion.Y += (_rand.Next(200) - 100) * .004f;
            motion.Z += (_rand.Next(200) - 100) * .004f;
            float r = (_rand.NextFloat() + _rand.NextFloat() + 1f) * .036f;
            motion = motion / Glm.Distance(motion) * r;
            motion.Y += .1f;

            Physics.MotionX = motion.X;
            Physics.MotionY = motion.Y;
            Physics.MotionZ = motion.Z;

            if (IsCube)
            {
                Scale = .25f + _rand.NextFloat() * .25f;
            }
        }

        /// <summary>
        /// Задать светится ли частичка в темноте
        /// </summary>
        public void SetLight(bool light)
            => Param = (IsSprite ? 1 : 0) + (light ? 2 : 0);

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
            if (_age++ > _maxAge)
            {
                SetDead();
            }

            else
            {



                //if (!IsVolumetricForm())
                //{
                //    Color = new Vector4(_rand.NextFloat(), _rand.NextFloat(), _rand.NextFloat(), 1);
                //}

                //Scale *= .95f;
                //  SetLight(Param >> 1 == 0);

                if (_age == 3)
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
        }

        /// <summary>
        /// Игровой такт на клиенте
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void UpdateClient(WorldClient world, float deltaTime)
            => Render.UpdateClient(world, deltaTime);
    }
}
