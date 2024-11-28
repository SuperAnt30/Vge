using Vge.World.Block;
using WinGL.Util;

namespace Vge.Util
{
    /// <summary>
    /// Объект отвечающий какой объект попадает под луч
    /// </summary>
    public class MovingObjectPosition
    {
        /// <summary>
        /// Объект данных блока
        /// </summary>
        public readonly BlockState Block;
        /// <summary>
        /// Позиция блока
        /// </summary>
        public readonly BlockPos BlockPosition;
        /// <summary>
        /// Объект данных житкого блока
        /// </summary>
        public int IdBlockLiquid { get; private set; }
        /// <summary>
        /// Позиция житкого блока
        /// </summary>
        public BlockPos BlockLiquidPosition { get; private set; }
        /// <summary>
        /// Имеется ли попадание по жидкому блоку
        /// </summary>
        public bool IsLiquid { get; private set; } = false;
        /// <summary>
        /// Нормаль попадания по блоку
        /// </summary>
        public readonly Vector3i Norm;
        /// <summary>
        /// Координата куда попал луч в глобальных координатах по блоку
        /// </summary>
        public readonly Vector3 RayHit;
        /// <summary>
        /// Точка куда устанавливаем блок (параметр с RayCast)
        /// значение в пределах 0..1, образно фиксируем пиксел клика на стороне
        /// </summary>
        public readonly Vector3 Facing;
        /// <summary>
        /// Сторона блока куда смотрит луч, нельзя по умолчанию All, надо строго из 6 сторон
        /// </summary>
        public readonly Pole Side = Pole.Up;

        private readonly MovingObjectType _type = MovingObjectType.None;

        /// <summary>
        /// Нет попадания
        /// </summary>
        public MovingObjectPosition() => Block = new BlockState().Empty();

        /// <summary>
        /// Проверка вектора, с какой стороны попали, точка попадания
        /// </summary>
        /// <param name="vec">точка попадания</param>
        /// <param name="side">с какой стороны попали</param>
        public MovingObjectPosition(Vector3 vec, Pole side)
        {
            // TODO::2024-11-28 Почему блок?! MovingObjectType.Block
            _type = MovingObjectType.Block;
            RayHit = vec;
            Side = side;
        }

        /// <summary>
        /// Попадаем в блок
        /// </summary>
        /// <param name="blockState">блок</param>
        /// <param name="pos">позиция блока</param>
        /// <param name="side">Сторона блока куда смотрит луч</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        /// <param name="norm">Нормаль попадания</param>
        /// <param name="rayHit">Координата куда попал луч</param>
        public MovingObjectPosition(BlockState blockState, BlockPos pos, Pole side, Vector3 facing, Vector3i norm, Vector3 rayHit)
        {
            Block = blockState;
            BlockPosition = pos;
            Facing = facing;
            Side = side;
            RayHit = rayHit;
            Norm = norm;
            _type = MovingObjectType.Block;
        }

        /// <summary>
        /// Попадает ли луч на блок
        /// </summary>
        public bool IsBlock() => _type == MovingObjectType.Block;

        /// <summary>
        /// Попадает ли луч на сущность
        /// </summary>
        public bool IsEntity() => _type == MovingObjectType.Entity;

        //public bool IsCollision() => _type != MovingObjectType.None;


        /// <summary>
        /// Задать попадание по жидкому блоку
        /// </summary>
        public void SetLiquid(int idBlock, BlockPos blockPos)
        {
            IsLiquid = true;
            IdBlockLiquid = idBlock;
            BlockLiquidPosition = blockPos;
        }

        /// <summary>
        /// Тип объекта
        /// </summary>
        private enum MovingObjectType
        {
            None = 0,
            Block = 1,
            Entity = 2
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is MovingObjectPosition moving)
            {
                if (moving.IsBlock() && IsBlock())
                {
                    return moving.BlockPosition.Equals(BlockPosition) && moving.Block.Equals(Block);
                }
                //if (moving.IsEntity() && IsEntity())
                //{
                //    return moving.Entity.Id == Entity.Id;
                //}
                return true;
            }
            return false;
        }

        public override int GetHashCode() 
            => _type.GetHashCode() ^ RayHit.GetHashCode() ^ Block.GetHashCode();

        public override string ToString()
        {
            string str = "";
            //if (_type == MovingObjectType.Entity)
            //{
            //    float h = Entity is EntityLiving ? ((EntityLiving)Entity).GetHealth() : 0;
            //    str = Entity.GetName() + " " + h + " " + Entity.Position;
            //}
            return string.Format("{0} {3} {1} {2} {4}", _type, Side, Facing, str, IsLiquid ? "Li" : "");
        }
    }
}
