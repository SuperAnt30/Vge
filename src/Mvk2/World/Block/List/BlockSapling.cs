using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок саженца
    /// </summary>
    public class BlockSapling : BlockBase
    {
        /// <summary>
        /// Индекс элемента для генерации
        /// </summary>
        private readonly int _elementId;

        public BlockSapling(int elementId) => _elementId = elementId; 

        /// <summary>
        /// Массив сторон прямоугольных форм для рендера
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public override QuadSide[] GetQuads(int met, int xb, int zb)
        //{
        //    return _quads[met];
        //}

        // <summary>
        /// Обновить блок в такте
        /// </summary>
        public override void UpdateTick(WorldServer world, ChunkServer chunk,
            BlockPos blockPos, BlockState blockState, Rand random)
        {
            //world.SetBlockState(pos, new BlockState() { Id = IndexBlock, Met = met }, 46);
            //if (isAddLiquid)
            //world.SetBlockAddLiquidTick(pos, _tickRate);
            //else
            //world.SetBlockTick(pos, _tickRate);

            // TileEntity
            // Получить генератор дерева
            //IElementUpdate element = world.Settings.BlocksElement.Element(_elementId);
            //if (element != null)
            //{
            //    element.Update(world, blockPos);
            //}
            //else
            //{
            //    world.SetBlockToAir(blockPos);
            //}


            //if (blockState.lightSky > 9 && random.Next(14) == 0 && world.IsAreaLoaded(blockPos, 12))
            //{
           // _featureTree.GenefateTree(world, random, blockPos);
            //}
        }
    }
}
