using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World.Gen;

namespace Vge.World.Block.List
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

        public BlockSapling(int elementId)
        {
            _elementId = elementId;
        }

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
        public override void UpdateTick(WorldServer world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            //world.SetBlockState(pos, new BlockState() { Id = IndexBlock, Met = met }, 46);
            //if (isAddLiquid)
            //world.SetBlockAddLiquidTick(pos, _tickRate);
            //else
            //world.SetBlockTick(pos, _tickRate);
            // Получить генератор дерева
            IElementGenerator element = world.BlocksGenerate.Element(_elementId);
            if (element != null)
            {
                element.Generation(world, blockPos);
            }
            else
            {
                world.SetBlockToAir(blockPos);
            }
            //if (blockState.lightSky > 9 && random.Next(14) == 0 && world.IsAreaLoaded(blockPos, 12))
            //{
           // _featureTree.GenefateTree(world, random, blockPos);
            //}
        }
    }
}
