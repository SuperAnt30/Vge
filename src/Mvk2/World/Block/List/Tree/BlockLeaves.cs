using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок листвы
    /// </summary>
    public class BlockLeaves : BlockBase
    {
        /***
         * Met 0001 0000 1111
         * Для Leaves листва 4 bit форма 1 bit молодой?
         * 0 - вверх
         * 1 - низ
         * 2-5 бок
         * 6 - вверх 2
         * 7 - низ 2
         * 8-11 бок 2
         * 
         * +256 - Можно вешать плод
         * +512 - надо удалять
         */

        /// <summary>
        /// ID блок бревна текущего дерева
        /// </summary>
        protected int _idLog;
        /// <summary>
        /// ID блок ветки текущего дерева
        /// </summary>
        protected int _idBranch;
        /// <summary>
        /// ID блок плода текущего дерева
        /// </summary>
        protected int _idFetus;

        public BlockLeaves(IMaterial material) : base(material) { }

        /// <summary>
        /// Массив сторон прямоугольных форм для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override QuadSide[] GetQuads(int met, int xb, int zb) => _quads[met & 0xF];

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldServer world, ChunkServer chunk,
            BlockPos blockPos, BlockState blockState, BlockBase neighborBlock)
        {
            base.NeighborBlockChange(world, chunk, blockPos, blockState, neighborBlock);
            if (!CanBlockStay(world, chunk, blockPos, blockState.Met))
            {
                //DropBlockAsItem(worldIn, blockPos, state);
                world.SetBlockToAir(blockPos, 47); // 1 2 4 8 32 без звука но с частичками
            }
        }

        /// <summary> 
        /// Проверка установи блока, можно ли его установить тут
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool CanBlockStay(WorldServer world, ChunkServer chunk, 
            BlockPos blockPos, int met = 0)
        {
            if (met > 255) met = met & 0xF;
            if (met > 5) met -= 6;
            int id = world.GetBlockState(blockPos.OffsetReversal(met)).Id;
            return id == _idLog || id == _idBranch;
        }

        /// <summary>
        /// Случайный эффект блока, для сервера
        /// </summary>
        public override void RandomTick(WorldServer world, ChunkServer chunk,
            BlockPos blockPos, BlockState blockState, Rand random)
        {
            int met = blockState.Met;
            if ((met & 1 << 9) != 0) // 512
            {
                if (random.Next(3) == 0)
                {
                    // Удаляем
                    world.SetBlockToAir(blockPos);
                }
            }
            else if ((met & 1 << 8) != 0) // 256
            {
                if (random.Next(10) == 0)
                {
                    // Вешаем плод
                    if (chunk.GetBlockStateNotCheck(blockPos.OffsetDown()).Id == 0)
                    {
                        chunk.SetBlockState(blockPos.OffsetDown(), new BlockState(_idFetus), 44);
                    }
                    chunk.SetBlockStateMet(blockPos, met - 256);
                }
            }
        }
    }
}
