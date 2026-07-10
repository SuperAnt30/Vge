namespace Vge.Entity.AI
{
    /// <summary>
    /// Задача бродить
    /// </summary>
    public class EntityAIWander : EntityAIBaseMove
    {
        /// <summary>
        /// Частота вероятности сработки задачи
        /// </summary>
        private readonly float _probability;

        /// <summary>
        /// Задача бродить
        /// </summary>
        public EntityAIWander(EntityMob entity, float probability = .008f, float speed = 1f)
            : base(entity, speed)
        {
            _probability = probability;
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute()
        {
            // Сущность далековато от игрока и шанс
            if (_entity.EntityAge < 200 && Rnd.NextFloat() < _probability)
            {
                int bxz = 15;
                int by = 7;

                _xPosition = _entity.PosX + Rnd.Next(bxz) - Rnd.Next(bxz);
                _yPosition = _entity.PosY + Rnd.Next(by) - Rnd.Next(by);
                _zPosition = _entity.PosZ + Rnd.Next(bxz) - Rnd.Next(bxz);
                
                return true;
            }

            return false;
        }
    }
}
