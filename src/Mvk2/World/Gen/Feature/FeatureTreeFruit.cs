using Mvk2.World.Block;
using Vge.Util;
using Vge.World.Block;
using Vge.World.Gen;
using WinGL.Util;

namespace Mvk2.World.Gen.Feature
{
    public class FeatureTreeFruit : FeatureTree
    {
        public FeatureTreeFruit(ArrayFast<BlockCache> blockCaches, byte minRandom, byte maxRandom, IChunkPrimer chunkPrimer)
            : base(blockCaches, chunkPrimer, minRandom, maxRandom,
                  BlocksRegMvk.LogFruit.IndexBlock, BlocksRegMvk.BranchFruit.IndexBlock, BlocksRegMvk.LeavesFruit.IndexBlock)
        { }

        public FeatureTreeFruit(ArrayFast<BlockCache> blockCaches, byte probabilityOne, IChunkPrimer chunkPrimer)
            : base(blockCaches, chunkPrimer, probabilityOne,
                  BlocksRegMvk.LogFruit.IndexBlock, BlocksRegMvk.BranchFruit.IndexBlock, BlocksRegMvk.LeavesFruit.IndexBlock)
        { }

        public FeatureTreeFruit(ArrayFast<BlockCache> blockCaches) : base(blockCaches,
            BlocksRegMvk.LogFruit.IndexBlock, BlocksRegMvk.BranchFruit.IndexBlock, BlocksRegMvk.LeavesFruit.IndexBlock)
        { }

        /// <summary>
        /// Случайные атрибуты дерева
        /// </summary>
        protected override void _RandSize()
        {
            // Высота ствола дерева до кроны
            _trunkHeight = _NextInt(8) + 6;
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
            _sectionCountBranches = 3;
            // Минимальная обязательная длинна ветки
            _branchLengthMin = 3;
            // Случайная дополнительная длинна ветки к обязательной
            _branchLengthRand = 3;
            // Насыщенность листвы на ветке, меньше 1 не допустимо, чем больше тем веток меньше
            _foliageBranch = 64;
            // Длинна корня, у фруктовых короткий
            _rootLenght = _trunkHeight / 3 - 1;
        }

        /// <summary>
        /// Сгенерировать стартовое положение в чанке
        /// </summary>
        protected override Vector2i _GetRandomPosBegin(Rand rand)
            => new Vector2i(rand.Next(8) * 2, rand.Next(8) * 2 + 1);
    }
}
