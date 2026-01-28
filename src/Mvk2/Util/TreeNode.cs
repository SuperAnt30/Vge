using System.Collections.Generic;
using Vge.World.Block;

namespace Mvk2.Util
{
    /// <summary>
    /// Узел дерева
    /// </summary>
    public class TreeNode
    {
        /// <summary>
        /// Дети
        /// </summary>
        public readonly List<TreeNode> Children = new List<TreeNode>();

        /// <summary>
        /// Блочный узел
        /// </summary>
        public readonly BlockPosLoc PosLoc;

        public TreeNode(int x, int y, int z, int id)
            => PosLoc = new BlockPosLoc(x, y, z, id);

        public TreeNode(BlockPosLoc blockNode) => PosLoc = blockNode;

        public override string ToString() => PosLoc + " C:" + Children.Count;
    }
}
