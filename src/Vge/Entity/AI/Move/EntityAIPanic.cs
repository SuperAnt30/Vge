namespace Vge.Entity.AI
{
    /// <summary>
    /// Задача паника
    /// </summary>
    public class EntityAIPanic : EntityAIBaseMove
    {
        /// <summary>
        /// Задача паника
        /// </summary>
        public EntityAIPanic(EntityMob entity, float speed = 1f) : base(entity, speed) { }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute()
        {
            if ((_entity.InFire() || _entity.GetAITarget() != null) && _entity.EntityAge < 200)
            {
                int bxz = 8;
                int by = 4;

                _xPosition = _entity.PosX + Rnd.Next(bxz) - Rnd.Next(bxz);
                _yPosition = _entity.PosY + Rnd.Next(by) - Rnd.Next(by);
                _zPosition = _entity.PosZ + Rnd.Next(bxz) - Rnd.Next(bxz);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Обновляет задачу
        /// </summary>
        public override void UpdateTask()
        {
            //if (Rnd.NextFloat() < .4f)
            _entity.MoveHelper.SetSprinting();
            if (Rnd.NextFloat() < .02f)
            {
                _entity.MoveHelper.SetJumping();
            }
        }
    }
}
