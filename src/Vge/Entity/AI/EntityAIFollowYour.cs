using Vge.World;

namespace Vge.Entity.AI
{
    /// <summary>
    /// Задача следовать за своими
    /// </summary>
    public class EntityAIFollowYour : EntityAIBase
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
        /// Отдалённость, на которую идёт проверка группы
        /// </summary>
        private readonly int _destination;

        /// <summary>
        /// Объект коллизии чтоб искать сущности
        /// </summary>
        private readonly CollisionBase _collision;

        /// <summary>
        /// Задача следовать за своими
        /// </summary>
        public EntityAIFollowYour(EntityMob entity, float probability = .008f, 
            float speed = 1f, int destination = 12) : base(entity, 3)
        {
            _collision = _entity.GetWorldServer().Collision;
            _speed = speed;
            _probability = probability;
            _destination = destination;
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute()
        {
            if (_entity.EntityAge > 100 || Rnd.NextFloat() >= _probability)
            {
                return false;
            }

            //Profiler.StartConsole();
            _collision.EntityBoundingBoxesFromSectorType(
                _entity.SizeLiving.GetBoundingBox().Expand(_destination, 4, _destination),
                -1, _entity.IndexEntity);
            int count = _collision.ListEntity.Count;

            if (count == 1)
            {
                // Ищем ближайшего и бежим к нему
                _collision.EntityBoundingBoxesFromSectorType(
                _entity.SizeLiving.GetBoundingBox().Expand(_destination * 2, 8, _destination * 2),
                -1, _entity.IndexEntity);
                count = _collision.ListEntity.Count;
            }
            //Profiler.StopConsole("FollowYour " + count.ToString());

            if (count > 1)
            {
                // Много разных, находим среднее растояние, себя учитываем
                float x = 0;
                float y = 0;
                float z = 0;
                EntityBase entity;

                for (int i = 0; i < count; i++)
                {
                    entity = _collision.ListEntity[i];
                    x += entity.PosX;
                    y += entity.PosY;
                    z += entity.PosZ;
                }
                _xPosition = x / count;
                _yPosition = y / count;
                _zPosition = z / count;
                return true;
            }

            return false;
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
                _speed, true, true, Rnd.NextFloat() * 3f + 1f);

    }
}
