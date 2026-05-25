using System.Runtime.CompilerServices;
using Vge.Entity.List;
using Vge.Entity.Physics;
using Vge.World;

namespace Mvk2.Entity.List
{
    /// <summary>
    /// Сущность предмета
    /// </summary>
    public class EntityItemMvk : EntityItem
    {
        /// <summary>
        /// Инициализация физики
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _InitPhysics(CollisionBase collision)
        {
            Physics = new PhysicsGround(collision, this, .5f);
            PresenceBlocks = new PresenceBlocksMvk();
        }
    }
}
