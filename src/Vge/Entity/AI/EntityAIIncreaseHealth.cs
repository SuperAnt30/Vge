namespace Vge.Entity.AI
{
    /// <summary>
    /// Задача увеличить здоровье, если нет нападающего
    /// </summary>
    public class EntityAIIncreaseHealth : EntityAIBase
    {
        /// <summary>
        /// Частота вероятности сработки задачи
        /// </summary>
        private readonly float _probability;

        /// <summary>
        /// Задача увеличить здоровье, если нет нападающего
        /// </summary>
        public EntityAIIncreaseHealth(EntityMob entity, float probability = .008f)
            : base(entity)
        {
            _probability = probability;
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute()
            => _entity.AttackTarget == null
            && !_entity.IsHealthMax() && Rnd.NextFloat() < _probability;

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        public override bool ContinueExecuting() => false;

        /// <summary>
        /// Выполните разовую задачу или начните выполнять непрерывную задачу
        /// </summary>
        public override void StartExecuting() => _entity.HealthAdd(1);
    }
}
