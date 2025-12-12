using Mvk2.World.Gen;
using System.Runtime.CompilerServices;

namespace Mvk2.World.Biome
{
    public class BiomeSwamp : BiomeIsland
    {
        public BiomeSwamp(ChunkProviderGenerateIsland chunkProvider)
            : base(chunkProvider) { }

        /// <summary>
        /// Получить смещение первого уровня от уровня моря
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override int _GetLevel1BiasWater() => _biasWater;

        /// <summary>
        /// Генерация столба от 3 до 5 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _GenLevel3_5(int xz, int yh, int level3, int level5)
        {
            int y;
            if (level5 == yh)
            {
                // Доп шум для перехода на черносём
                int l6 = level5 - _noise - 1;
                for (y = level3; y < l6; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLoam);
                for (y = l6; y < level5; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdHumus);
                if (level5 == yh) _chunkPrimer.SetBlockState(xz, yh, yh < HeightWater ? _blockIdHumus : _blockIdTurf);
            }
            else
            {
                for (y = level3; y < level5; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLoam);
            }
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
            // Глина
            for (int y = level; y <= yh; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdClay);
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
                if (_noise7 < 0) // Много
                {
                    _chunkPrimer.SetBlockState(xz, yh + 1, _blockIdOreIron);
                }
                else if (_noise7 < -6 && _noise7 == 1)
                {
                    _chunkPrimer.SetBlockState(xz, yh + 1, _blockIdFlowerClover);
                }
                else if (_noise17 < -3)
                {
                    _chunkPrimer.SetBlockState(xz, yh + 1, _blockIdGrass);
                }
            }
            //else if (b == _blockIdWater && _noise7 < 0)
            //{
            //    _chunkPrimer.SetBlockState(xz, yh + 1, _blockIdOreDiamond);
            //}
        }
    }
}
