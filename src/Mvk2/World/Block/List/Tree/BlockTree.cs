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
         * 
         * 0011 0000 0011
         * Для Log Бревно 2 bit форма 1 bit игрок 1 bit тикер
         * 0 - вверх, генерация
         * 1/2 - бок, генерация
         * 3 - вверх, генерация, нижний блок для пня и тика
         * 4 - вверх, игрок
         * 5/6 - бок, игрок
         * +256 - игрок
         * +512 - тикер
         * 
         * 0011 0000 0111
         * Для Branch Ветвь 3 bit форма 1 bit игрок 1 bit тикер
         * 0 - вверх, генерация
         * 1/2 - бок, генерация
         * 3-6 - вверх, генерация, смещение к краю +X -X -Z +Z
         * +256 - игрок
         * +512 - тикер
         * 
         * Для Sapling Саженец
         * 0 - вверх
         * 
         * 0001 0000 0011
         * Для Root корень 2 bit форма 1 bit игрок 
         * 0 - вверх, генерация
         * 1/2 - бок, генерация
         * +256 - игрок
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

        public BlockTree(IMaterial material, TypeTree type) : base(material) => Type = type;

        /// <summary>
        /// Массив сторон прямоугольных форм для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override QuadSide[] GetQuads(int met, int xb, int zb) => _quads[met & 0xF];

        /// <summary>
        /// Обновить блок в такте
        /// </summary>
        public override void UpdateTick(WorldServer world, ChunkServer chunk,
            BlockPos blockPos, BlockState blockState, Rand rand)
        {
            if (Type == TypeTree.Sapling)
            {
                // Саженец
                _GetFeatureTree(world)?.StepSapling(world, chunk, blockPos, rand);
            }
            else if (blockState.Met >= 512)
            {
                // Другие шаги дерева
                _GetFeatureTree(world)?.StepsOther(world, chunk, blockPos, rand);
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
            if (Type == TypeTree.Log || Type == TypeTree.Branch)
            {
                // Список всех блок сущностей в квадрате 3*3 чанка
                List<BlockEntityBase> blocksEntity = world.GetBlocksEntity3x3(chunk);

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
