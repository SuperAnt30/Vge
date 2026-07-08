using WinGL.Util;

namespace Vge.Entity.AI
{
    /// <summary>
    /// Задача смотреть без дела
    /// </summary>
    public class EntityAILookIdle : EntityAIBase
    {
        private int _time;
        private float _lookX;
        private float _lookZ;
        /// <summary>
        /// Частота вероятности сработки задачи
        /// </summary>
        private readonly float _probability;

        /// <summary>
        /// Задача смотреть без дела
        /// </summary>
        public EntityAILookIdle(EntityMob entity, float probability = .02f)
            : base(entity, 2)
        {
            _probability = probability;
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute() => Rnd.NextFloat() < _probability;

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        public override bool ContinueExecuting() => _time >= 0;

        /// <summary>
        /// Выполните разовую задачу или начните выполнять непрерывную задачу
        /// </summary>
        public override void StartExecuting()
        {
            float angle = Glm.Pi360 * Rnd.NextFloat();
            _lookX = Glm.Cos(angle);
            _lookZ = Glm.Sin(angle);
            _time = 20 + Rnd.Next(20);
        }

        /// <summary>
        /// Обновляет задачу
        /// </summary>
        public override void UpdateTask()
        {
            _time--;
            _entity.LookHelper.SetLookPosition(
                _entity.PosX + _lookX, 
                _entity.PosY + _entity.GetEyeHeight(), 
                _entity.PosZ + _lookZ,
                Glm.Pi10, Glm.Pi10);
        }
    }
}
