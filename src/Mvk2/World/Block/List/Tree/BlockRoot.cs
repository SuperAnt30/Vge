using Mvk2.World.BlockEntity.List;
using System.Runtime.CompilerServices;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок древесного корня
    /// </summary>
    public class BlockRoot : BlockBase
    {
        /***
         * Met 0001 0000 0011
         * Для Root корень 2 bit форма 1 bit игрок 
         * 0 - вверх, генерация
         * 1/2 - бок, генерация
         * +256 - игрок
         */

        public BlockRoot(IMaterial material) : base(material) { }

        /// <summary>
        /// Массив сторон прямоугольных форм для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override QuadSide[] GetQuads(int met, int xb, int zb) => _quads[met & 0xF];

        /// <summary>
        /// Действие блока после его удаления
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void OnBreakBlock(WorldBase world, ChunkServer chunk,
            BlockPos blockPos, BlockState stateOld, BlockState stateNew)
        {
            if (stateOld.Met < 256) // только для сгенерированных блоков
            {
                int y = blockPos.Y + 1;
                int x = blockPos.X & 15;
                int z = blockPos.Z & 15;
                int xz = z << 4 | x;
                while (chunk.GetBlockStateNotCheckLight(xz, y).Id == IndexBlock)
                {
                    y++;
                }
                blockPos.Y = y;
                if (chunk.GetBlockEntity(blockPos) is BlockEntityTree)
                {
                    // Запускаем тикер чтоб древо отработало
                    chunk.SetBlockTick(x, y, z, false, 3);
                }
            }
        }
    }
}
