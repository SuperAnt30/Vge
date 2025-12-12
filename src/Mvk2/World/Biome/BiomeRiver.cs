using Mvk2.World.Gen;
using System.Runtime.CompilerServices;

namespace Mvk2.World.Biome
{
    public class BiomeRiver : BiomeIsland
    {
        public BiomeRiver(ChunkProviderGenerateIsland chunkProvider)
            : base(chunkProvider) { }

        /// <summary>
        /// Получить смещение первого уровня от уровня моря
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override int _GetLevel1BiasWater() => _biasWater;

        /// <summary>
        /// Генерация столба от 2 до 3 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevel2_3(int xz, int yh, int level2, int level3)
        {
            // Местами прослойки песка между глиной и суглинком
            for (int y = level2; y < level3; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdSand);
            if (level3 == yh) _chunkPrimer.SetBlockState(xz, yh, _blockIdSand);
        }

        /// <summary>
        /// Генерация столба от 5 до 4 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevel5_4(int xz, int yh, int level5, int level4)
        {
            // Местами прослойки глины между вверхном и суглинком
            for (int y = level5; y < level4; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdClay);
            // Сверху глина
            if (level4 == yh) _chunkPrimer.SetBlockState(xz, yh, _blockIdClay);
        }

        /// <summary>
        /// Генерация столба верхнего уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevelUp(int xz, int yh, int level)
        {
            int y;
            if (yh < 50)
            {
                // Песок
                for (y = level; y <= yh; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdSand);
            }
            else
            {
                
                int y2 = level + _noise;
                for (y = level; y < y2; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdSand);
                for (y = y2; y < yh; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLoam);
                _chunkPrimer.SetBlockState(xz, yh, _blockIdTurfLoam);
            }
        }

        /// <summary>
        /// Генерация столба поверхности, трава, цветы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevelSurface(int xz, int yh)
        {
            ushort b = _chunkPrimer.GetBlockId(xz, yh);
            if (b == _blockIdTurf || b == _blockIdTurfLoam)
            {
                base._GenLevelSurface(xz, yh);
            }
        }
    }
}
