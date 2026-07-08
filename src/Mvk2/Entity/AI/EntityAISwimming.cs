using Mvk2.Entity.AI.PathFinding;
using Vge.Entity;
using Vge.Entity.AI;

namespace Mvk2.Entity.AI
{
    /// <summary>
    /// Задача всплывать если в воде
    /// </summary>
    public class EntityAISwimming : EntityAIBase
    {
        /// <summary>
        /// Задача всплывать если в воде
        /// </summary>
        public EntityAISwimming(EntityMob entity) : base(entity, 4)
        {
            ((PathNavigateGround)entity.Navigator).CanSwim(true);
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute() => _entity.PresenceBlocks.IsInLiquid;

        /// <summary>
        /// Обновляет задачу
        /// </summary>
        public override void UpdateTask()
        {
            if (Rnd.NextFloat() < .8f)
            {
                _entity.MoveHelper.SetJumping();
            }
        }
    }
}
