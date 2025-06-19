using Vge.Entity.Physics;
using Vge.Util;
using Vge.World;
using Vge.World.Block;

namespace Vge.Entity.List
{
    /// <summary>
    /// Сущность метательная большая
    /// </summary>
    public class EntityThrowableSmall : EntityThrowable
    {
        /// <summary>
        /// Запуск сущности после всех инициализаций, как правило только на сервере .3
        /// </summary>
        public override void InitRun(EntityLiving entityThrower, int i)
            => _InitRun(entityThrower, i, .6f);

        protected override void _InitSize()
            => Size = new SizeEntityPoint(this, 25);

        protected override void _InitPhysics(CollisionBase collision)
            => Physics = new PhysicsGround(collision, this, .9f);

        /// <summary>
        /// Возвращает true, если другие Сущности не должны проходить через эту Сущность
        /// </summary>
        public override bool CanBeCollidedWith() => false;

        /// <summary>
        /// Вызывается, когда быстрая сущность сталкивается с блоком или сущностью.
        /// </summary>
        public override void OnImpact(WorldBase world, MovingObjectPosition moving)
        {
            SetDead();
            if (moving.IsEntity())
            {
                moving.Entity.SetDead();
            }
            else if (moving.IsBlock())
            {
                world.SetBlockToAir(moving.BlockPosition, world.IsRemote ? 14 : 31);
            }
        }
    }
}
