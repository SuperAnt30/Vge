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
        /// Берёза в обновлении
        /// </summary>
        public FeatureTreeBirch BirchUp { get; set; }
        /// <summary>
        /// Дуб в обновлении
        /// </summary>
        public FeatureTreeOak OakUp { get; set; }

        private readonly IChunkPrimer _chunkPrimer;

        public GenTree(IChunkPrimer chunkPrimer)
        {
            _chunkPrimer = chunkPrimer;
            // Берёза
            BirchUp = new FeatureTreeBirch(BlockUpCaches);
            // Дуб
            OakUp = new FeatureTreeOak(BlockUpCaches);
        }

        /// <summary>
        /// Создать объект генерации берёзы в одном чанке, много от minRandom до maxRandom
        /// </summary>
        public FeatureTreeBirch CreateBirrchGen( byte minRandom, byte maxRandom)
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

    }
}
