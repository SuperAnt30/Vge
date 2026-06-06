using Mvk2.World.Gen.Feature;
using Vge.Util;
using Vge.World.Block;
using Vge.World.Gen;

namespace Mvk2.World.Gen
{
    /// <summary>
    /// Генерация деревьев
    /// </summary>
    public class GenTree
    {
        /// <summary>
        /// Массив кеш блоков для генерации структур текущего мира в потоке генерации
        /// </summary>
        public readonly ArrayFast<BlockCache> BlockGenCaches = new ArrayFast<BlockCache>(16384);
        /// <summary>
        /// Массив кеш блоков для генерации структур текущего мира в потоке обновления
        /// </summary>
        public readonly ArrayFast<BlockCache> BlockUpCaches = new ArrayFast<BlockCache>(16384);

        /// <summary>
        /// 0 = Берёза, 1 = Дуб, 2 = Фруктовое дерево, 3 = Хвойные (Ель или сосна)
        /// </summary>
        public readonly FeatureTree[] FeatureTrees;

        private readonly IChunkPrimer _chunkPrimer;

        public GenTree(IChunkPrimer chunkPrimer)
        {
            _chunkPrimer = chunkPrimer;
            FeatureTrees = new FeatureTree[] {
                // Берёза
                new FeatureTreeBirch(BlockUpCaches),
                // Дуб
                new FeatureTreeOak(BlockUpCaches),
                // Фруктовое дерева
                new FeatureTreeFruit(BlockUpCaches),
                // Хвойные
                new FeatureTreeConifer(BlockUpCaches)
            };
        }

        /// <summary>
        /// Создать объект генерации берёзы в одном чанке, много от minRandom до maxRandom
        /// </summary>
        public FeatureTreeBirch CreateBirrchGen(byte minRandom, byte maxRandom)
            => new FeatureTreeBirch(BlockGenCaches, minRandom, maxRandom, _chunkPrimer);

        /// <summary>
        /// Создать объект генерации берёзы в одном чанке, с вероятностью probabilityOne
        /// </summary>
        public FeatureTreeBirch CreateBirrchGen(byte probabilityOne)
            => new FeatureTreeBirch(BlockGenCaches, probabilityOne, _chunkPrimer);

        /// <summary>
        /// Создать объект генерации дуба в одном чанке, много от minRandom до maxRandom
        /// </summary>
        public FeatureTreeOak CreateOakGen(byte minRandom, byte maxRandom)
            => new FeatureTreeOak(BlockGenCaches, minRandom, maxRandom, _chunkPrimer);

        /// <summary>
        /// Создать объект генерации дуба в одном чанке, с вероятностью probabilityOne
        /// </summary>
        public FeatureTreeOak CreateOakGen(byte probabilityOne)
            => new FeatureTreeOak(BlockGenCaches, probabilityOne, _chunkPrimer);

        /// <summary>
        /// Создать объект генерации фруктового дерева в одном чанке, много от minRandom до maxRandom
        /// </summary>
        public FeatureTreeFruit CreateFruitGen(byte minRandom, byte maxRandom)
            => new FeatureTreeFruit(BlockGenCaches, minRandom, maxRandom, _chunkPrimer);

        /// <summary>
        /// Создать объект генерации фруктового дерева в одном чанке, с вероятностью probabilityOne
        /// </summary>
        public FeatureTreeFruit CreateFruitGen(byte probabilityOne)
            => new FeatureTreeFruit(BlockGenCaches, probabilityOne, _chunkPrimer);

        /// <summary>
        /// Создать объект генерации хвойного дерева в одном чанке, много от minRandom до maxRandom
        /// </summary>
        public FeatureTreeConifer CreateConiferGen(byte minRandom, byte maxRandom)
            => new FeatureTreeConifer(BlockGenCaches, minRandom, maxRandom, _chunkPrimer);

        /// <summary>
        /// Создать объект генерации хвойного дерева в одном чанке, с вероятностью probabilityOne
        /// </summary>
        public FeatureTreeConifer CreateConiferGen(byte probabilityOne)
            => new FeatureTreeConifer(BlockGenCaches, probabilityOne, _chunkPrimer);

    }
}
