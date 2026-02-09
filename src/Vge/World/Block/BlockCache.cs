using System.Runtime.CompilerServices;

namespace Vge.World.Block
{
    /// <summary>
    /// Структура кэша блока, для генерации будущих элементов
    /// </summary>
    public struct BlockCache
    {
        /// <summary>
        /// Глобальная координата
        /// </summary>
        public BlockPos Position;
        /// <summary>
        /// ID блока
        /// </summary>
        public int Id;
        /// <summary>
        /// Met данные блока
        /// </summary>
        public int Met;
        /// <summary>
        /// Флаг, доп параметр
        /// 0 = всегда меняем
        /// 1 = меняем если не воздух
        /// 2 = меняем если только воздух
        /// </summary>
        public byte Flag;
        /// <summary>
        /// Индекс родителя из массива. Если нет родителя = -1
        /// </summary>
        public int ParentIndex;

        public BlockCache(int x, int y, int z, int id, int met = 0)
        {
            Position = new BlockPos(x, y, z);
            Id = id;
            Met = met;
            Flag = 0;
            ParentIndex = -1;
        }
        public BlockCache(BlockPos blockPos, int id, int met = 0)
        {
            Position = blockPos;
            Id = id;
            Met = met;
            Flag = 0;
            ParentIndex = -1;
        }
        public BlockCache(BlockPosLoc posLoc, int met = 0)
        {
            Position = posLoc.GetBlockPos();
            Id = posLoc.Id;
            Met = met;
            Flag = 0;
            ParentIndex = -1;
        }

        public BlockCache CopyNotFlag()
            => new BlockCache(Position, Id, Met);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetIdMet(int id, int met)
        {
            Id = id;
            Met = met;
        }

        /// <summary>
        /// Получить данные блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockState GetBlockState() => new BlockState(Id) { Met = Met };

        public override string ToString() 
            => string.Format("{0} Id:{1} Met:{2} F:{3} P:{4}", Position, Id, Met, Flag, ParentIndex);
    }
}
