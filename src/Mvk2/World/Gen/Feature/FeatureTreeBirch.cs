using Mvk2.World.Block;
using Vge.Util;
using Vge.World.Block;
using Vge.World.Gen;
using WinGL.Util;


namespace Mvk2.World.Gen.Feature
{
    public class FeatureTreeBirch : FeatureTree
    {

        public FeatureTreeBirch(ArrayFast<BlockCache> blockCaches, IChunkPrimer chunkPrimer)
            : base(blockCaches, chunkPrimer, 3, 4,
                  BlocksRegMvk.LogBirch.IndexBlock, BlocksRegMvk.BranchBirch.IndexBlock, BlocksRegMvk.LeavesBirch.IndexBlock)
        {

        }

        /// <summary>
        /// Случайные атрибуты дерева
        /// </summary>
        protected override void _RandSize()
        {
            // Высота ствола дерева до кроны
            _trunkHeight = _NextInt(12) + 12;
            // Ствол снизу без веток 
            _trunkWithoutBranches = _NextInt(3) + 2;
            // смещение, через какое ствол может смещаться
            _trunkBias = _NextInt(5) + 4;
            // Максимальное смещение ствола от пенька
            _maxTrunkBias = 3;
            // Количество секций веток для сужения
            //                _    / \
            //          _    / \  |   |
            //    _    / \  |   | |   |
            //   / \  |   | |   | |   |
            //   \ /   \ /   \ /   \ /
            //  2 |   3 |   4 |   5 |
            _sectionCountBranches = 5;
            // Минимальная обязательная длинна ветки
            _branchLengthMin = 1;
            // Случайная дополнительная длинна ветки к обязательной
            _branchLengthRand = 2;
            // Насыщенность листвы на ветке, меньше 1 не допустимо, чем больше тем веток меньше
            _foliageBranch = 64;
        }

        /// <summary>
        /// Сгенерировать стартовое положение в чанке
        /// </summary>
        protected override Vector2i _GetRandomPosBegin(Rand rand)
            => new Vector2i(rand.Next(8) * 2, rand.Next(16));
    }
}
