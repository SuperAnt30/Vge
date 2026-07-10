namespace Vge.Entity.AI
{
    /// <summary>
    /// Задача следовать за одним из своих
    /// </summary>
    public class EntityAIFollowOneYour : EntityAIFollowYour
    {
        /// <summary>
        /// Задача следовать за одним из своих
        /// </summary>
        public EntityAIFollowOneYour(EntityMob entity, float probability = .008f, float speed = 1f, 
            int destination = 12) : base(entity, probability, speed, destination) { }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute()
        {
            if (_entity.EntityAge < 200 && Rnd.NextFloat() < _probability)
            {
                int count = _EntityFromSectorType();
                if (count > 1)
                {
                    EntityBase entity = _collision.ListEntity[Rnd.Next(count)];
                    if (entity.Id != _entity.Id)
                    {
                        _xPosition = entity.PosX;
                        _yPosition = entity.PosY;
                        _zPosition = entity.PosZ;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
