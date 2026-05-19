using Vge.Entity;
using Vge.Entity.Physics;
using Vge.Entity.Sizes;
using Vge.World;

namespace Mvk2.Entity.List
{
    /// <summary>
    /// Сущность курицы
    /// </summary>
    public class EntityChicken : EntityMob
    {

        public EntityChicken() : base()
        {
            SolidHeadWithBody = false;
            _persistenceRequired = true;
        }

        /// <summary>
        /// Инициализация размеров сущности
        /// </summary>
        protected override void _InitSize()
            => Size = new SizeEntityBox(this, .3f, .99f, 100);

        /// <summary>
        /// Инициализация физики
        /// </summary>
        protected override void _InitPhysics(CollisionBase collision)
        {
            PhysicsGroundLiving physicsGround = new PhysicsGroundLiving(collision, this);
            physicsGround.SetHeightAutoJump(1f);
            Physics = physicsGround;
        }

        /// <summary>
        /// Игровой такт на клиенте
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void UpdateClient(WorldClient world, float deltaTime)
        {
            UpdatePositionServer();
            // Поворот тела от поворота головы или движения
            _RotationBody();
            Render.UpdateClient(world, deltaTime);
        }
    }
}
