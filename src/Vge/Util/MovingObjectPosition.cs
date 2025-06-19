using System.Runtime.CompilerServices;
using Vge.Entity;
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
        /// Объект сущьности
        /// </summary>
        public EntityBase Entity { get; private set; }
        /// <summary>
        /// Объект данных блока
        /// </summary>
        public BlockState Block { get; private set; } = new BlockState().Empty();
        /// <summary>
        /// Позиция блока
        /// </summary>
        public BlockPos BlockPosition { get; private set; }
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
        public Vector3i Norm { get; private set; }
        /// <summary>
        /// Координата куда попал луч в глобальных координатах по блоку
        /// </summary>
        public Vector3 RayHit { get; private set; }
        /// <summary>
        /// Точка куда устанавливаем блок (параметр с RayCast)
        /// значение в пределах 0..1, образно фиксируем пиксел клика на стороне
        /// </summary>
        public Vector3 Facing { get; private set; }
        /// <summary>
        /// Сторона блока куда смотрит луч, нельзя по умолчанию All, надо строго из 6 сторон
        /// </summary>
        public Pole Side { get; private set; } = Pole.Up;

        private MovingObjectType _type = MovingObjectType.None;

        /// <summary>
        /// Копировать объект в новый
        /// </summary>
        public MovingObjectPosition Copy()
        {
            MovingObjectPosition movingObject = new MovingObjectPosition();
            movingObject.Copy(this);
            return movingObject;
        }
        /// <summary>
        /// Скопировать данные объекта movingObject в текущий
        /// </summary>
        public void Copy(MovingObjectPosition movingObject)
        {
            _type = movingObject._type;
            if (_type != MovingObjectType.None)
            {
                RayHit = movingObject.RayHit;
                Side = movingObject.Side;
                if (_type == MovingObjectType.Entity)
                {
                    Entity = movingObject.Entity;
                }
                else
                {
                    Block = movingObject.Block;
                    BlockPosition = movingObject.BlockPosition;
                    Facing = movingObject.Facing;
                    Norm = movingObject.Norm;
                }
            }
        }

        /// <summary>
        /// Очистить, ни кого не выбрали
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (_type == MovingObjectType.Entity)
            {
                Entity = null;
            }
            _type = MovingObjectType.None;
        }

        /// <summary>
        /// Попадаем в сущность
        /// </summary>
        /// <param name="entity">Объект сущности</param>
        /// <param name="intersection">Точка пересечения</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ObjectEntity(EntityBase entity, PointIntersection intersection)
        {
            Entity = entity;
            RayHit = intersection.RayHit;
            Side = intersection.Side;
            _type = MovingObjectType.Entity;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ObjectBlock(BlockState blockState, BlockPos pos, Pole side, Vector3 facing, Vector3i norm, Vector3 rayHit)
        {
            if (_type == MovingObjectType.Entity)
            {
                Entity = null;
            }
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlock() => _type == MovingObjectType.Block;
        /// <summary>
        /// Попадает ли луч на сущность
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEntity() => _type == MovingObjectType.Entity;
        /// <summary>
        /// Лучь попадает или в сущность или в блок
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCollision() => _type != MovingObjectType.None;

        /// <summary>
        /// Задать попадание по жидкому блоку
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                else if (moving.IsEntity() && IsEntity())
                {
                    return moving.Entity.Id == Entity.Id;
                }
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
