using Mvk2.World.Block;
using Mvk2.World.BlockEntity;
using Mvk2.World.BlockEntity.List;
using System;
using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;
using Vge.World.Gen;
using Vge.World.Gen.Feature;
using Vge.World.Сalendar;
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
        /// Индекс блока корня
        /// </summary>
        private readonly int _blockRootId;
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
        /// Длинна корня
        /// </summary>
        protected int _rootLenght;

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
        /// <summary>
        /// Массив траектории древа с прошлого шага. Используется только в Update
        /// </summary>
        private BlockPosLoc[] _blocks;

        public FeatureTree(ArrayFast<BlockCache> blockCaches, IChunkPrimer chunkPrimer, 
            byte minRandom, byte maxRandom, int blockLogId, int blockBranchId, int blockLeavesId) 
            : base(chunkPrimer, minRandom, maxRandom, BlocksRegMvk.TurfLoam.IndexBlock)
        {
            _blockCaches = blockCaches; 
            _blockLogId = blockLogId;
            _blockBranchId = blockBranchId;
            _blockLeavesId = blockLeavesId;
            _blockRootId = BlocksRegMvk.TreeRoot.IndexBlock;
        }

        public FeatureTree(ArrayFast<BlockCache> blockCaches, IChunkPrimer chunkPrimer,
            byte probabilityOne, int blockLogId, int blockBranchId, int blockLeavesId)
            : base(chunkPrimer, probabilityOne, BlocksRegMvk.TurfLoam.IndexBlock)
        {
            _blockCaches = blockCaches;
            _blockLogId = blockLogId;
            _blockBranchId = blockBranchId;
            _blockLeavesId = blockLeavesId;
            _blockRootId = BlocksRegMvk.TreeRoot.IndexBlock;
        }

        public FeatureTree(ArrayFast<BlockCache> blockCaches, 
            int blockLogId, int blockBranchId, int blockLeavesId)
        {
            _blockCaches = blockCaches;
            _blockLogId = blockLogId;
            _blockBranchId = blockBranchId;
            _blockLeavesId = blockLeavesId;
            _blockRootId = BlocksRegMvk.TreeRoot.IndexBlock;
        }

        #region Random

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
        /// Вероятность, где 1 это 100%, 2 50/50. NextInt(x) == 0
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool _NextBool(int x) => _NextInt(x) == 0;

        /// <summary>
        /// Задать случайное зерно
        /// </summary>
        protected void _SetRand(Rand rand, bool size = true)
        {
            // сид для доп генерации дерева, не отвлекаясь от генерации мира
            _seed = rand.Next();
            if (size)
            {
                _RandSize();
            }
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
            // Длинна корня
            _rootLenght = _NextInt(5) + 2;
        }

        /// <summary>
        /// Сгенерировать стартовое положение в чанке
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual Vector2i _GetRandomPosBegin(Rand rand)
            => new Vector2i(rand.Next(16), rand.Next(16));

        #endregion

        #region Decoration

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

            int idBlockBegin = chunkSpawn.GetBlockState(bx, by - 2, bz).Id;

            if (idBlockBegin == BlocksRegMvk.Loam.IndexBlock
                || idBlockBegin == BlocksRegMvk.Humus.IndexBlock 
                || idBlockBegin == _blockRootId
                || idBlockBegin == _blockLogId)
            {
                _SetRand(rand);
                _GenerationMature(bx, by, bz);

                if (_biasX == 0 && _biasZ == 0)
                {
                    // Чанк спавна равен текущему чанку записи
                    BlockEntityTree blockEntity = _CreateBlockEntity();
                    blockEntity.SetBlockPosition(chunkSpawn, new BlockState(_blockLogId | 515 << 12),
                        new BlockPos(chunkSpawn.BlockX + bx, by - 1, chunkSpawn.BlockZ + bz));
                    blockEntity.SetArrayLocal(_blockCaches);
                    blockEntity.StepMature(_rootLenght);
                    _chunkPrimer.SetBlockEntity(blockEntity);
                }

                _ExportBlockCachesInChunkPrimer();
            }
        }

        #endregion

        #region Листва

        /// <summary>
        ///Листва на мокушке
        /// </summary>
        protected void _LeavesTop(int x, int y, int z)
        {
            // Up
            AddBlockLeaves(x, y + 1, z, _NextInt(2) * 6);
            // Бок
            _LeavesTrunk(x, y, z);
        }

        /// <summary>
        ///Листва на стволе
        /// </summary>
        protected void _LeavesTrunk(int x, int y, int z)
        {
            // East
            AddBlockLeaves(x + 1, y, z, 2 + _NextInt(2) * 6);
            // West
            AddBlockLeaves(x - 1, y, z, 3 + _NextInt(2) * 6);
            // North
            AddBlockLeaves(x, y, z - 1, 4 + _NextInt(2) * 6);
            // South
            AddBlockLeaves(x, y, z + 1, 5 + _NextInt(2) * 6);
        }

        /// <summary>
        /// Листва на ветке, side = 0-3 горизонтальный вектор направление вектора MvkStatic.AreaOne4
        /// 0 - South, 1 - East, 2 - North, 3 - West
        /// </summary>
        protected virtual void _LeavesBranch(int x, int y, int z, int side)
        {
            // Up
            AddBlockLeaves(x, y + 1, z, _NextInt(2) * 6);
            // Down
            AddBlockLeaves(x, y - 1, z, 1 + _NextInt(2) * 6);

            if (side == 0) // South
            {
                // Все кроме North
                // East
                AddBlockLeaves(x + 1, y, z, 2 + _NextInt(2) * 6);
                // West
                AddBlockLeaves(x - 1, y, z, 3 + _NextInt(2) * 6);
                // South
                AddBlockLeaves(x, y, z + 1, 5 + _NextInt(2) * 6);
            }
            else if (side == 1) // East
            {
                // Все кроме West
                // East
                AddBlockLeaves(x + 1, y, z, 2 + _NextInt(2) * 6);
                // North
                AddBlockLeaves(x, y, z - 1, 4 + _NextInt(2) * 6);
                // South
                AddBlockLeaves(x, y, z + 1, 5 + _NextInt(2) * 6);
            }
            else if (side == 2) // North
            {
                // Все кроме South
                // East
                AddBlockLeaves(x + 1, y, z, 2 + _NextInt(2) * 6);
                // West
                AddBlockLeaves(x - 1, y, z, 3 + _NextInt(2) * 6);
                // North
                AddBlockLeaves(x, y, z - 1, 4 + _NextInt(2) * 6);
            }
            else // West
            {
                // Все кроме East
                // West
                AddBlockLeaves(x - 1, y, z, 3 + _NextInt(2) * 6);
                // North
                AddBlockLeaves(x, y, z - 1, 4 + _NextInt(2) * 6);
                // South
                AddBlockLeaves(x, y, z + 1, 5 + _NextInt(2) * 6);
            }
        }

        #endregion

        #region Add blocks

        /// <summary>
        /// Добавить блок в кеш ствол, без мет
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _AddBlockTrunk(int x, int y, int z, int id, int parent)
            => _blockCaches.Add(new BlockCache(x, y, z, id) { ParentIndex = parent });

        /// <summary>
        /// Добавить блок в кеш с мет данными, ветки или начальный блок (пень) молодого дерева
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _AddBlockMet(int x, int y, int z, int id, int met, int parent)
            => _blockCaches.Add(new BlockCache(x, y, z, id, met) { ParentIndex = parent });

        /// <summary>
        /// Добавить блок в кеш пняь с мет данными и пометкой что можно ломать блоки как корень
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _AddBlockStump(int x, int y, int z, int met, int parent)
            => _blockCaches.Add(new BlockCache(x, y, z, _blockLogId, met) { ParentIndex = parent, Flag = 3 });

        /// <summary>
        /// Добавить блок в кеш пняь с мет данными и пометкой что можно ломать блоки как корень
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _AddBlockRoot(int x, int y, int z, int met)
            => _blockCaches.Add(new BlockCache(x, y, z, _blockRootId, met) { ParentIndex = -2 });

        /// <summary>
        /// Добавить блок в кеш листву с мет данных и пометкой нет привязки к древу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddBlockLeaves(int x, int y, int z, int met)
            => _blockCaches.Add(new BlockCache(x, y, z, _blockLeavesId, met) { ParentIndex = -3, Flag = 2 });

        /// <summary>
        /// Создать блок сущности для дерева
        /// </summary>
        protected virtual BlockEntityTree _CreateBlockEntity()
            => Ce.BlocksEntity.CreateEntityServer(BlocksEntityRegMvk.IdTree) as BlockEntityTree;

        #endregion

        #region Export

        /// <summary>
        /// Экспортировать кэш блоки в временный чанк для генерации
        /// Не обновление!
        /// </summary>
        protected void _ExportBlockCachesInChunkPrimer()
        {
            int count = _blockCaches.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    _SetBlockState(_blockCaches[i]);
                }
            }
            _blockCaches.Clear();
        }

        /// <summary>
        /// Экспортировать кэш блоков с локальными координатами в серверный мир, зная блочное смещение
        /// </summary>
        protected void _ExportBlockCachesInWorld(WorldServer world, int bX, int bZ)
        {
            int count = _blockCaches.Count;
            if (count > 0)
            {
                BlockPos pos;
                BlockCache blockCache;
                for (int i = 0; i < count; i++)
                {
                    blockCache = _blockCaches[i];
                    pos = blockCache.Position;
                    pos.X += bX;
                    pos.Z += bZ;
                    if (blockCache.Flag == 2) // Это листва
                    {
                        // Блоки где обязательно нужен воздух, листва
                        if (world.GetBlockState(pos).Id == 0)
                        {
                            // Без соседей
                            world.SetBlockState(pos, blockCache.GetBlockState(), 44);
                        }
                    }
                    else
                    {
                        // Без соседей, кроме первого
                        world.SetBlockState(pos, blockCache.GetBlockState(), i == 0 ? 46 : 44);
                    }
                }
            }
            _blockCaches.Clear();
        }

        #endregion

        #region Generation

        /// <summary>
        /// Сгенерировать взрослое древо в кеш блоки из локальных координат чанка
        /// </summary>
        private void _GenerationMature(int bx, int by, int bz)
        {
            _blockCaches.Clear();
            // Значения где формируется ствол, чтоб ствол не уходил далеко
            int bx0 = bx;
            int bz0 = bz;

            // общий счётчик родителя
            int parentAmount = 0;
            // родитель ствола
            int parentTrunk = 0;
            // родитель ветки
            int parentBranch;

            // Пенёк основа
            _AddBlockStump(bx0, by - 1, bz0, 515, -1);
            _AddBlockStump(bx0, by, bz0, 3, parentTrunk);

            parentTrunk = ++parentAmount;
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
            // Была ли ветка на стороне прошлым уровнем
            bool[] branchSide = new bool[4];
            // Коэффициент длинны веток 0 - 1
            float factorLightBranches;
            // Количество блоков в секции
            int countBlockSection;
            // Сторона смещение ствола в этом уровне
            int sideTrunkBiasLevel;
            // снизу вверх
            for (iUp = 0; iUp < count; iUp++)
            {
                y = by + iUp;
                sideTrunkBiasLevel = -1;
                // готовим смещение ствола
                if (_trunkBias <= 0)
                {
                    _trunkBias = _NextInt(3) + 1;
                    txz = _NextInt(3) - 1;
                    if (txz != 0 && bx0 >= bx + txz - _maxTrunkBias && bx0 <= bx + txz + _maxTrunkBias)
                    {
                        if (txz == 1)
                        {
                            if (!branchSide[1])
                            {
                                bx++;
                                sideTrunkBiasLevel = 3;
                            }
                        }
                        else
                        {
                            if (!branchSide[3])
                            {
                                bx--;
                                sideTrunkBiasLevel = 1;
                            }
                        }
                    }
                    else
                    {
                        txz = _NextInt(3) - 1;
                        if (txz != 0 && bz0 >= bz + txz - _maxTrunkBias && bz0 <= bz + txz + _maxTrunkBias)
                        {
                            if (txz == 1)
                            {
                                if (!branchSide[0])
                                {
                                    bz++;
                                    sideTrunkBiasLevel = 2;
                                }
                            }
                            else
                            {
                                if (!branchSide[2])
                                {
                                    bz--;
                                    sideTrunkBiasLevel = 0;
                                }
                            }
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
                    _AddBlockTrunk(bx, y, bz, _blockLogId, parentTrunk);
                    parentTrunk = ++parentAmount;
                }

                // Ветки
                if (iUp >= _trunkWithoutBranches)
                {
                    // Находи коэффициент длинны веток
                    itwb = iUp - _trunkWithoutBranches;
                    countBlockSection = trunkWithBranches / _sectionCountBranches;
                    factorLightBranches = itwb < countBlockSection ? itwb / (float)countBlockSection : itwb > trunkWithBranches - countBlockSection
                        ? (countBlockSection - (itwb - (trunkWithBranches - countBlockSection))) / (float)countBlockSection : 1;

                    // цикл направлении веток
                    for (iSide = 0; iSide < 4; iSide++)
                    {
                        branchSide[iSide] = false;
                        if (row[iSide] <= 0 && sideTrunkBiasLevel != iSide)
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
                                    branchSide[iSide] = true;
                                    // Точно имеется ветка работаем с ней
                                    parentBranch = parentTrunk;
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

                                        if (iBranche != lightBranche)
                                        {
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
                                            _AddBlockMet(sx, sy, sz, _blockLogId,
                                                (iSide == 0 || iSide == 2) ? 2 : 1, parentBranch);
                                        }
                                        else
                                        {
                                            // фиксируем тонкую ветку
                                            _AddBlockMet(sx, sy, sz, _blockBranchId,
                                                (iSide == 0 || iSide == 2) ? 2 : 1, parentBranch);
                                        }
                                        parentBranch = ++parentAmount;

                                        if (iBranche == lightBranche || _NextInt(_foliageBranch) == 0)
                                        {
                                            // Листва на ветке
                                            _LeavesBranch(sx, sy, sz, iSide);
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
            _AddBlockTrunk(bx, y, bz, _blockBranchId, parentTrunk);
            // Листва на мокушке
            _LeavesTop(bx, y, bz);

            // === Корень
            for (iUp = 0; iUp < _rootLenght; iUp++)
            {
                y = by - 2 - iUp;
                _AddBlockRoot(bx0, y, bz0, 0);
            }
            // Стороны
            // цикл направлении веток
            for (iSide = 0; iSide < 4; iSide++)
            {
                lightBranche = _NextInt(4);
                if (lightBranche > 0)
                {
                    sy = _NextInt(4);
                    if (sy >= _rootLenght) sy = _rootLenght - 1;
                    sy = by - 2 - sy;
                    vecX = Ce.AreaOne4X[iSide];
                    vecY = Ce.AreaOne4Y[iSide];
                    sx = bx0;
                    sz = bz0;
                    for (iBranche = 1; iBranche <= lightBranche; iBranche++)
                    {
                        // цикл длинны ветки
                        if (vecX != 0) sx += vecX;
                        if (vecY != 0) sz += vecY;
                        _AddBlockRoot(sx, sy, sz, (iSide == 0 || iSide == 2) ? 2 : 1);
                    }
                }
            }
        }

        /// <summary>
        /// Сгенерировать молодое дерево в кеш блоки из локальных координат чанка, шаг из саженеца
        /// </summary>
        private void _GenerationYoung(int bx, int by, int bz)
        {
            _blockCaches.Clear();

            // Пенёк основа
            _AddBlockMet(bx, by, bz, _blockBranchId, 512, -1);
            // родитель ствола
            int parentTrunk = 0;

            // временное значение смещение ствола
            int txz;
            int y, iUp;
            // высота ствола
            _trunkHeight /= 3;
            // смещение, через какое ствол может смещаться
            _trunkBias /= 3;
            // Индекс смещения листвы на стволе
            _trunkWithoutBranches /= 3;
            // мет данные для тонкого ствола
            int met = 0;

            // снизу вверх
            for (iUp = 1; iUp < _trunkHeight; iUp++)
            {
                y = by + iUp;
                // готовим смещение ствола
                if (_trunkBias <= 0)
                {
                    _trunkBias = _NextInt(3) + 1;
                    txz = _NextInt(3) - 1;
                    if (txz != 0)
                    {
                        met = txz == 1 ? 3 : 4;
                    }
                    else
                    {
                        txz = _NextInt(3) - 1;
                        if (txz != 0) met = txz == 1 ? 6 : 5;
                        else met = 0;
                    }
                }
                else
                {
                    _trunkBias--;
                }

                // Ствол
                _AddBlockMet(bx, y, bz, _blockBranchId, met, parentTrunk);
                parentTrunk++;

                // Листва
                if (_trunkWithoutBranches < 0)
                {
                    _trunkWithoutBranches = _NextInt(2) + 1;
                    _LeavesTrunk(bx, y, bz);
                }
                else if (iUp < _trunkHeight - 2)
                {
                    _trunkWithoutBranches--;
                }
            }

            y = by + _trunkHeight;
            // Ствол
            _AddBlockTrunk(bx, y, bz, _blockBranchId, parentTrunk);
            // Листва на мокушке
            _LeavesTop(bx, y, bz);
        }

        #endregion

        #region Checks

        /// <summary>
        /// Проверяем блоки на возможность установить
        /// </summary>
        protected bool _CheckBlockCachesInWorld(WorldServer world, int bX, int bZ)
        {
            int count = _blockCaches.Count;
            if (count > 0)
            {
                BlockPos pos;
                BlockCache blockCache;
                int id;
                for (int i = 0; i < count; i++)
                {
                    blockCache = _blockCaches[i];

                    if (blockCache.ParentIndex != -3) // Листву игнорируем
                    {
                        pos = blockCache.Position;
                        if (!_IsPresentBlock(pos))
                        {
                            pos.X += bX;
                            pos.Z += bZ;
                            id = world.GetBlockState(pos).Id;
                            if (blockCache.ParentIndex == -2 || blockCache.Flag == 3) // Корень или пень
                            {
                                if (!_CheckRoot(id)) return false;
                            }
                            else if (id != 0 && id != _blockLeavesId)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Проверка корня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool _CheckRoot(int id)
            => id == _blockRootId || Ce.Blocks.BlockObjects[id].Material.RootGrowing;

        /// <summary>
        /// Проверить наличие блока в массиве блока сущности дерева.
        /// Локальные координаты
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _IsPresentBlock(BlockPos pos)
        {
            if (_blocks != null)
            {
                for (int i = 0; i < _blocks.Length; i++)
                {
                    if (_blocks[i].EqualsPos(pos)) return true;
                }
            }
            return false;
        }

        #endregion

        #region Steps

        /// <summary>
        /// Шаг первый, саженец
        /// </summary>
        public void StepSapling(WorldServer world, ChunkServer chunk, BlockPos blockPos, Rand rand)
        {
            _SetRand(rand);
            // Для роста надо вытащить блок сущности
            _blocks = new BlockPosLoc[] { new BlockPosLoc(blockPos.X & 15, blockPos.Y, blockPos.Z & 15, _blockBranchId, -1) };
            // Генерация древа
            _GenerationYoung(blockPos.X & 15, blockPos.Y, blockPos.Z & 15);

            // Проверка кто мешает
            if (_CheckBlockCachesInWorld(world, chunk.BlockX, chunk.BlockZ))
            {
                // Дерево вырасло

                // Создаём Блок сущности
                BlockEntityTree blockEntity = _CreateBlockEntity();
                blockEntity.SetBlockPosition(chunk, new BlockState(_blockLogId | 3 << 12), blockPos);
                blockEntity.SetArrayLocal(_blockCaches);
                chunk.SetBlockEntity(blockEntity);

                _ExportBlockCachesInWorld(world, chunk.BlockX, chunk.BlockZ);
            }
            else if(_NextInt(5) == 0)
            {
                // Саженец засох
                world.SetBlockState(blockPos, new BlockState(BlocksRegMvk.SaplingDry.IndexBlock), 44);
            }
            _blocks = null;
        }

        /// <summary>
        /// Другие шаги, когда уже имеется блок сущности, не саженец
        /// </summary>
        public void StepsOther(WorldServer world, ChunkServer chunk, BlockPos blockPos, Rand rand)
        {
            if (chunk.GetBlockEntity(blockPos) is BlockEntityTree blockEntity)
            {
                if (blockEntity.Step == BlockEntityTree.TypeStep.Young)
                {
                    // Пробуем рост с молодого в нормальное дерево
                    _SetRand(rand);

                    // Генерация древа
                    if (blockEntity.GetHeight() < _trunkHeight / 5)
                    {
                        // Засохло, скорее всего срубленное
                        _DriedUp(world, blockEntity);
                    }
                    else
                    {
                        // Для роста надо вытащить блок сущности
                        _blocks = blockEntity.CopyArrayBlocks();

                        // Взрослое
                        _GenerationMature(blockPos.X & 15, blockPos.Y, blockPos.Z & 15);

                        // Проверка кто мешает
                        if (_CheckBlockCachesInWorld(world, chunk.BlockX, chunk.BlockZ))
                        {
                            // Удаляем все блоки прошлого дерева в мире
                            blockEntity.RemoveAll(world);
                            // Меняем шаг
                            blockEntity.StepMature(_rootLenght);
                            // Вносим новый массив дерева
                            blockEntity.SetArrayLocal(_blockCaches);
                            // Экспорт
                            _ExportBlockCachesInWorld(world, chunk.BlockX, chunk.BlockZ);

                            chunk.RemoveBlockEntity(blockPos);

                            blockPos = blockPos.OffsetDown();
                            blockEntity.SetBlockPosition(chunk, chunk.GetBlockState(blockPos), blockPos);
                            chunk.SetBlockEntity(blockEntity);
                        }
                        // Если не смоглы выростить из-за вписать модель, генерируем высыхание
                        else if (_NextInt(5) == 0)
                        {
                            // Удалить листву и перестаём тикать
                            _DriedUp(world, blockEntity);
                        }
                        _blocks = null;
                    }
                }
                else if (blockEntity.Step == BlockEntityTree.TypeStep.Mature)
                {
                    // Если взрослое дерево
                    BlockEntityTree.StateVariant state = blockEntity.CheckingCondition(world);

                    if (state == BlockEntityTree.StateVariant.Dry)
                    {
                        _DriedUp(world, blockEntity);
                    }
                    else if (state == BlockEntityTree.StateVariant.Fetus)
                    {
                        _SetRand(rand, false);
                        _PlaceFetus(world, blockEntity);
                    }
                    else if (state == BlockEntityTree.StateVariant.Cancel)
                    {
                        _CancelFetus(world, blockEntity);
                    }
                }
            }
        }

        #endregion

        #region StateVariant

        /// <summary>
        /// Дерево засохло, надо пометить всю листву на высыхание
        /// </summary>
        private void _DriedUp(WorldServer world, BlockEntityTree blockEntity)
        {
            blockEntity.DriedUp();
            int count = blockEntity.Blocks.Length;
            if (count > 0)
            {
                int biasX = (blockEntity.Position.X >> 4) << 4;
                int biasZ = (blockEntity.Position.Z >> 4) << 4;

                BlockPosLoc posLoc;
                BlockPos pos = new BlockPos();
                BlockPos posLeaves;
                BlockState state;
                // Пробегаемся по всем блокам траектории дерева, и помечаем листву +512 на высыхание
                int i, j, met;
                for (i = 0; i < count; i++)
                {
                    posLoc = blockEntity.Blocks[i];
                    pos.X = posLoc.X + biasX;
                    pos.Y = posLoc.Y;
                    pos.Z = posLoc.Z + biasZ;

                    for (j = 0; j < 6; j++) // 6 сторон ветки
                    {
                        posLeaves = pos.Offset(j);
                        state = world.GetBlockState(posLeaves);
                        if (state.Id == _blockLeavesId) // Если текущая ветка
                        {
                            met = state.Met;
                            if (met > 255) met = met & 0xF;
                            if (met > 5) met -= 6;
                            if (met == j) // Если текущаяя сторона
                            {
                                world.SetBlockStateMet(posLeaves, state.Met & 0xF | 512, false);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Дерево готовится к урожаю плотов, надо пометить часть листвы на урожай
        /// </summary>
        private void _PlaceFetus(WorldServer world, BlockEntityTree blockEntity)
        {
            int count = blockEntity.Blocks.Length;
            if (count > 0)
            {
                int biasX = (blockEntity.Position.X >> 4) << 4;
                int biasZ = (blockEntity.Position.Z >> 4) << 4;

                // Удача, чем меньше число, тем больше фруктов
                // TODO::2026-02-10 удача для плодов, от корня можно или почвы
                int fortune = 10;

                BlockPosLoc posLoc;
                BlockPos pos = new BlockPos();
                BlockPos posLeaves;
                BlockState state;
                // Пробегаемся по всем блокам траектории дерева, и помечаем листву +256 на урожай
                int i, j, met;
                
                //int c1 = 0;
                //int c2 = 0;
                for (i = 0; i < count; i++)
                {
                    posLoc = blockEntity.Blocks[i];
                    pos.X = posLoc.X + biasX;
                    pos.Y = posLoc.Y;
                    pos.Z = posLoc.Z + biasZ;

                    for (j = 0; j < 6; j++) // 6 сторон ветки
                    {
                        posLeaves = pos.Offset(j);
                        state = world.GetBlockState(posLeaves);
                        if (state.Id == _blockLeavesId) // Если текущая ветка
                        {
                            met = state.Met;
                            if (met > 255) met = met & 0xF;
                            if (met > 5) met -= 6;
                            if (met == j && _NextBool(fortune)) // Если текущаяя сторона
                            {
                                world.SetBlockStateMet(posLeaves, state.Met & 0xF | 256, false);
                                //c2++;
                            }
                            //c1++;
                        }
                    }
                }
                //Console.WriteLine("Fetus " + blockEntity.Position + " " + c1 + "/" + c2);
            }
        }

        /// <summary>
        /// Дерево закончило выращивать плоды, надо пометить всю листву чтоб не выращивала
        /// </summary>
        private void _CancelFetus(WorldServer world, BlockEntityTree blockEntity)
        {
            int count = blockEntity.Blocks.Length;
            if (count > 0)
            {
                int biasX = (blockEntity.Position.X >> 4) << 4;
                int biasZ = (blockEntity.Position.Z >> 4) << 4;

                BlockPosLoc posLoc;
                BlockPos pos = new BlockPos();
                BlockPos posLeaves;
                BlockState state;
                // Пробегаемся по всем блокам траектории дерева, и помечаем листву -256
                int i, j, met;
                for (i = 0; i < count; i++)
                {
                    posLoc = blockEntity.Blocks[i];
                    pos.X = posLoc.X + biasX;
                    pos.Y = posLoc.Y;
                    pos.Z = posLoc.Z + biasZ;

                    for (j = 0; j < 6; j++) // 6 сторон ветки
                    {
                        posLeaves = pos.Offset(j);
                        state = world.GetBlockState(posLeaves);
                        if (state.Id == _blockLeavesId) // Если текущая ветка
                        {
                            met = state.Met;
                            if (met > 255)
                            {
                                met = met & 0xF;
                                if (met > 5) met -= 6;
                                if (met == j) // Если текущаяя сторона
                                {
                                    world.SetBlockStateMet(posLeaves, state.Met & 0xF, false);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
