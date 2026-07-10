using Vge.Entity.Player;
using WinGL.Util;

namespace Vge.Entity.AI
{
    /// <summary>
    /// Задача смотреть на ближайшего игрока
    /// </summary>
    public class EntityAIWatchClosest : EntityAIBase
    {
        /// <summary>
        /// Сущность на которую смотрит
        /// </summary>
        private PlayerServer _closestEntity;
        /// <summary>
        /// Это максимальное расстояние, на котором ИИ будет искать сущность
        /// </summary>
        private readonly float _maxDistanceForPlayer;
        /// <summary>
        /// Частота вероятности сработки задачи
        /// </summary>
        private readonly float _probability;

        /// <summary>
        /// Задача смотреть на ближайшего игрока
        /// </summary>
        public EntityAIWatchClosest(EntityMob entity, float distance, float probability = .02f)
            : base(entity, 2)
        {
            _maxDistanceForPlayer = distance;
            _probability = probability;
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute()
        {
            if (_entity.Tracker != null 
                && _entity.Tracker.ClosestPlayer != null 
                && Rnd.NextFloat() < _probability 
                && _entity.Tracker.DistantionPlayer < _maxDistanceForPlayer)
            {
                _closestEntity = _entity.Tracker.ClosestPlayer;
                return true;
            }
            
            _closestEntity = null;
            return false;
        }

        /// <summary>
        /// Сбрасывает задачу
        /// </summary>
        public override void ResetTask() => _closestEntity = null;

        /// <summary>
        /// Обновляет задачу
        /// </summary>
        public override void UpdateTask()
        {
            _entity.LookHelper.SetLookPositionWithEntity(_closestEntity, Glm.Pi10, Glm.Pi10);
        }
    }
}
