using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World.Block;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок дерева ветка
    /// </summary>
    public class BlockBranch : BlockTree
    {
        /***
         * Met
         * 
         * 0011 0000 0111
         * Для Branch Ветвь 3 bit форма 1 bit игрок 1 bit тикер
         * 0 - вверх, генерация
         * 1/2 - бок, генерация
         * 3-6 - вверх, генерация, смещение к краю +X -X -Z +Z
         * +256 - игрок
         * +512 - тикер
         * 
         */

        public BlockBranch(MaterialBase material, TypeTree type, int indexGen) 
            : base(material, type, indexGen) { }

        /// <summary>
        /// Передать список  ограничительных рамок блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met)
        {
            met = met & 7;

            if (met == 1) return new AxisAlignedBB[] { new AxisAlignedBB(pos.X, pos.Y + .25f, pos.Z + .25f, 
                    pos.X + 1, pos.Y + .75f, pos.Z + .75f) };
            if (met == 2) return new AxisAlignedBB[] { new AxisAlignedBB(pos.X + .25f, pos.Y + .25f, pos.Z,
                    pos.X + .75f, pos.Y + .75f, pos.Z + 1) };

            if (met == 3) return new AxisAlignedBB[] { new AxisAlignedBB(pos.X + .5f, pos.Y, pos.Z + .25f,
                pos.X + .1f, pos.Y + 1, pos.Z + .75f) };
            if (met == 4) return new AxisAlignedBB[] { new AxisAlignedBB(pos.X, pos.Y, pos.Z + .25f,
                pos.X + .5f, pos.Y + 1, pos.Z + .75f) };
            if (met == 5) return new AxisAlignedBB[] { new AxisAlignedBB(pos.X + .25f, pos.Y, pos.Z,
                pos.X + .75f, pos.Y + 1, pos.Z + .5f) };
            if (met == 6) return new AxisAlignedBB[] { new AxisAlignedBB(pos.X + .25f, pos.Y, pos.Z + .5f,
                pos.X + .75f, pos.Y + 1, pos.Z + 1f) };

            return new AxisAlignedBB[] { new AxisAlignedBB(pos.X + .25f, pos.Y, pos.Z + .25f, 
                pos.X + .75f, pos.Y + 1, pos.Z + .75f) };
        }
    }
}
