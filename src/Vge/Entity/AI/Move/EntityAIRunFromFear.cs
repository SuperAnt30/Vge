using WinGL.Util;

namespace Vge.Entity.AI
{
    /// <summary>
    /// Задача убегать от страха, от игрока
    /// </summary>
    public class EntityAIRunFromFear : EntityAIBaseMove
    {
        /// <summary>
        /// Это максимальное расстояние, на котором ИИ будет искать сущность
        /// </summary>
        private readonly float _maxDistanceForPlayer;
        /// <summary>
        /// Частота вероятности сработки задачи
        /// </summary>
        private readonly float _probability;

        /// <summary>
        /// Задача убегать от страха, от игрока
        /// </summary>
        public EntityAIRunFromFear(EntityMob entity, float distance, float probability = .2f, float speed = 1f) 
            : base(entity, speed)
        {
            _maxDistanceForPlayer = distance;
            _probability = probability;
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute()
        {
            if (_entity.Tracker != null && _entity.Tracker.ClosestPlayer != null
                && _entity.Tracker.ClosestPlayer.IsSurvival()
                && _entity.Tracker.DistantionPlayer < _maxDistanceForPlayer)
            {
                float probability = _probability;
                if (_entity.Tracker.ClosestPlayer.IsSneaking()
                    && _entity.Tracker.DistantionPlayer > _maxDistanceForPlayer * .5f)
                {
                    probability *= .125f;
                }
                if (Rnd.NextFloat() < probability)
                {
                    int bxz = 12;
                    int by = 5;

                    for (int i = 0; i < 8; i++)
                    {
                        _xPosition = _entity.PosX + Rnd.Next(bxz) - Rnd.Next(bxz);
                        _yPosition = _entity.PosY + Rnd.Next(by) - Rnd.Next(by);
                        _zPosition = _entity.PosZ + Rnd.Next(bxz) - Rnd.Next(bxz);

                        if (Glm.Distance(_entity.Tracker.ClosestPlayer.GetPositionVec(),
                            new Vector3(_xPosition, _yPosition, _zPosition)) - 5f > _entity.Tracker.DistantionPlayer)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Обновляет задачу
        /// </summary>
        public override void UpdateTask()
        {
            _entity.MoveHelper.SetSprinting();
            if (Rnd.NextFloat() < .02f)
            {
                _entity.MoveHelper.SetJumping();
            }
        }
    }
}
