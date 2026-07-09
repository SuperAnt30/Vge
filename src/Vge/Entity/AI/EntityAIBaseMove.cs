namespace Vge.Entity.AI
{
    /// <summary>
    /// Абстрактный класс ИИ для перемещений
    /// </summary>
    public abstract class EntityAIBaseMove : EntityAIBase
    {
        /// <summary>
        /// Коэффицент скорости
        /// </summary>
        protected readonly float _speed;

        /// <summary>
        /// Позиция X куда идём
        /// </summary>
        protected float _xPosition;
        /// <summary>
        /// Позиция X куда идём
        /// </summary>
        protected float _yPosition;
        /// <summary>
        /// Позиция X куда идём
        /// </summary>
        protected float _zPosition;
        /// <summary>
        /// Задать расстояние до точки, которое будет считаться выполненым, 0 - расчёта нет
        /// </summary>
        protected float _acceptanceRadius;

        /// <summary>
        /// Задача следовать за своими
        /// </summary>
        public EntityAIBaseMove(EntityMob entity, float speed = 1) : base(entity, 3)
        {
            _speed = speed;
        }

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        public override bool ContinueExecuting() => !_entity.Navigator.NoPath();

        /// <summary>
        /// Выполните разовую задачу или начните выполнять непрерывную задачу
        /// </summary>
        public override void StartExecuting()
            => _entity.Navigator.TryMoveToXYZ(_xPosition, _yPosition, _zPosition,
                _speed, true, true, _acceptanceRadius);

    }
}
