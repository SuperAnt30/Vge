using Vge.Entity.Physics;
using Vge.World;

namespace Vge.Entity.List
{
    /// <summary>
    /// Сущность метательная большая
    /// </summary>
    public class EntityThrowableBig : EntityThrowable
    {
        /// <summary>
        /// Запуск сущности после всех инициализаций, как правило только на сервере .3
        /// </summary>
        public override void InitRun(EntityLiving entityThrower, int i)
            => _InitRun(entityThrower, i, .3f);

        protected override void _InitSize()
        {
            Width = .5f;
            Height = 1f;
            _weight = 100;
        }

        protected override void _InitPhysics(CollisionBase collision)
            => Physics = new PhysicsGround(collision, this, 0);
    }
}
