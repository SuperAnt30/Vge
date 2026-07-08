using System;

namespace Vge.Entity.AI
{
    /// <summary>
    /// Задача паника
    /// </summary>
    public class EntityAIPanic : EntityAIBase
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
        /// Задача бродить
        /// </summary>
        public EntityAIPanic(EntityMob entity, float speed = 1f)
            : base(entity, 3)
        {
            _speed = speed;
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute()
        {
            if (!_entity.InFire() && _entity.GetAITarget() == null || _entity.EntityAge > 200)
            {
                return false;
            }

            int bxz = 7;
            int by = 4;

            _xPosition = _entity.PosX + Rnd.Next(bxz) - Rnd.Next(bxz);
            _yPosition = _entity.PosY + Rnd.Next(by) - Rnd.Next(by);
            _zPosition = _entity.PosZ + Rnd.Next(bxz) - Rnd.Next(bxz);

            return true;
        }

        /// <summary>
        /// Обновляет задачу
        /// </summary>
        public override void UpdateTask()
        {
            if (Rnd.NextFloat() < .4f)
            {
                _entity.MoveHelper.SetSprinting();
            }
        }

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        public override bool ContinueExecuting() => !_entity.Navigator.NoPath();

        /// <summary>
        /// Выполните разовую задачу или начните выполнять непрерывную задачу
        /// </summary>
        public override void StartExecuting()
            => _entity.Navigator.TryMoveToXYZ(_xPosition, _yPosition, _zPosition, _speed);
    }
}
