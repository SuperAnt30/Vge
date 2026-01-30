using Mvk2.World.BlockEntity;
using Mvk2.World.BlockEntity.List;
using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World.Block;
using Vge.World.Chunk;
using Vge.World.Gen;
using Vge.World.Gen.Feature;
using WinGL.Util;

namespace Mvk2.World.Gen.Feature
{
    /// <summary>
    /// Генерация дерева на этапе создания чанка.
    /// Так же есть рост дерева объект работает в тактах ElementGrowthTree
    /// </summary>
    public class FeatureTree : FeatureArea
    {
        /// <summary>
        /// Индекс блока бревна
        /// </summary>
        private readonly int _blockLogId;
        /// <summary>
        /// Индекс блока ветки
        /// </summary>
        private readonly int _blockBranchId;
        /// <summary>
        /// Индекс блока листвы
        /// </summary>
        private readonly int _blockLeavesId;

        /// <summary>
        /// Высота ствола дерева
        /// </summary>
        protected int _trunkHeight;
        /// <summary>
        /// Ствол снизу без веток 
        /// </summary>
        protected int _trunkWithoutBranches;
        /// <summary>
        /// Cмещение ствола, через какое количество блоков может смещаться
        /// </summary>
        protected int _trunkBias;
        /// <summary>
        /// Максимальное смещение ствола от пенька
        /// </summary>
        protected int _maxTrunkBias;
        /// <summary>
        /// Количество секций веток для сужения
        ///                _    / \
        ///          _    / \  |   |
        ///    _    / \  |   | |   |
        ///   / \  |   | |   | |   |
        ///   \ /   \ /   \ /   \ /
        ///  2 |   3 |   4 |   5 |
        /// </summary>
        protected int _sectionCountBranches;
        /// <summary>
        /// Минимальная обязательная длинна ветки
        /// </summary>
        protected int _branchLengthMin;
        /// <summary>
        /// Случайная дополнительная длинна ветки к обязательной
        /// </summary>
        protected int _branchLengthRand;
        /// <summary>
        /// Насыщенность листвы на ветке, значение в NextInt() меньше 1 не допустимо, чем больше тем веток меньше
        /// При значении 1 максимально
        /// </summary>
        protected int _foliageBranch;

        /// <summary>
        /// Заключительная часть LCG, которая использует координаты фрагмента X, Z
        /// вместе с двумя другими начальными значениями для генерации псевдослучайных чисел
        /// </summary>
        private long _seed;

        /// <summary>
        /// Массив кеш блоков для генерации структур текущего мира в потоке генерации
        /// </summary>
        private readonly ArrayFast<BlockCache> _blockCaches;
        /// <summary>
        /// Массив всех стартующих в этом чанке деревев.
        /// Защита от рандома в том же месте, если в чанке много однотипных дервеьев, типа лесов
        /// </summary>
        private readonly ListFast<BlockPos> _listTreeBegin = new ListFast<BlockPos>(10);

        public FeatureTree(ArrayFast<BlockCache> blockCaches, IChunkPrimer chunkPrimer, 
            byte minRandom, byte maxRandom, int blockLogId, int blockBranchId, int blockLeavesId) 
            : base(chunkPrimer, minRandom, maxRandom, blockLogId)
        {
            _blockCaches = blockCaches; 
            _blockLogId = blockLogId;
            _blockBranchId = blockBranchId;
            _blockLeavesId = blockLeavesId;
        }
        //public FeatureTree(IChunkPrimer chunkPrimer, byte probabilityOne,
        //    int blockId) : base(chunkPrimer, probabilityOne, blockId)
        //{
        //    //_rangeY = _isAir ? maxY : (byte)(maxY - minY);
        //    //_count = count;
        //}

        /// <summary>
        /// Возвращает псевдослучайное число LCG из [0, x). Аргументы: целое х
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int _NextInt(int x)
        {
            int random = (int)((_seed >> 24) % x);
            if (random < 0) random += x;
            _seed *= _seed * 1284865837 + 4150755663;
            return random;
        }

        /// <summary>
        /// Вероятность, где 1 это 100%, 2 50/50. NextInt(x) != 0
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool _NextBool(int x) => _NextInt(x) != 0;

        /// <summary>
        /// Задать случайное зерно
        /// </summary>
        protected void _SetRand(Rand rand)
        {
            // сид для доп генерации дерева, не отвлекаясь от генерации мира
            _seed = rand.Next();
            _RandSize();
        }

        /// <summary>
        /// Случайные атрибуты дерева
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _RandSize()
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual Vector2i _GetRandomPosBegin(Rand rand)
            => new Vector2i(rand.Next(16), rand.Next(16));

        /// <summary>
        /// Перед декорацией областе всех проходов
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _DecorationAreaOctaveBefore() => _listTreeBegin.Clear();

        /// <summary>
        /// Декорация области одного прохода
        /// </summary>
        protected override void _DecorationAreaOctave(ChunkServer chunkSpawn, Rand rand)
        {
            Vector2i posBegin = _GetRandomPosBegin(rand);
            int bx = posBegin.X;
            int bz = posBegin.Y;
            int by = chunkSpawn.HeightMapGen[bz << 4 | bx];

            BlockPos posPen = new BlockPos(bx, by, bz);
            if (_listTreeBegin.Count > 0)
            {
                if (_listTreeBegin.Contains(posPen))
                {
                    // На этом месте уже имеется что-то, скорее всего дерево.
                    // По этому мы не будем тут генерировать дерево
                    return;
                }
            }
            _listTreeBegin.Add(posPen);

            _blockCaches.Clear();
            // Значения где формируется ствол, чтоб ствол не уходил далеко
            int bx0 = bx;
            int bz0 = bz;

            BlockState blockBegin = new BlockState(_blockLogId | 3 << 12);
            
            _SetRand(rand);

            // Пенёк основа
            _SetBlockCacheTick(bx0, by, bz0, _blockLogId, 3, 120);
            int parent = _Parent();
            // родитель ветки
            int parentBranch;
            int vecX, vecY;
            
            // временное значение смещение ствола
            int txz;
            int y, iUp, iSide, iBranche;
            // Длина ветки
            int lightBranche;
            // высота ствола
            int count = _trunkHeight;
            // ствол с ветками
            int trunkWithBranches = _trunkHeight - _trunkWithoutBranches;
            // i для счётчика сужения веток
            int itwb;
            // stick - палка
            int sx, sy, sz;
            // через сколько может быть смещение ветки по y, один раз
            int stickBiasY;
            // через сколько может быть смещение ветки по x или z, много раз
            int stickBiasXZ;

            // Массив смещения веток по конкретной стороне, чтоб не слипались друг с другом
            int[] row = new int[] { _NextInt(3), _NextInt(3), _NextInt(3), _NextInt(3) };
            // Коэффициент длинны веток 0 - 1
            float factorLightBranches;
            // Количество блоков в секции
            int countBlockSection;
            // Имеется ли смещение ствола в этом уровне
            bool isTrunkBiasLevel;
            // снизу вверх
            for (iUp = 0; iUp < count; iUp++)
            {
                y = by + iUp;
                isTrunkBiasLevel = false;
                // готовим смещение ствола
                if (_trunkBias <= 0)
                {
                    _trunkBias = _NextInt(3) + 1;
                    txz = _NextInt(3) - 1;

                    if (txz != 0 && bx0 >= bx + txz - _maxTrunkBias && bx0 <= bx + txz + _maxTrunkBias)
                    {
                        bx += txz;
                        isTrunkBiasLevel = true;
                    }
                    else
                    {
                        txz = _NextInt(3) - 1;
                        if (txz != 0 && bz0 >= bz + txz - _maxTrunkBias && bz0 <= bz + txz + _maxTrunkBias)
                        {
                            bz += txz;
                            isTrunkBiasLevel = true;
                        }
                    }
                }
                else
                {
                    _trunkBias--;
                }

                // Ствол, если нижний то параметр для пенька
                if (iUp != 0)
                {
                    _SetBlockCache(bx, y, bz, _blockLogId, parent);
                    parent = _Parent();
                }

                // Ветки
                if (!isTrunkBiasLevel && iUp >= _trunkWithoutBranches)
                {
                    // Находи коэффициент длинны веток
                    itwb = iUp - _trunkWithoutBranches;
                    countBlockSection = trunkWithBranches / _sectionCountBranches;
                    factorLightBranches = itwb < countBlockSection ? itwb / (float)countBlockSection : itwb > trunkWithBranches - countBlockSection
                        ? (countBlockSection - (itwb - (trunkWithBranches - countBlockSection))) / (float)countBlockSection : 1;

                    // цикл направлении веток
                    for (iSide = 0; iSide < 4; iSide++)
                    {
                        if (row[iSide] == 0)
                        {
                            // Есть ветка
                            if (_NextInt(16) == 0)
                            {
                                // Разреживание веток, отрицаем ветку и добавляем смещение к ветке
                                row[iSide] = _NextInt(3);
                            }
                            else
                            {
                                // определяем длинну ветки
                                lightBranche = (int)((_NextInt(_branchLengthRand) + _branchLengthMin) * factorLightBranches);
                                if (lightBranche > 0)
                                {
                                    // Точно имеется ветка работаем с ней
                                    parentBranch = parent;
                                    // параметр смещения ветки, 2 - 3 можно откорректировать для разных деревьев
                                    row[iSide] = _NextInt(2) + 2;

                                    vecX = Ce.AreaOne4X[iSide];
                                    vecY = Ce.AreaOne4Y[iSide];
                                    sx = bx;
                                    sy = y;
                                    sz = bz;

                                    stickBiasY = 3;
                                    stickBiasXZ = 2;

                                    for (iBranche = 1; iBranche <= lightBranche; iBranche++)
                                    {
                                        // цикл длинны ветки
                                        if (vecX != 0) sx += vecX;
                                        if (vecY != 0) sz += vecY;

                                        // Проверка смещение ветки по вертикале
                                        if (stickBiasY >= 0)
                                        {
                                            if (stickBiasY == 0) sy++;
                                            stickBiasY--;
                                        }

                                        // Проверка смещение ветки по горизонтали
                                        if (stickBiasXZ == 0)
                                        {
                                            stickBiasXZ = 2;
                                            if (vecY == 0) sz += _NextInt(3) - 1;
                                            if (vecX == 0) sx += _NextInt(3) - 1;
                                        }
                                        else
                                        {
                                            stickBiasXZ--;
                                        }

                                        // фиксируем ветку
                                        _SetBlockCacheMet(sx, sy, sz, iBranche == lightBranche ? _blockBranchId : _blockLogId,
                                            (iSide == 0 || iSide == 2) ? 2 : 1, parentBranch);
                                        parentBranch = _Parent();

                                        if (iBranche == lightBranche || _NextInt(_foliageBranch) == 0)
                                        {
                                            // Листва на ветке
                                            _FoliageBranch(sx, sy, sz, iSide, parentBranch);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            row[iSide]--;
                        }
                    }
                }
            }

            y = by + count;
            // Ствол
            _SetBlockCache(bx, y, bz, _blockBranchId, parent);
            parent = _Parent();
            // Листва на мокушке
            _FoliageTop(bx, y, bz, parent);
            
            
            /*
            int x0 = bx;
            int y0 = by;
            int z0 = bz;

            
            _SetBlockCacheTick(x0, y0, z0, _blockLogId, 3, 120);
            _SetBlockCache(x0, y0 + 1, z0, _blockId, _Parent());
            _SetBlockCache(x0, y0 + 2, z0, _blockId, _Parent());
            _SetBlockCache(x0, y0 + 3, z0, _blockId, _Parent());
            int parent = _Parent();
            for (int x = x0 + 1; x < x0 + 9; x++)
            {
                _SetBlockCacheMet(x, y0 + 3, z0, _blockBranchId, 1, _Parent());
            }
            _SetBlockCache(x0, y0 + 4, z0, _blockId, parent);
            parent = _Parent();
            for (int x = x0 - 1; x > x0 - 9; x--)
            {
                _SetBlockCacheMet(x, y0 + 4, z0, _blockBranchId, 1, _Parent());
            }
            _SetBlockCache(x0, y0 + 5, z0, _blockId, parent);
            parent = _Parent();
            for (int z = z0 + 1; z < z0 + 9; z++)
            {
                _SetBlockCacheMet(x0, y0 + 5, z, _blockBranchId, 2, _Parent());
            }
            for (int z = z0 - 1; z > z0 - 9; z--)
            {
                _SetBlockCacheMet(x0, y0 + 5, z, _blockBranchId, 2, z == z0 - 1 ? parent : _Parent());
            }
            _SetBlockCache(x0, y0 + 6, z0, _blockId, parent);
            //_SetBlockCache(x0, y0 + 3, z0 - 4, _blockLeavesId, 1);
            //_SetBlockCache(x0, y0 + 5, z0 - 4, _blockLeavesId);

            //_SetBlockCache(x0 + 1, y0 + 6, z0, _blockLeavesId, 2);
            //_SetBlockCache(x0 - 1, y0 + 6, z0, _blockLeavesId, 3);
            //_SetBlockCache(x0, y0 + 6, z0 - 1, _blockLeavesId, 4);
            //_SetBlockCache(x0, y0 + 6, z0 + 1, _blockLeavesId, 5);


            _SetBlockCache(x0, y0 + 7, z0, _blockId, _Parent());
            _SetBlockCache(x0, y0 + 8, z0, _blockBranchId, _Parent());
            _SetBlockCacheMet(x0, y0 + 9, z0, _blockBranchId, 4, _Parent());
            _SetBlockCache(x0, y0 + 10, z0, _blockBranchId, _Parent());

            //_SetBlockCache(x0 + 1, y0 + 8, z0, _blockLeavesId, 2);
            //_SetBlockCache(x0 + 1, y0 + 10, z0, _blockLeavesId, 8);
            //_SetBlockCache(x0 - 1, y0 + 10, z0, _blockLeavesId, 9);
            //_SetBlockCache(x0, y0 + 10, z0 - 1, _blockLeavesId, 10);
            //_SetBlockCache(x0, y0 + 10, z0 + 1, _blockLeavesId, 11);

            _SetBlockCacheMet(x0, y0 + 11, z0, _blockLeavesId, 6, _Parent());
            */


            // ==== End
            
            if (_biasX == 0 && _biasZ == 0)
            {
                // Чанк спавна равен текущему чанку записи
                // Можно подготовить массив для TileEntity из _blockCaches
                BlockEntityTree blockEntity = Ce.BlocksEntity.CreateEntityServer(
                    BlocksEntityRegMvk.IdTree, chunkSpawn.WorldServ) as BlockEntityTree;

                blockEntity.SetBlockPosition(blockBegin, 
                    new BlockPos(chunkSpawn.BlockX + bx0, by, chunkSpawn.BlockZ + bz0));
                blockEntity.SetArray(_blockCaches);
                _chunkPrimer.SetBlockEntity(blockEntity);
            }

            _ExportBlockCaches();
        }

        
        /// <summary>
        /// Проверить блок если вернёт true останавливаем генерацию
        /// </summary>
        //protected bool _CheckBlock(WorldBase world, int x, int y, int z)
        //{
        //    if (!isGen)
        //    {
        //        checkPos.X = x;
        //        checkPos.Y = y;
        //        checkPos.Z = z;
        //        checkEnumBlock = world.GetBlockState(checkPos).GetEBlock();
        //        checkEnumMaterial = Blocks.GetBlockCache(checkEnumBlock).Material.EMaterial;
        //        if (!(checkEnumBlock == EnumBlock.Air || checkEnumMaterial == EnumMaterial.Leaves || checkEnumMaterial == EnumMaterial.Sapling)
        //            && checkEnumBlock != log)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        
        /// <summary>
        ///Листва на мокушке
        /// </summary>
        protected void _FoliageTop(int x, int y, int z, int parent)
        {
            // Up
            _SetBlockCacheLeaves(x, y + 1, z, _blockLeavesId, _NextInt(2) * 6, parent);
            // East
            _SetBlockCacheLeaves(x + 1, y, z, _blockLeavesId, 2 + _NextInt(2) * 6, parent);
            // West
            _SetBlockCacheLeaves(x - 1, y, z, _blockLeavesId, 3 + _NextInt(2) * 6, parent);
            // North
            _SetBlockCacheLeaves(x, y, z - 1, _blockLeavesId, 4 + _NextInt(2) * 6, parent);
            // South
            _SetBlockCacheLeaves(x, y, z + 1, _blockLeavesId, 5 + _NextInt(2) * 6, parent);
        }

        /// <summary>
        /// Листва на ветке, side = 0-3 горизонтальный вектор направление вектора MvkStatic.AreaOne4
        /// 0 - South, 1 - East, 2 - North, 3 - West
        /// </summary>
        protected virtual void _FoliageBranch(int x, int y, int z, int side, int parent)
        {
            // Up
            _SetBlockCacheLeaves(x, y + 1, z, _blockLeavesId, _NextInt(2) * 6, parent);
            // Down
            _SetBlockCacheLeaves(x, y - 1, z, _blockLeavesId, 1 + _NextInt(2) * 6, parent);

            if (side == 0) // South
            {
                // Все кроме North
                // East
                _SetBlockCacheLeaves(x + 1, y, z, _blockLeavesId, 2 + _NextInt(2) * 6, parent);
                // West
                _SetBlockCacheLeaves(x - 1, y, z, _blockLeavesId, 3 + _NextInt(2) * 6, parent);
                // South
                _SetBlockCacheLeaves(x, y, z + 1, _blockLeavesId, 5 + _NextInt(2) * 6, parent);
            }
            else if (side == 1) // East
            {
                // Все кроме West
                // East
                _SetBlockCacheLeaves(x + 1, y, z, _blockLeavesId, 2 + _NextInt(2) * 6, parent);
                // North
                _SetBlockCacheLeaves(x, y, z - 1, _blockLeavesId, 4 + _NextInt(2) * 6, parent);
                // South
                _SetBlockCacheLeaves(x, y, z + 1, _blockLeavesId, 5 + _NextInt(2) * 6, parent);
            }
            else if (side == 2) // North
            {
                // Все кроме South
                // East
                _SetBlockCacheLeaves(x + 1, y, z, _blockLeavesId, 2 + _NextInt(2) * 6, parent);
                // West
                _SetBlockCacheLeaves(x - 1, y, z, _blockLeavesId, 3 + _NextInt(2) * 6, parent);
                // North
                _SetBlockCacheLeaves(x, y, z - 1, _blockLeavesId, 4 + _NextInt(2) * 6, parent);
            }
            else // West
            {
                // Все кроме East
                // West
                _SetBlockCacheLeaves(x - 1, y, z, _blockLeavesId, 3 + _NextInt(2) * 6, parent);
                // North
                _SetBlockCacheLeaves(x, y, z - 1, _blockLeavesId, 4 + _NextInt(2) * 6, parent);
                // South
                _SetBlockCacheLeaves(x, y, z + 1, _blockLeavesId, 5 + _NextInt(2) * 6, parent);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int _Parent() => _blockCaches.Count - 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _SetBlockCache(int x, int y, int z, int id, int parent)
            => _blockCaches.Add(new BlockCache(x, y, z, id) { ParentIndex = parent });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _SetBlockCacheMet(int x, int y, int z, int id, int met, int parent)
            => _blockCaches.Add(new BlockCache(x, y, z, id, met) { ParentIndex = parent });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _SetBlockCacheLeaves(int x, int y, int z, int id, int met, int parent)
            => _blockCaches.Add(new BlockCache(x, y, z, id, met) { ParentIndex = parent, Flag = 2 });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _SetBlockCacheTick(int x, int y, int z, int id, int met, uint tick)
            => _blockCaches.Add(new BlockCache(x, y, z, id, met) { Tick = tick });


        /// <summary>
        /// Экспортировать кэш блоки в временные для чанка генерации
        /// </summary>
        protected void _ExportBlockCaches()
        {
            int count = _blockCaches.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    _SetBlockState(_blockCaches[i]);

                    //SetBlockState(blockCache.Position, blockCache.GetBlockState(), 46);
                    //if (blockCache.Tick != 0)
                    //{
                    //    SetBlockTick(blockCache.Position, blockCache.Tick);
                    //}
                }
            }
            _blockCaches.Clear();
        }
    }
}
