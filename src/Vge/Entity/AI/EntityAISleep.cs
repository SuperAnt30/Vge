using Vge.Network.Packets.Server;
using WinGL.Util;

namespace Vge.Entity.AI
{
    /// <summary>
    /// Задача спать, если ночь
    /// </summary>
    public class EntityAISleep : EntityAIBase
    {
        /// <summary>
        /// Частота вероятности сработки задачи
        /// </summary>
        private readonly float _probability;

        /// <summary>
        /// Действие сна
        /// </summary>
        private bool _actionSleep;

        /// <summary>
        /// Задача спать, если ночь
        /// </summary>
        public EntityAISleep(EntityMob entity, float probability = .1f) : base(entity, 7)
        {
            _probability = probability;
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute()
        {
            if (!_entity.GetWorldServer().Settings.Calendar.IsDayTime() 
                && !_entity.IsSleep())
            {
                // Ночь, и сущность ещё не спит. Надо ложится спать
                if (_entity.GetAITarget() == null && Rnd.NextFloat() < _probability)
                {
                    _actionSleep = true;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Обновляет задачу
        /// </summary>
        public override void UpdateTask()
        {
            if (_actionSleep)
            {
              //  bool actionSleeped = true;
                if (_entity.GetWorldServer().Settings.Calendar.IsDayTime())
                {
                    // День, надо просыпаться
                    if (_entity.IsSleep() && Rnd.NextFloat() < _probability)
                    {
                        _actionSleep = false;
                    }
                }
                else
                {
                    // Ночь, сущность спит, надо пробудить
                    if (_entity.GetAITarget() != null)
                    {
                        _actionSleep = false;
                    }
                }

                if (_actionSleep)
                {
                    if (!_entity.Physics.IsMotionChange)
                    {
                        if (!_entity.IsSleep())
                        {
                            // Укладываем спать
                            _entity.SetSleep(true);
                            _entity.GetWorldServer().Tracker.SendToAllTrackingEntity(_entity.Id,
                                    new PacketS0BAnimation(_entity.Id, PacketS0BAnimation.EnumAction.EyeClose));
                            _entity.LookHelper.SetLookPitch(-Glm.Pi30, Glm.Pi10);
                        }
                        // Положение сидя
                        _entity.MoveHelper.SetSneak(false);
                    }
                }
                else
                {
                    if (_entity.IsSleep())
                    {
                        // Просыпаемся
                        _entity.SetSleep(false);
                        _entity.GetWorldServer().Tracker.SendToAllTrackingEntity(_entity.Id,
                                new PacketS0BAnimation(_entity.Id, PacketS0BAnimation.EnumAction.EyeOpen));
                        _entity.LookHelper.SetLookPitch(0, Glm.Pi10);
                    }
                }
            }
        }

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        public override bool ContinueExecuting() => _actionSleep;
    }
}
