using Vge.Util;
using Vge.World.Chunk;

namespace Vge.World.Gen.Feature
{
    /// <summary>
    /// Абстрактный класс генерация особенностей для одного чанка
    /// </summary>
    public abstract class FeatureColumn : IFeatureGeneratorColumn
    {
        protected readonly IChunkPrimer _chunkPrimer;
        /// <summary>
        /// Какой блок ставим
        /// </summary>
        protected readonly ushort _blockId;
        /// <summary>
        /// Минимальное количество
        /// </summary>
        private readonly byte _minRandom;
        /// <summary>
        /// Количество для рандома
        /// </summary>
        private readonly byte _countRandom;
        /// <summary>
        /// Вероятность одной
        /// </summary>
        private int _probabilityOne;

        public FeatureColumn(IChunkPrimer chunkPrimer, byte probabilityOne, 
            ushort blockId)
        {
            _chunkPrimer = chunkPrimer;
            _probabilityOne = probabilityOne;
            _blockId = blockId;
        }

        public FeatureColumn(IChunkPrimer chunkPrimer, byte minRandom, byte maxRandom,
            ushort blockId)
        {
            _chunkPrimer = chunkPrimer;
            _minRandom = minRandom;
            _countRandom = (byte)(maxRandom - minRandom);
            _blockId = blockId;
        }

        /// <summary>
        /// Декорация блока или столба не выходящего за чанк
        /// </summary>
        public void DecorationsColumn(ChunkServer chunkSpawn, Rand rand)
        {
            if (_probabilityOne > 0)
            {
                if (rand.Next(_probabilityOne) == 0)
                {
                    _DecorationColumnOctave(chunkSpawn, rand);
                }
            }
            else
            {
                int countRandom = _countRandom > 0 ? (rand.Next(_countRandom) + _minRandom) : _minRandom;
                if (countRandom > 0)
                {
                    for (int ir = 0; ir < countRandom; ir++)
                    {
                        _DecorationColumnOctave(chunkSpawn, rand);
                    }
                }
            }
        }

        /// <summary>
        /// Декорация блока или столба не выходящего за чанк, одного прохода
        /// </summary>
        protected virtual void _DecorationColumnOctave(ChunkServer chunkSpawn, Rand rand) { }
    }
}
