using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;
using Vge.World.Сalendar;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок плода
    /// </summary>
    public class BlockFetus : BlockBase
    {
        public BlockFetus(IMaterial material) : base(material) { }

        /// <summary>
        /// ID блок листвы текущего дерева
        /// </summary>
        protected int _idLeaves;

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldServer world, ChunkServer chunk,
            BlockPos blockPos, BlockState blockState, BlockBase neighborBlock)
        {
            base.NeighborBlockChange(world, chunk, blockPos, blockState, neighborBlock);
            if (!CanBlockStay(world, blockPos, blockState.Met))
            {
                //DropBlockAsItem(worldIn, blockPos, state);
                world.SetBlockToAir(blockPos, 47); // 1 2 4 8 32 без звука но с частичками
            }
        }

        /// <summary> 
        /// Проверка установи блока, можно ли его установить тут
        /// </summary>
        public override bool CanBlockStay(WorldBase world, BlockPos blockPos, int met = 0)
        {
            ChunkBase chunk = world.GetChunk(blockPos);
            if (chunk != null)
            {
                return chunk.GetBlockState(blockPos.OffsetUp()).Id == _idLeaves;
            }
            return false;
        }

        /// <summary>
        /// Случайный эффект блока, для сервера
        /// </summary>
        public override void RandomTick(WorldServer world, ChunkServer chunk,
            BlockPos blockPos, BlockState blockState, Rand random)
        {
            EnumTimeYear timeYear = world.Settings.Calendar.TimeYear;
            if (timeYear == EnumTimeYear.Winter || timeYear == EnumTimeYear.Spring)
            {
                // Удаляем
                world.SetBlockToAir(blockPos);
            }
        }
    }
}
