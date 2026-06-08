using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;
using Vge.World.Сalendar;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок высокой травы
    /// </summary>
    public class BlockTallGrass : BlockBase
    {
        /***
        * Met
        * 0 - один блок высоты
        * 1 - два блока высоты
        * 2 - три блока высоты
        * 3 - четыре блока высоты
        * 4 - пять блока высоты
        */

        public BlockTallGrass(MaterialBase material) : base(material) { }

        /// <summary>
        /// Массив сторон прямоугольных форм для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override QuadSide[] GetQuads(int met, int xb, int zb)
        {
            int i = (xb + zb) & 7;
            if (i > 4) i -= 4;
            if (met == 1) i += 5;
            else if (met == 2) i += 10;
            else if (met == 3) i += 15;
            else if (met == 4) i += 20;
            else if (met == 5) i += 25;

            return _quads[i];
        }

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldServer world, ChunkServer chunk,
            BlockPos blockPos, BlockState blockState, BlockBase neighborBlock)
        {
            base.NeighborBlockChange(world, chunk, blockPos, blockState, neighborBlock);
            if (!CanBlockStay(world, chunk, blockPos))
            {
                // Снизу не подходящий блок для травы
                //DropBlockAsItem(worldIn, blockPos, state);
                world.SetBlockToAir(blockPos);
            }
            else
            {
                // Сверху проверяем невидемые блоки
                int met = blockState.Met;
                if (met > 0)
                {
                    for (int i = 1; i <= met; i++)
                    {
                        if (chunk.GetBlockState(blockPos.OffsetUp(i)).Id 
                            != BlocksRegMvk.GrassNull.IndexBlock)
                        {
                            world.SetBlockStateMet(blockPos, i - 1, true);
                            break;
                        }
                    }
                }
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
                return block.IndexBlock == BlocksRegMvk.TurfLoam.IndexBlock
                    || block.IndexBlock == BlocksRegMvk.Turf.IndexBlock
                    || block.IndexBlock == BlocksRegMvk.Clay.IndexBlock;
            }
            return false;
        }

        /// <summary>
        /// Рост травы
        /// </summary>
        public void GrassGrowth(WorldServer world, ChunkServer chunk, BlockState blockState, BlockPos blockPos)
        {
            if (blockState.Met < 4)
            {
                int met = blockState.Met + 1;
                world.SetBlockStateMet(blockPos, met, true);
                BlockPos pos;
                for (int i = 1; i <= met; i++)
                {
                    pos = blockPos.OffsetUp(i);
                    if (chunk.GetBlockState(pos).Id == 0)
                    {
                        world.SetBlockState(pos, new BlockState(BlocksRegMvk.GrassNull.IndexBlock), 46);
                    }
                }
            }
        }

        /// <summary>
        /// Случайный эффект блока, для сервера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void RandomTick(WorldServer world, ChunkServer chunk,
            BlockPos blockPos, BlockState blockState, Rand random)
        {
            if (blockState.Met < 4 && blockState.LightSky < 10 
                && random.Next(blockState.Met * 20 + 10) == 0) // Чем выше, тем реже ростём
            {
                EnumTimeYear timeYear = world.Settings.Calendar.TimeYear;
                if (timeYear == EnumTimeYear.Spring || timeYear == EnumTimeYear.Summer)
                {
                    GrassGrowth(world, chunk, blockState, blockPos);
                }
            }
        }
    }
}
