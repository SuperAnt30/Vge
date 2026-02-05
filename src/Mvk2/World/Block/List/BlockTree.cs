using Mvk2.World.BlockEntity;
using Mvk2.World.BlockEntity.List;
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
         * 3-6 - вверх, генерация, смещение к краю +X -X -Z +Z
         * 
         * Для Sapling Саженец
         * 0 - вверх
         * 
         * Для Root корень
         * 0 - вверх, генерация
         * 1/2 - бок, генерация
         * 
         * Для Leaves листва
         * 0 - вверх
         * 1 - низ
         * 2-5 бок
         * 6 - вверх 2
         * 7 - низ 2
         * 8-11 бок 2
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
            Root,
            /// <summary>
            /// Листва
            /// </summary>
            Leaves
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
                }
                else
                {
                    genTree.StepsGrowth(world, chunk, blockPos, rand);
                }
            }
        }

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
            if (Type == TypeTree.Root)
            {
                // Удаление корня, для фиксации его мощи

            }
            else if (Type != TypeTree.Sapling && Type != TypeTree.Leaves)
            {
                //System.Console.WriteLine("OnBreakBlock " + blockPos + " " + stateOld.Id + "->" + stateNew.Id);
                // Список всех блок сущностей в квадрате 3*3 чанка
                List<BlockEntityBase> blocksEntity = new List<BlockEntityBase>();
                chunk.AddRangeBlockEntity(blocksEntity);
                for (int i = 0; i < 8; i++)
                {
                    world.GetChunkServer(chunk.X + Ce.AreaOne8X[i], chunk.Y + Ce.AreaOne8Y[i]).AddRangeBlockEntity(blocksEntity);
                }

                // Пробегаемся и производим удаление
                foreach(BlockEntityBase blockEntity in blocksEntity)
                {
                    if (blockEntity is BlockEntityTree blockEntityTree
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
