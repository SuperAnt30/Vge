using Mvk2.World.Gen;
using Mvk2.World.Gen.Feature;
using System.Runtime.CompilerServices;
using Vge.World;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок дерева дуба: бревно, ветка или саженец
    /// </summary>
    public class BlockTreeOak : BlockTree
    {
        public BlockTreeOak(TypeTree type) : base(type) { }

        /// <summary>
        /// Получить объект генерации дерева
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override FeatureTree _GetFeatureTree(WorldServer world) 
            => world.ChunkPrServ.ChunkGenerate is IGenTree genTree ? genTree.Tree.OakUp 
            : (FeatureTree)null;
    }
}
