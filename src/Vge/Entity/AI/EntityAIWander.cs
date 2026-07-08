using System;

namespace Vge.Entity.AI
{
    /// <summary>
    /// Задача бродить
    /// </summary>
    public class EntityAIWander : EntityAIBase
    {
        /// <summary>
        /// Позиция X куда идём
        /// </summary>
        private float _xPosition;
        /// <summary>
        /// Позиция X куда идём
        /// </summary>
        private float _yPosition;
        /// <summary>
        /// Позиция X куда идём
        /// </summary>
        private float _zPosition;
        /// <summary>
        /// Коэффицент скорости
        /// </summary>
        private readonly float _speed;
        /// <summary>
        /// Частота вероятности сработки задачи
        /// </summary>
        private readonly float _probability;
        /// <summary>
        /// Параметр мгновенного выполнения этой задачи
        /// </summary>
        private bool _instantExecution;

        /// <summary>
        /// Задача бродить
        /// </summary>
        public EntityAIWander(EntityMob entity, float probability = .008f, float speed = 1f)
            : base(entity, 3)
        {
            _speed = speed;
            _probability = probability;
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute()
        {
            if (!_instantExecution)
            {
                // Сущность далековато от игрока и шанс
                if (_entity.EntityAge > 100 || Rnd.NextFloat() >= _probability)
                {
                    return false;
                }
            }

            _xPosition = _entity.PosX;
            _yPosition = _entity.PosY;
            _zPosition = _entity.PosZ;

            int bxz = 15;
            int by = 7;
            _xPosition += Rnd.Next(bxz) - Rnd.Next(bxz);
            _yPosition += Rnd.Next(by) - Rnd.Next(by);
            _zPosition += Rnd.Next(bxz) - Rnd.Next(bxz);
            _instantExecution = false;
            return true;

        }

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        public override bool ContinueExecuting() => !_entity.Navigator.NoPath();

        /// <summary>
        /// Выполните разовую задачу или начните выполнять непрерывную задачу
        /// </summary>
        public override void StartExecuting()
            => _entity.Navigator.TryMoveToXYZ(_xPosition, _yPosition, _zPosition, _speed, true, true);

        /// <summary>
        /// Мгновенное выполнение
        /// </summary>
        public void InstantExecution() => _instantExecution = true;
    }
}
