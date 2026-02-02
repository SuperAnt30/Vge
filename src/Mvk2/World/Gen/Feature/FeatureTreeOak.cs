using Mvk2.World.Block;
using Vge.Util;
using Vge.World.Block;
using Vge.World.Gen;
using WinGL.Util;

namespace Mvk2.World.Gen.Feature
{
    public class FeatureTreeOak : FeatureTree
    {
        public FeatureTreeOak(ArrayFast<BlockCache> blockCaches, byte minRandom, byte maxRandom, IChunkPrimer chunkPrimer)
            : base(blockCaches, chunkPrimer, minRandom, maxRandom,
                  BlocksRegMvk.LogOak.IndexBlock, BlocksRegMvk.BranchOak.IndexBlock, BlocksRegMvk.LeavesOak.IndexBlock)
        { }

        public FeatureTreeOak(ArrayFast<BlockCache> blockCaches, byte probabilityOne, IChunkPrimer chunkPrimer)
            : base(blockCaches, chunkPrimer, probabilityOne,
                  BlocksRegMvk.LogOak.IndexBlock, BlocksRegMvk.BranchOak.IndexBlock, BlocksRegMvk.LeavesOak.IndexBlock)
        { }

        public FeatureTreeOak(ArrayFast<BlockCache> blockCaches) : base(blockCaches,
            BlocksRegMvk.LogOak.IndexBlock, BlocksRegMvk.BranchOak.IndexBlock, BlocksRegMvk.LeavesOak.IndexBlock)
        { }

        /// <summary>
        /// Случайные атрибуты дерева
        /// </summary>
        protected override void _RandSize()
        {
            // Высота ствола дерева до кроны
            _trunkHeight = _NextInt(11) + 12;
            // Ствол снизу без веток 
            _trunkWithoutBranches = _NextInt(3) + 2;
            // смещение, через какое ствол может смещаться
            _trunkBias = _NextInt(8) + 2;
            // Максимальное смещение ствола от пенька
            _maxTrunkBias = 6;
            // Количество секций веток для сужения
            //                _    / \
            //          _    / \  |   |
            //    _    / \  |   | |   |
            //   / \  |   | |   | |   |
            //   \ /   \ /   \ /   \ /
            //  2 |   3 |   4 |   5 |
            _sectionCountBranches = 4;
            // Минимальная обязательная длинна ветки
            _branchLengthMin = 4;
            // Случайная дополнительная длинна ветки к обязательной
            _branchLengthRand = 4;
            // Насыщенность листвы на ветке, меньше 1 не допустимо, чем больше тем веток меньше
            _foliageBranch = _NextInt(8) == 0 ? _NextInt(8) + 1 : 32;
        }

        /// <summary>
        /// Сгенерировать стартовое положение в чанке
        /// </summary>
        protected override Vector2i _GetRandomPosBegin(Rand rand)
            => new Vector2i(rand.Next(8) * 2 + 1, rand.Next(16));
    }
}
