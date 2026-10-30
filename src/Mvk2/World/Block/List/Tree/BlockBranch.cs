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
            : base(material, type, indexGen)
        {
            _fullCube = new AxisAlignedBB[][] {
                new AxisAlignedBB[] { new AxisAlignedBB(.25f, 0, .25f, .75f, 1, .75f) }, // 0
                new AxisAlignedBB[] { new AxisAlignedBB(0, .25f, .25f, 1, .75f, .75f) }, // 1
                new AxisAlignedBB[] { new AxisAlignedBB(.25f, .25f, 0, .75f, .75f, 1) }, // 2
                new AxisAlignedBB[] { new AxisAlignedBB(.5f, 0, .25f, 1, 1, .75f) }, // 3
                new AxisAlignedBB[] { new AxisAlignedBB(0, 0, .25f, .5f, 1, .75f) }, // 4
                new AxisAlignedBB[] { new AxisAlignedBB(.25f, 0, 0, .75f, 1, .5f) }, // 5
                new AxisAlignedBB[] { new AxisAlignedBB(.25f, 0, .5f, .75f, 1, 1) } // 6
            };
        }

        /// <summary>
        /// Локальные рамки коллизии для метаданных
        /// </summary>
        public override AxisAlignedBB[] _GetCollisionBoxesLocal(int met) => _fullCube[met & 7];
    }
}
