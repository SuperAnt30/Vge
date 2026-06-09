using System.Runtime.CompilerServices;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок пустой травы, эмитация для взаимодействия без прорисовки
    /// </summary>
    public class BlockGrassNull : BlockBase
    {
        public BlockGrassNull(MaterialBase material) : base(material) { }

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldServer world, ChunkServer chunk,
            BlockPos blockPos, BlockState blockState, BlockBase neighborBlock)
        {
            base.NeighborBlockChange(world, chunk, blockPos, blockState, neighborBlock);
            if (!CanBlockStay(world, chunk, blockPos))
            {
                // Снизу не подходящий блок
                world.SetBlockToAir(blockPos, 47);
            }
        }

        /// <summary> 
        /// Проверка установи блока, можно ли его установить тут
        /// </summary>
        public override bool CanBlockStay(WorldBase world, ChunkBase chunk, BlockPos blockPos, int met = 0)
        {
            if (chunk == null) chunk = world.GetChunk(blockPos);
            if (chunk != null)
            {
                BlockBase block = chunk.GetBlockState(blockPos.OffsetDown()).GetBlock();
                return block.IndexBlock == BlocksRegMvk.TallGrass.IndexBlock
                    || block.IndexBlock == IndexBlock;
            }
            return false;
        }

        /// <summary>
        /// Действие блока после его удаления
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void OnBreakBlock(WorldBase world, ChunkServer chunk,
            BlockPos blockPos, BlockState stateOld, BlockState stateNew)
        {
            if (!world.IsRemote)
            {
                int yi = 1;
                BlockPos pos;
                while (yi < 5)
                {
                    pos = blockPos.OffsetUp(-yi);
                    int index = chunk.GetBlockState(pos).Id;
                    if (index == 0)
                    {
                        // Тут мы попадаем, когда соседний удалённый блок попался, надо приостановить.
                        break;
                    }
                    else if (index == BlocksRegMvk.TallGrass.IndexBlock)
                    {
                        // Нашли основу высокой травы, меняем мет данные
                        world.SetBlockStateMet(pos, yi - 1, true);
                        break;
                    }
                    yi++;
                }
            }
        }
    }
}
