using Mvk2.World.Gen;
using Mvk2.World.Gen.Feature;
using System.Runtime.CompilerServices;
using Vge.World;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок листвы берёзы
    /// </summary>
    public class BlockLeavesBirch : BlockLeaves
    {
        /// <summary>
        /// Дополнительная инициализация блока после инициализации предметов и корректировки id блоков
        /// </summary>
        public override void InitAfterItems()
        {
            _idLog = BlocksRegMvk.LogBirch.IndexBlock;
            _idBranch = BlocksRegMvk.BranchBirch.IndexBlock;
            _idFetus = BlocksRegMvk.FetusBirch.IndexBlock;
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
