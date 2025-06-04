using Vge.Entity.Physics;
using Vge.Renderer.World.Entity;
using Vge.World;

namespace Vge.Entity.List
{
    /// <summary>
    /// Сущность метательная большая
    /// </summary>
    public class EntityThrowableBig : EntityThrowable
    {
        /// <summary>
        /// Создаёт клиент
        /// </summary>
        public EntityThrowableBig(ushort indexEntity, EntitiesRenderer entities)
            : base(indexEntity, entities) { }

        /// <summary>
        /// Сущность метательная, создаёт сервер
        /// </summary>
        /// <param name="entityThrower">Кто метнул</param>
        /// <param name="speedThrower">Скорость метания</param>
        public EntityThrowableBig(ushort indexEntity, CollisionBase collision,
            EntityLiving entityThrower, int i, float speedThrower)
            : base(indexEntity, collision, entityThrower, i, .3f) { }

        protected override void _InitSize()
        {
            Width = .5f;
            Height = 1f;
            _weight = 100;
        }

        protected override void _InitPhysics(CollisionBase collision)
        {
            Physics = new PhysicsGround(collision, this, 0);
        }
    }
}
