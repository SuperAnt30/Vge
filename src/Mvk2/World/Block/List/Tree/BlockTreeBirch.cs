using Mvk2.World.Gen;
using Mvk2.World.Gen.Feature;
using System.Runtime.CompilerServices;
using Vge.World;
using Vge.World.Block;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок дерева берёзы: бревно, ветка или саженец
    /// </summary>
    public class BlockTreeBirch : BlockTree
    {
        public BlockTreeBirch(IMaterial material, TypeTree type)  : base(material, type) { }

        /// <summary>
        /// Получить объект генерации дерева
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override FeatureTree _GetFeatureTree(WorldServer world)
            => world.ChunkPrServ.ChunkGenerate is IGenTree genTree ? genTree.Tree.BirchUp
            : (FeatureTree)null;
    }
}
