using Mvk2.World.BlockEntity;
using Mvk2.World.BlockEntity.List;
using Mvk2.World.Gen;
using Mvk2.World.Gen.Feature;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.BlockEntity;
using Vge.World.Chunk;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок дерева: бревно, ветка или саженец
    /// </summary>
    public class BlockTree : BlockBase
    {
        /***
         * Met
         * Для Log Бревно
         * 0 - вверх, генерация
         * 1/2 - бок, генерация
         * 3 - вверх, генерация, нижний блок для пня и тика
         * 
         * 4 - вверх, игрок
         * 5/6 - бок, игрок
         * 
         * Для Branch Ветвь
         * 0 - вверх, генерация
         * 1/2 - бок, генерация
         * 3-6 - вверх, генерация, смещение к краю
         * 
         * Для Sapling Саженец
         * 0 - вверх
         * 
         * Для Root корень
         * 0 - вверх, генерация
         * 1/2 - бок, генерация
         * 
         */

        /// <summary>
        /// Тип блока дерева
        /// </summary>
        public enum TypeTree
        {
            /// <summary>
            /// Саженец
            /// </summary>
            Sapling,
            /// <summary>
            /// Бревно
            /// </summary>
            Log,
            /// <summary>
            /// Ветвь
            /// </summary>
            Branch,
            /// <summary>
            /// Корень
            /// </summary>
            Root
        }

        /// <summary>
        /// Тип блока древесины
        /// </summary>
        public readonly TypeTree Type;

        /// <summary>
        /// ID блок саженца текущего дерева
        /// </summary>
        public int IdSapling { get; protected set; }
        /// <summary>
        /// ID блок бревна текущего дерева
        /// </summary>
        public int IdLog { get; protected set; }
        /// <summary>
        /// ID блок ветки текущего дерева
        /// </summary>
        public int IdBranch { get; protected set; }
        /// <summary>
        /// ID блок листвы текущего дерева
        /// </summary>
        public int IdLeaves { get; protected set; }
        /// <summary>
        /// ID блок плода текущего дерева
        /// </summary>
        public int IdFetus { get; protected set; }

        public BlockTree(TypeTree type)
        {
            Type = type;
        }

        /// <summary>
        /// Массив сторон прямоугольных форм для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override QuadSide[] GetQuads(int met, int xb, int zb) => _quads[met & 0xFF];

        /// <summary>
        /// Обновить блок в такте
        /// </summary>
        public override void UpdateTick(WorldServer world, ChunkServer chunk,
            BlockPos blockPos, BlockState blockState, Rand rand)
        {
            FeatureTree genTree = _GetFeatureTree(world);
            if (genTree != null)
            {
                if (Type == TypeTree.Sapling)
                {
                    // Саженец
                    genTree.StepSapling(world, chunk, blockPos, rand);
                    //{
                    //    //BlockEntityTree blockEntity = _CreateBlockEntity(world);

                    //    //BlockState blockStateNew = new BlockState(IdBranch);
                    //    //blockEntity.SetBlockPosition(blockStateNew, blockPos);
                    //    //blockEntity.SetBeginBlock(blockPos, blockStateNew);
                    //    //world.SetBlockState(blockPos, blockStateNew, 46);
                    //    //chunk.SetBlockEntity(blockEntity);
                    //    //blockEntity.SetTick(chunk, 60);
                    //}
                }
                else
                {
                    // Обновление блока только в основание может быть, типа пня
                    //(chunk.GetBlockEntity(blockPos) as BlockEntityTree)?.UpdateTick(world, 
                    //    chunk, rand, genTree);
                }
            }
        }

        /// <summary>
        /// Создать блок сущности для дерева
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual BlockEntityTree _CreateBlockEntity(WorldServer world) =>
            Ce.BlocksEntity.CreateEntityServer(BlocksEntityRegMvk.IdTree, world) as BlockEntityTree;

        /// <summary>
        /// Получить объект генерации дерева
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual FeatureTree _GetFeatureTree(WorldServer world) => null;

        /// <summary>
        /// Действие блока после его удаления
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void OnBreakBlock(WorldServer world, ChunkServer chunk, 
            BlockPos blockPos, BlockState stateOld, BlockState stateNew)
        {
            if (Type != TypeTree.Sapling && Type != TypeTree.Root)
            {

                // TODO:: тут надо игнорить при смене Sapling на Branch и Branch на Log
                //if (Type == TypeTree.Log)
                {
                    _RemoveBlockTreeInChunk(world, chunk, blockPos);
                    for (int i = 0; i < 8; i++)
                    {
                        _RemoveBlockTreeInChunk(world,
                            world.GetChunkServer(chunk.X + Ce.AreaOne8X[i], chunk.Y + Ce.AreaOne8Y[i]), blockPos);
                    }
                }
            }
        }

        /// <summary>
        /// Удаление блока дерева в чанке
        /// </summary>
        private void _RemoveBlockTreeInChunk(WorldServer world, ChunkServer chunk, BlockPos blockPos)
        {
            if (chunk.GetBlockEntityCount() > 0)
            {
                foreach (KeyValuePair<int, BlockEntityBase> item in chunk.MapBlocksEntity)// TODO::2026-02-02 Ошибка может быть, надо заменить
                {
                    if (item.Value is BlockEntityTree blockEntityTree
                        && blockEntityTree.IsAABB(blockPos))
                    {     
                        // Это блок возможно принадлежит этому дереву. Откусить
                        blockEntityTree.RemoveBlock(world, chunk, blockPos);
                    }
                }
            }
        }
    }
}
