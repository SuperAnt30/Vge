using Mvk2.World.Block;
using Mvk2.World.Gen;
using Mvk2.World.Gen.Feature;
using System;
using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World.Block;
using Vge.World.Chunk;
using Vge.World.Gen;

namespace Mvk2.World.Biome
{
    /// <summary>
    /// Абстрактный класс биома для генерации Остров
    /// </summary>
    public class BiomeIsland
    {
        /// <summary>
        /// Высота уровня воды
        /// </summary>
        public const int HeightWater = 47;
        /// <summary>
        /// Высота воды -1
        /// </summary>
        protected const int _heightWaterMinus = HeightWater - 1;
        /// <summary>
        /// Высота воды -2
        /// </summary>
        protected const int _heightWaterMinus_2 = HeightWater - 2;
        /// <summary>
        /// Высота воды +1
        /// </summary>
        protected const int _heightWaterPlus = HeightWater + 1;
        /// <summary>
        /// Высота холмов, плюсует к высоте воды
        /// </summary>
        protected const int _heightHill = 72;
        /// <summary>
        /// Высота в горах, проплешина с землёй
        /// </summary>
        protected const int _heightMountainsMix = 58;
        /// <summary>
        /// Высота в горах пустынных
        /// </summary>
        protected const int _heightMountainsDesert = 60;
        /// <summary>
        /// Высота холмов на пляже
        /// </summary>
        protected const int _heightHillBeach = 288;
        /// <summary>
        /// Высота холмов на море
        /// </summary>
        protected const int _heightHillSea = 384;
        /// <summary>
        /// Минимальная высота для декорации блинчиков
        /// </summary>
        protected const int _heightPancakeMin = 36;
        /// <summary>
        /// Центр амплитуды пещер
        /// </summary>
        protected const int _heightCenterCave = 32;

        public readonly ChunkProviderGenerateIsland Provider;

        protected ChunkPrimerIsland _chunkPrimer;
        protected int _xbc;
        protected int _zbc;

        protected readonly Rand _rand;

        /// <summary>
        /// Блок для отладки визуализации биома
        /// </summary>
        protected readonly ushort _blockIdBiomDebug;
        /// <summary>
        /// Вверхний блок
        /// </summary>
        protected readonly ushort _blockIdUp;
        /// <summary>
        /// Блок тела
        /// </summary>
        protected readonly ushort _blockIdBody;
        /// <summary>
        /// Блок камня
        /// </summary>
        protected readonly ushort _blockIdStone;
        /// <summary>
        /// Блок воды
        /// </summary>
        protected readonly ushort _blockIdWater;
        /// <summary>
        /// Имеется ли верхняя прослойка (тело) блоков
        /// </summary>
        protected bool _isBlockBody = true;

        protected readonly IFeatureGeneratorColumn[] _featureColumns;
        protected readonly IFeatureGeneratorArea[] _featureAreas;
        protected readonly IFeatureGeneratorColumn[] _featureColumnsAfter;

        protected BiomeIsland() { }
        public BiomeIsland(ChunkProviderGenerateIsland chunkProvider)
        {
            Provider = chunkProvider;
            _chunkPrimer = chunkProvider.ChunkPrimer;
            _rand = Provider.Rnd;
            _blockIdStone = BlocksRegMvk.Stone.IndexBlock;
            _blockIdWater = BlocksRegMvk.Water.IndexBlock;
            _blockIdBiomDebug = _blockIdUp = BlocksRegMvk.Granite.IndexBlock;
            _blockIdBody = BlocksRegMvk.Granite.IndexBlock;

            _featureColumns = new IFeatureGeneratorColumn[]
            { 
                //new FeatureCactus(_chunkPrimer)
            };

            _featureAreas = new IFeatureGeneratorArea[]
            {
                new FeaturePancake(_chunkPrimer)
            };

            _featureColumnsAfter = new IFeatureGeneratorColumn[]
            {
                new FeatureCactus(_chunkPrimer)
            };
        }

        /// <summary>
        /// Инициализировать декорацию
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void InitDecorator()
        {
            //Decorator = new BiomeDecoratorRobinson(Provider.World);
            //Decorator.Init();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(int xbc, int zbc)
        {
            _xbc = xbc;
            _zbc = zbc;
        }

        /// <summary>
        /// Обновить сид по колонке
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ColumnUpSeed(int x, int z)
        {
            _rand.SetSeed(Provider.Seed);
            int realX = (_xbc + x) * _rand.Next();
            int realZ = (_zbc + z) * _rand.Next();
            _rand.SetSeed(realX ^ realZ ^ Provider.Seed);
        }

        /// <summary>
        /// Получить высоту
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHeight(float height, float river) => GetLevelHeight(0, 0, height, river);

        /// <summary>
        /// Получить уровень высоты
        /// </summary>
        /// <param name="height">Высота -1..0..1</param>
        /// <param name="river">Определение центра реки 1..0..1</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual int GetLevelHeight(int x, int z, float height, float river)
            => HeightWater + (int)(height * _heightHill);

        /// <summary>
        /// Возращаем сгенерированный столбец и возвращает фактическую высоту, без воды
        /// </summary>
        /// <param name="xz">z << 4 | x</param>
        /// <param name="height">Высота в блоках, средняя рекомендуемая</param>
        public int ReliefColumn(int xz, int height)
        {
            int yh = height;
            if (yh < 2) yh = 2;
            int result = _chunkPrimer.HeightMap[xz] = yh;
            int y = 0;

            try
            {
                if (_isBlockBody)
                {
                    // Определяем высоту тела по шуму (3 - 6)
                    int bodyHeight = (int)(Provider.AreaNoise[xz] / 4f + 5f);

                    int yb = yh - bodyHeight;
                    if (yb < 2) yb = 2;

                    // заполняем камнем
                    for (y = 3; y < yb; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdStone);
                    // заполняем тело
                    _Body(yb, yh, xz);
                }
                else
                {
                    // заполняем камнем
                    for (y = 3; y <= yh; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdStone);
                }
            }
            catch (Exception ex)
            {
                Logger.Crash(ex, "Biome.Column yh:{0} y:{1} xz:{2}", yh, y, xz);
            }
            if (yh < HeightWater)
            {
                // меньше уровня воды
                yh++;
                for (y = yh; y < _heightWaterPlus; y++)
                {
                    // заполняем водой
                    _chunkPrimer.SetBlockState(xz, y, _blockIdWater);
                }
            }
            return result;
        }

        /// <summary>
        /// Декорация в текущем чанке, не выходя за пределы
        /// </summary>
        /// <param name="chunkSpawn">Чанк где был спавн декорации</param>
        public void DecorationsColumn(ChunkBase chunkSpawn) => _DecorationsColumn(_featureColumns, chunkSpawn);

        /// <summary>
        /// Декорация в текущем чанке, не выходя за пределы после Area
        /// </summary>
        /// <param name="chunkSpawn">Чанк где был спавн декорации</param>
        public void DecorationsColumnAfter(ChunkBase chunkSpawn) => _DecorationsColumn(_featureColumnsAfter, chunkSpawn);

        /// <summary>
        /// Декорация в текущем чанке, не выходя за пределы
        /// </summary>
        /// <param name="chunkSpawn">Чанк где был спавн декорации</param>
        private void _DecorationsColumn(IFeatureGeneratorColumn[] featureColumns, ChunkBase chunkSpawn)
        {
            int xbc = chunkSpawn.CurrentChunkX << 4;
            int zbc = chunkSpawn.CurrentChunkY << 4;
            Rand rand = Provider.Rnd;
            _UpSeed(rand, xbc, zbc, Provider.Seed);

            // ... тут перечень декор блоков из списка биомов
            foreach (IFeatureGeneratorColumn feature in featureColumns)
            {
                feature.DecorationsColumn(chunkSpawn, rand);
            }
            // Цветы
            //for (int i = 0; i < 0; i++)
            //{
            //    _feature.DecorationsColumn(chunkSpawn, rand);
            //}
        }

        /// <summary>
        /// Декорация областей которые могу выйти за 1 чанк
        /// </summary>
        /// <param name="chunk">Чанк в который вносим данные</param>
        /// <param name="chunkSpawn">Чанк где был спавн декорации</param>
        public void DecorationsArea(ChunkBase chunk, ChunkBase chunkSpawn)
        {
            int xbc = chunkSpawn.CurrentChunkX << 4;
            int zbc = chunkSpawn.CurrentChunkY << 4;
            int biasX = chunk.X - chunkSpawn.X;
            int biasZ = chunk.Y - chunkSpawn.Y;
            Rand rand = Provider.Rnd;
            _UpSeed(rand, xbc, zbc, Provider.Seed);

            // ... тут перечень декор блоков из списка биомов
            foreach (IFeatureGeneratorArea feature in _featureAreas)
            {
                feature.DecorationsArea(chunkSpawn, rand, biasX, biasZ);
            }

            // Цветы
            //for (int i = 0; i < 5; i++)
            //{
            //    _featureArea.DecorationsArea(chunkSpawn, rand, biasX, biasZ);
            //}
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _UpSeed(Rand rand, int xbc, int zbc, long seed)
        {
            rand.SetSeed(seed);
            int realX = xbc * rand.Next();
            int realZ = zbc * rand.Next();
            rand.SetSeed(realX ^ realZ ^ seed);
        }

        /// <summary>
        /// Заполняем тело
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _Body(int yb, int yh, int xz)
        {
            for (int y = yb; y < yh; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdBody);
            _chunkPrimer.SetBlockState(xz, yh, yh < HeightWater ? _blockIdBody : _blockIdUp);
        }
    }
}
