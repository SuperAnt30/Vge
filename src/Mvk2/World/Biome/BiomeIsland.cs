using Mvk2.World.Block;
using Mvk2.World.Gen;
using Mvk2.World.Gen.Feature;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vge.Util;
using Vge.World.Chunk;
using Vge.World.Gen;
using WinGL.Util;

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

        protected readonly ushort _blockIdBedrock = BlocksRegMvk.Bedrock.IndexBlock;
        protected readonly ushort _blockIdLimestone = BlocksRegMvk.Limestone.IndexBlock;
        protected readonly ushort _blockIdClay = BlocksRegMvk.Clay.IndexBlock;
        protected readonly ushort _blockIdSand = BlocksRegMvk.Sand.IndexBlock;
        protected readonly ushort _blockIdLoam = BlocksRegMvk.Loam.IndexBlock;
        protected readonly ushort _blockIdHumus = BlocksRegMvk.Humus.IndexBlock;
        protected readonly ushort _blockIdTurf = BlocksRegMvk.Turf.IndexBlock;
        protected readonly ushort _blockIdTurfLoam = BlocksRegMvk.TurfLoam.IndexBlock;
        protected readonly ushort _blockIdGravel = BlocksRegMvk.Gravel.IndexBlock;
        protected readonly ushort _blockIdOreCoal = BlocksRegMvk.OreCoal.IndexBlock;
        protected readonly ushort _blockIdOreIron = BlocksRegMvk.OreIron.IndexBlock;

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

        protected IFeatureGeneratorColumn[] _featureColumns;
        protected IFeatureGeneratorArea[] _featureAreas;
        protected IFeatureGeneratorColumn[] _featureColumnsAfter;

        /// <summary>
        /// Мелкий шум для переходов слоёв в 3 блока (-1 .. 1)
        /// </summary>
        protected int _noise;
        /// <summary>
        /// Плавный шум для переходов слоёв в 7 блока (-3 .. 3)
        /// </summary>
        protected int _noise7;
        /// <summary>
        /// Смещение от уровня моря
        /// </summary>
        protected int _biasWater;

        //protected BiomeIsland() { }
        public BiomeIsland(ChunkProviderGenerateIsland chunkProvider)
        {
            Provider = chunkProvider;
            _chunkPrimer = chunkProvider.ChunkPrimer;
            _rand = Provider.Rnd;

            _blockIdStone = BlocksRegMvk.Stone.IndexBlock;
            _blockIdWater = BlocksRegMvk.Water.IndexBlock;
            _blockIdBiomDebug = _blockIdUp = BlocksRegMvk.Granite.IndexBlock;
            _blockIdBody = BlocksRegMvk.Granite.IndexBlock;

            _InitFeature();
        }

        protected virtual void _InitFeature()
        {
            // Сначало надо те которые меняют блоки камня (блинчики, руда), они ставят флаг, если их использовать после, будет пустота
            // Потом используем те, которые добавляют, трава, деревья и прочее

            _featureColumns = new IFeatureGeneratorColumn[]
            { 
                //new FeatureCactus(_chunkPrimer, 20)
            };

            _featureAreas = new IFeatureGeneratorArea[]
            {
               // new FeaturePancake(_chunkPrimer, 1, 1, BlocksRegMvk.Stone.IndexBlock, 6, 8),
                //new FeaturePancake(_chunkPrimer, 0, 3, BlocksRegMvk.Glass.IndexBlock, 12, 15),
                //new FeaturePancake(_chunkPrimer, 0, 5, 0, 8, 15),
                //new FeaturePancake(_chunkPrimer, 0, 3, BlocksRegMvk.Lava.IndexBlock, 1, 5),
                
                //new FeatureMinable(_chunkPrimer, 0, 3, BlocksRegMvk.Brol.IndexBlock, 33),
              //  new FeatureMinable(_chunkPrimer, 1, 1, BlocksRegMvk.Glass.IndexBlock, 55, 35, 96),
                //new FeatureMinable(_chunkPrimer, 1, 1, BlocksRegMvk.Cobblestone.IndexBlock, 12)
            };

            _featureColumnsAfter = new IFeatureGeneratorColumn[]
            {
               // new FeatureCactus(_chunkPrimer, 5)
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

        #region Relief

        /// <summary>
        /// Возращаем сгенерированный столбец и возвращает фактическую высоту, без воды
        /// </summary>
        /// <param name="xz">z << 4 | x</param>
        /// <param name="height">Высота в блоках, средняя рекомендуемая</param>
        public virtual int ReliefColumn(int xz, int height)
        {
            // Текущая рабочая высота
            int yh = -1;
            int y = -1;
            // Уровень без моря
            int result = -1;

            try
            {
                // Мелкий шум для переходов слоёв в 7 блока (-3 .. 3)
                _noise7 = -(int)(Provider.AreaNoise[xz] * .4f);
                // Мелкий шум для переходов слоёв в 3 блока (-1 .. 1)
                _noise = (int)(Provider.DownNoise[xz] * 5f);
                // Смещение от уровня моря
                _biasWater = yh - HeightWater;
            
                // Бедрок
                int level0 = (int)(Provider.CaveRiversNoise[xz] / 5f) + 3; // ~ 2 .. 4
                level0 += _noise;
                if (level0 < 1) level0 = 1;
                
                // Определяем высоты
                result = yh = height < level0 ? level0 : height;
                // Смещение от уровня моря
                _biasWater = yh - HeightWater;
                // Столб бедрока
                for (y = 0; y < level0; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdBedrock);

                if (yh > 56 + level0)
                {
                    // Гранит в холмах и горах
                    int ig = yh - 56 - level0;
                    float fg = ig / (float)yh;
                    fg = fg * 1.4f;
                    fg += 1;
                    int level1g = (int)(ig * fg) + level0;

                    // Столб гранити
                    for (y = level0; y < level1g; y++) _chunkPrimer.SetBlockState(xz, y, BlocksRegMvk.Granite.IndexBlock);
                    level0 = level1g;
                }

                // Известняк
                int level1 = (int)(Provider.CaveRiversNoise[xz] * .75f) + 36 // ~ 31 .. 41
                    + _GetLevel1BiasWater() + _noise;
                //level1 = 40 + _biasWater;
                if (level1 > yh) level1 = yh;
                if (level1 < level0) level1 = level0;

                _GenLevel0_1(xz, yh, level0, level1);

                if (level1 < yh)
                {
                    // Продолжаем

                    // Низ песка
                    int level2 = (int)Provider.Level2Noise[xz] + 41 // ~ 34 .. 48
                        + _biasWater - _noise;
                    if (level2 > yh) level2 = yh;
                    if (level2 < level1) level2 = level1;
                    // Низ суглинка
                    int level3 = (int)Provider.Level3Noise[xz] + 42 // ~ 35 .. 49
                        + _biasWater - _noise; 
                    if (level3 > yh) level3 = yh;
                    if (level3 < level1) level3 = level1;

                    // Вверх песка
                    int level4 = (int)Provider.Level4Noise[xz] + 42 // ~ 35 .. 49
                        + _biasWater;
                    if (level4 > yh) level4 = yh;
                    // Вверх суглинка
                    int level5 = (int)Provider.Level5Noise[xz] + 48 // ~ 41 .. 55
                        + _biasWater;
                    if (level5 > yh) level5 = yh;

                    if (level1 > 20)
                    {
                        // Для руд и прочего, должно быть выше 20
                        // Этого мало, типа слой
                        if (level4 + 2 >= level5 && level4 + 2 >= level3)
                        {
                            // Угольная жила
                            int min = Mth.Min(level5 - level3, level4 + 2 - level5) + 1;
                            if (_noise > 0) min *= 2;
                            int level = level1 - _noise7 - 1 - _noise;
                            min = level - min;
                            if (min > level1) min = level1;
                            level += _noise7 + 3;
                            if (level > yh) level = yh;
                            if (level > level1)
                            {
                                level1 = level;
                                if (level2 < level1) level2 = level1;
                                if (level3 < level1) level3 = level1;
                            }
                            _GenLevel1Little(xz, yh, min, level);
                        }

                        // Этого много, типа слой
                        if (level4 < level5 && level4 > level3)
                        {
                            // Угольная жила
                            int min = Mth.Min(level4 - level3, level5 - level4) * 2;
                             //int level = level1 + _noise7 - 1;
                            int level = level1 - _noise7 * 3 - 6 - _noise;
                            _GenLevel1Many(xz, yh, level - min, level);
                        }
                    }

                    // Глина
                    if (level3 > level2)
                    {
                        if (level2 > level1) _GenLevel1_2(xz, yh, level1, level2);
                        else level2 = level1;
                        _GenLevel2_3(xz, yh, level2, level3);
                    }
                    else
                    {
                        if (level3 > level1) _GenLevel1_2(xz, yh, level1, level3);
                        else level3 = level1;
                    }

                    if (level3 < yh)
                    {
                        // Продолжаем
                        if (level5 > level3) _GenLevel3_5(xz, yh, level3, level5);
                        else level5 = level3;

                        if (level4 > level5) 
                        {
                            _GenLevel5_4(xz, yh, level5, level4);
                            if (yh > level4) _GenLevelUp(xz, yh, level4);
                        }
                        else if (yh > level5) _GenLevelUp(xz, yh, level5);
                    }
                }

                if (yh < HeightWater)
                {
                    yh++;
                    // Меньше уровня воды, заливаем водой
                    for (y = yh; y < _heightWaterPlus; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdWater);
                    //yh = HeightWater;
                }
            }
            catch (Exception ex)
            {
                Logger.Crash(ex, "Biome.Column yh:{0} y:{1} xz:{2}", yh, y, xz);
            }

            _chunkPrimer.HeightMap[xz] = yh;
            return result;
        }

        /// <summary>
        /// Получить смещение первого уровня от уровня моря
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual int _GetLevel1BiasWater() => Mth.Abs(_biasWater);

        /// <summary>
        /// Генерация столба от 0 до 1 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _GenLevel0_1(int xz, int yh, int level0, int level1)
        {
            for (int y = level0; y < level1; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLimestone);
            if (level1 == yh) _chunkPrimer.SetBlockState(xz, yh, yh < HeightWater ? _blockIdLoam : _blockIdTurfLoam);
        }

        /// <summary>
        /// Генерация столба от около 1 мало
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _GenLevel1Little(int xz, int yh, int level0, int level1)
        {
            for (int y = level0; y < level1; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdOreCoal);
            if (level1 == yh) _chunkPrimer.SetBlockState(xz, yh, _blockIdOreCoal);
        }

        /// <summary>
        /// Генерация столба от около 1 много
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _GenLevel1Many(int xz, int yh, int level0, int level1)
        {
            for (int y = level0; y < level1; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdSand);
            if (level1 == yh) _chunkPrimer.SetBlockState(xz, yh, _blockIdSand);
        }

        /// <summary>
        /// Генерация столба от 1 до 2 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _GenLevel1_2(int xz, int yh, int level1, int level2)
        {
            // Глина
            for (int y = level1; y < level2; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdClay);
            if (level2 == yh) _chunkPrimer.SetBlockState(xz, yh, yh < HeightWater ? _blockIdClay : _blockIdTurfLoam);
        }

        /// <summary>
        /// Генерация столба от 2 до 3 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _GenLevel2_3(int xz, int yh, int level2, int level3)
        {
            // Местами прослойки песка между глиной и суглинком
            for (int y = level2; y < level3; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdSand);
            if (level3 == yh) _chunkPrimer.SetBlockState(xz, yh, yh < HeightWater ? _blockIdSand : _blockIdTurfLoam);
        }

        /// <summary>
        /// Генерация столба от 3 до 5 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _GenLevel3_5(int xz, int yh, int level3, int level5)
        {
            // Суглинок
            for (int y = level3; y < level5; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLoam);
            // Может дёрн
            if (level5 == yh) _chunkPrimer.SetBlockState(xz, yh, yh < HeightWater ? _blockIdLoam : _blockIdTurfLoam);
        }

        /// <summary>
        /// Генерация столба от 5 до 4 уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _GenLevel5_4(int xz, int yh, int level5, int level4)
        {
            // Местами прослойки песка между вверхном и суглинком
            for (int y = level5; y < level4; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdSand);
            // Сверху песок
            if (level4 == yh) _chunkPrimer.SetBlockState(xz, yh, yh < HeightWater ? _blockIdSand : _blockIdTurfLoam);
            //_chunkPrimer.SetBlockState(xz, yh, _blockIdSand);
        }

        /// <summary>
        /// Генерация столба верхнего уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _GenLevelUp(int xz, int yh, int level)
        {
            if (level > 44 && level < 53)
            {
                // Чернозём
                for (int y = level; y < yh; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdHumus);
                _chunkPrimer.SetBlockState(xz, yh, yh < HeightWater ? _blockIdLoam : _blockIdTurf);
            }
            else
            {
                // Суглинок
                for (int y = level; y < yh; y++) _chunkPrimer.SetBlockState(xz, y, _blockIdLoam);
                _chunkPrimer.SetBlockState(xz, yh, yh < HeightWater ? _blockIdLoam : _blockIdTurfLoam);
            }
        }

        #endregion

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
