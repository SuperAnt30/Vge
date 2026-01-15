using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World.Chunk;

namespace Vge.World.Gen.Feature
{
    /// <summary>
    /// Абстрактный класс генерация особенностей для области
    /// </summary>
    public abstract class FeatureArea : IFeatureGeneratorArea
    {
        private readonly IChunkPrimer _chunkPrimer;
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
        /// <summary>
        /// Смещение чанка по X, для определения чанка в какой сетим
        /// </summary>
        private int _biasX;
        /// <summary>
        /// Смещение чанка по Z, для определения чанка в какой сетим
        /// </summary>
        private int _biasZ;

        /// <summary>
        /// Вероятность одной
        /// </summary>
        public FeatureArea(IChunkPrimer chunkPrimer, byte probabilityOne, 
            ushort blockId)
        {
            _chunkPrimer = chunkPrimer;
            _probabilityOne = probabilityOne;
            _blockId = blockId;
        }

        /// <summary>
        /// Количество в одном чанке
        /// </summary>
        public FeatureArea(IChunkPrimer chunkPrimer, byte minRandom, byte maxRandom,
            ushort blockId)
        {
            _chunkPrimer = chunkPrimer;
            _minRandom = minRandom;
            _countRandom = (byte)(maxRandom - minRandom);
            _blockId = blockId;
        }

        /// <summary>
        /// Декорация областей которые могу выйти за 1 чанк
        /// </summary>
        public void DecorationsArea(ChunkServer chunkSpawn, Rand rand, int biasX, int biasZ)
        {
            if (_probabilityOne > 0)
            {
                if (rand.Next(_probabilityOne) == 0)
                {
                    _biasX = biasX;
                    _biasZ = biasZ;
                    _DecorationAreaOctave(chunkSpawn, rand);
                }
            }
            else
            {
                int countRandom = _countRandom > 0 ? (rand.Next(_countRandom) + _minRandom) : _minRandom;
                if (countRandom > 0)
                {
                    _biasX = biasX;
                    _biasZ = biasZ;

                    for (int ir = 0; ir < countRandom; ir++)
                    {
                        _DecorationAreaOctave(chunkSpawn, rand);
                    }
                }
            }
        }

        /// <summary>
        /// Декорация области одного прохода
        /// </summary>
        protected virtual void _DecorationAreaOctave(ChunkServer chunkSpawn, Rand rand) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void _SetBlockReplace(int x, int y, int z, int id, byte flag)
        {
            if (_biasX == (x >> 4) && _biasZ == (z >> 4))
            {
                int xz = (z & 15) << 4 | (x & 15);
                _chunkPrimer.SetBlockIdFlag(xz, y, id, flag);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void _SetBlockState(int x, int y, int z, int id, int met)
        {
            if (_biasX == (x >> 4) && _biasZ == (z >> 4))
            {
                int xz = (z & 15) << 4 | (x & 15);
                _chunkPrimer.SetBlockState(xz, y, id, met);
            }
        }
    }
}
