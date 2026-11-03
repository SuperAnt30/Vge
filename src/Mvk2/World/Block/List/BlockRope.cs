using System;
using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;
using WinGL.Util;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок верёвка
    /// </summary>
    public class BlockRope : BlockBase
    {
        /***
         * Met
         * 
         * 0000 0000 0111
         * 0 - вверх, генерация
         * 1-4 - вверх, генерация, смещение к краю -X +X +Z -Z
         * 
         */

        public BlockRope(MaterialBase material) : base(material)
        {
            _fullCube = new AxisAlignedBB[][] {
                // 6 pix толщина
                //new AxisAlignedBB[] { new AxisAlignedBB(.3125f, 0, .3125f, .6875f, 1, .6875f) }, // 0
                //new AxisAlignedBB[] { new AxisAlignedBB(0, 0, .3125f, .375f, 1, .6875f) }, // 1
                //new AxisAlignedBB[] { new AxisAlignedBB(.625f, 0, .3125f, 1, 1, .6875f) }, // 2
                //new AxisAlignedBB[] { new AxisAlignedBB(.3125f, 0, .625f, .6875f, 1, 1) }, // 3
                //new AxisAlignedBB[] { new AxisAlignedBB(.3125f, 0, 0, .6875f, 1, .375f) }  // 4
                // 4 pix tolobyf
                new AxisAlignedBB[] { new AxisAlignedBB(6, 0, 6, 10, 16, 10) }, // 0
                new AxisAlignedBB[] { new AxisAlignedBB(0, 0, 6, 4, 16, 10) }, // 1
                new AxisAlignedBB[] { new AxisAlignedBB(12, 0, 6, 16, 16, 10) }, // 2
                new AxisAlignedBB[] { new AxisAlignedBB(6, 0, 12, 10, 16, 16) }, // 3
                new AxisAlignedBB[] { new AxisAlignedBB(6, 0, 0, 10, 16, 4) }  // 4
            };
            _InitFullCubeDivide16();
        }

        /// <summary>
        /// Массив сторон прямоугольных форм для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override QuadSide[] GetQuads(int met, int xb, int zb) => _quads[met & 7];

        /// <summary>
        /// Локальные рамки коллизии для метаданных
        /// </summary>
        public override AxisAlignedBB[] _GetCollisionBoxesLocal(int met) => _fullCube[met & 7];

        /// <summary>
        /// Локальные рамки коллизии для метаданных
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Vector3[] _GetOutlineLocal(int met) => _hitbox[met & 7];

        /// <summary>
        /// Действие перед размещеннием блока, для определения метданных
        /// </summary>
        public override BlockState OnBlockPlaced(WorldBase world, BlockPos blockPos, BlockState state,
            Pole side, Vector3 facing, float playerYaw)
        {
            ChunkBase chunk = world.GetChunk(blockPos);
            if (chunk != null && blockPos.Y < chunk.Settings.NumberMaxBlock)
            {
                BlockState blockStateUp = chunk.GetBlockState(blockPos.OffsetUp());
                if (blockStateUp.Id == IndexBlock) return state.NewMet(blockStateUp.Met);
            }
            if (side == Pole.Up || side == Pole.Down) return state.NewMet(0);
            return state.NewMet((int)side - 1);
        }

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
                world.SetBlockToAir(blockPos);
            }
        }

        /// <summary> 
        /// Проверка установи блока, можно ли его установить тут
        /// </summary>
        public override bool CanBlockStay(WorldBase world, ChunkBase chunk, BlockPos blockPos, int met = 0)
        {
            if (chunk == null) chunk = world.GetChunk(blockPos);
            if (chunk != null && blockPos.Y > 0 && blockPos.Y < chunk.Settings.NumberMaxBlock)
            {
                BlockBase blockDown = chunk.GetBlockState(blockPos.OffsetDown()).GetBlock();
                if (blockDown.IsReplaceable || blockDown.IndexBlock == IndexBlock)
                {
                    BlockBase block = chunk.GetBlockState(blockPos.OffsetUp()).GetBlock();
                    if (block.IndexBlock == IndexBlock) return true;
                    if (met == 0) return block.FullBlock;

                    // Проверка соседа
                    block = world.GetBlockState(blockPos.OffsetReversal(met + 1)).GetBlock();
                    return block.FullBlock;
                }
            }
            return false;
        }
    }
}
