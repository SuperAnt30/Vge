using Mvk2.World.Gen;
using Mvk2.World.Gen.Feature;
using System.Runtime.CompilerServices;
using Vge.World;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок дерева берёзы: бревно, ветка или саженец
    /// </summary>
    public class BlockTreeBirch : BlockTree
    {
        public BlockTreeBirch(TypeTree type)  : base(type) { }

        /// <summary>
        /// Дополнительная инициализация блока после инициализации предметов и корректировки id блоков
        /// </summary>
        public override void InitAfterItems()
        {
            IdSapling = BlocksRegMvk.SaplingBirch.IndexBlock;
            IdLog = BlocksRegMvk.LogBirch.IndexBlock;
            IdBranch = BlocksRegMvk.BranchBirch.IndexBlock;
            IdLeaves = BlocksRegMvk.LeavesBirch.IndexBlock;
        }

        /// <summary>
        /// Получить объект генерации дерева
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override FeatureTree _GetFeatureTree(WorldServer world)
            => world.ChunkPrServ.ChunkGenerate is IGenTree genTree ? genTree.Tree.BirchUp
            : (FeatureTree)null;
    }
}
