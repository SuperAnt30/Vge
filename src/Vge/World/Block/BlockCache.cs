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
        /// Тело, доп параметр
        /// </summary>
        public int Flag;
        /// <summary>
        /// Тикащий блок, если значение не равно 0, и оно означает время тика, без приоритета
        /// </summary>
        public uint Tick;

        public BlockCache(BlockPos blockPos, int id, int flag = 0, uint tick = 0)
        {
            Position = blockPos;
            Id = id;
            Flag = flag;
            Tick = tick;
        }

        /// <summary>
        /// Получить данные блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockState GetBlockState() => new BlockState(Id);

        public override string ToString() 
            => string.Format("{0} Id:{1} F:{2}", Position, Id, Flag);
    }
}
