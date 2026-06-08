using Mvk2.World.Block;
using Vge.Util;
using Vge.World.Block;
using Vge.World.Gen;
using WinGL.Util;

namespace Mvk2.World.Gen.Feature
{
    public class FeatureTreeConifer : FeatureTree
    {
        /// <summary>
        /// Сосна
        /// </summary>
        private bool _pine = true;

        public FeatureTreeConifer(ArrayFast<BlockCache> blockCaches, byte minRandom, byte maxRandom, IChunkPrimer chunkPrimer)
            : base(blockCaches, chunkPrimer, minRandom, maxRandom,
                  BlocksRegMvk.LogConifer.IndexBlock, BlocksRegMvk.BranchConifer.IndexBlock, BlocksRegMvk.LeavesConifer.IndexBlock)
        { }

        public FeatureTreeConifer(ArrayFast<BlockCache> blockCaches, byte probabilityOne, IChunkPrimer chunkPrimer)
            : base(blockCaches, chunkPrimer, probabilityOne,
                  BlocksRegMvk.LogConifer.IndexBlock, BlocksRegMvk.BranchConifer.IndexBlock, BlocksRegMvk.LeavesConifer.IndexBlock)
        { }

        public FeatureTreeConifer(ArrayFast<BlockCache> blockCaches) : base(blockCaches,
            BlocksRegMvk.LogConifer.IndexBlock, BlocksRegMvk.BranchConifer.IndexBlock, BlocksRegMvk.LeavesConifer.IndexBlock)
        { }

        /// <summary>
        /// Случайные атрибуты дерева
        /// </summary>
        protected override void _RandSize()
        {
            // определение сосны
            _pine = _NextInt(4) != 0;
            if (_pine)
            {
                // Высота ствола дерева до кроны
                _trunkHeight = _NextInt(9) + 20;
                // Ствол снизу без веток 
                _trunkWithoutBranches = _NextInt(3) + 4;
                // Количество секций веток для сужения
                //                _    / \
                //          _    / \  |   |
                //    _    / \  |   | |   |
                //   / \  |   | |   | |   |
                //   \ /   \ /   \ /   \ /
                //  2 |   3 |   4 |   5 |
                _sectionCountBranches = 3;
                // Минимальная обязательная длинна ветки
                _branchLengthMin = 5;
                // Случайная дополнительная длинна ветки к обязательной
                _branchLengthRand = 3;
                // Длинна корня, у дуба достаточно длинный (6 - 9)
                _rootLenght = _trunkHeight / 3;
            }
            else
            {
                // Высота ствола дерева до кроны
                _trunkHeight = _NextInt(10) + 10;
                // Ствол снизу без веток 
                _trunkWithoutBranches = _NextInt(3) + 1;
                // Минимальная обязательная длинна ветки
                _branchLengthMin = _trunkHeight / 4;
                // Случайная дополнительная длинна ветки к обязательной
                _branchLengthRand = _trunkHeight / 6;
                // Длинна корня, у дуба достаточно длинный (2 - 4)
                _rootLenght = _trunkHeight / 4;
            }
        }

        /// <summary>
        /// Сгенерировать взрослое древо в кеш блоки из локальных координат чанка
        /// </summary>
        protected override void _GenerationMature(int bx, int by, int bz)
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

            int y, iUp, iSide, iBranche;
            bool pineDown = false;
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

            // Cмещения веток по всем сторонам, чтоб не слипались друг с другом
            int row = 0;
            // Коэффициент длинны веток 0 - 1
            float factorLightBranches;
            // Количество блоков в секции
            int countBlockSection;
            // снизу вверх
            for (iUp = 0; iUp < count; iUp++)
            {
                y = by + iUp;
                // Ствол, если нижний то параметр для пенька
                if (iUp != 0)
                {
                    if (iUp <= _trunkWithoutBranches)
                    {
                        _AddBlockStump(bx, y, bz, 3, parentTrunk);
                    }
                    else
                    {
                        _AddBlockTrunk(bx, y, bz, _blockLogId, parentTrunk);
                    }
                    parentTrunk = ++parentAmount;
                }

                // Ветки
                if (iUp >= _trunkWithoutBranches)
                {
                    if (row == 0)
                    {
                        // Есть ветка

                        // параметр смещения ветки в 2 блока между ветками
                        row = 2;

                        // Находи коэффициент длинны веток
                        itwb = iUp - _trunkWithoutBranches;
                        if (_pine)
                        {
                            countBlockSection = trunkWithBranches / _sectionCountBranches;
                            factorLightBranches = itwb < countBlockSection ? itwb / (float)countBlockSection : itwb > trunkWithBranches - countBlockSection
                                ? (countBlockSection - (itwb - (trunkWithBranches - countBlockSection))) / (float)countBlockSection : 1;

                            // Больше сухих веток или меньше снизу
                            pineDown = itwb < trunkWithBranches / (_NextInt(2) + 2);

                            if (pineDown)
                            {
                                // Сокрощяем длинные сухие ветки
                                if (factorLightBranches > .25f) factorLightBranches /= 2f;
                                row = 1;
                            }
                        }
                        else
                        {
                            factorLightBranches = 1f - itwb / (float)trunkWithBranches;

                            if (factorLightBranches < .55f) row = 1;

                            // Листва вокруг ствола
                            _LeavesTrunk(bx, y, bz);
                        }

                        // цикл направлении веток
                        for (iSide = 0; iSide < 4; iSide++)
                        {
                            // определяем длинну ветки
                            lightBranche = (int)((_NextInt(_branchLengthRand) + _branchLengthMin) * factorLightBranches);

                            if (_pine && pineDown && _NextInt(3) == 0)
                            {
                                lightBranche = 0;
                            }

                            if (lightBranche > 0)
                            {
                                // Точно имеется ветка работаем с ней
                                parentBranch = parentTrunk;

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

                                    if (!_pine || (_pine && !pineDown))
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
                        row--;
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
        /// Листва на ветке, side = 0-3 горизонтальный вектор направление вектора MvkStatic.AreaOne4
        /// 0 - South, 1 - East, 2 - North, 3 - West
        /// </summary>
        protected override void _LeavesBranch(int x, int y, int z, int side)
        {
            if (side == 0) // South
            {
                // Все кроме North
                // East
                _AddBlockLeaves(x + 1, y, z, 2 + _NextInt(2) * 6);
                // West
                _AddBlockLeaves(x - 1, y, z, 3 + _NextInt(2) * 6);
                // South
                _AddBlockLeaves(x, y, z + 1, 5 + _NextInt(2) * 6);
            }
            else if (side == 1) // East
            {
                // Все кроме West
                // East
                _AddBlockLeaves(x + 1, y, z, 2 + _NextInt(2) * 6);
                // North
                _AddBlockLeaves(x, y, z - 1, 4 + _NextInt(2) * 6);
                // South
                _AddBlockLeaves(x, y, z + 1, 5 + _NextInt(2) * 6);
            }
            else if (side == 2) // North
            {
                // Все кроме South
                // East
                _AddBlockLeaves(x + 1, y, z, 2 + _NextInt(2) * 6);
                // West
                _AddBlockLeaves(x - 1, y, z, 3 + _NextInt(2) * 6);
                // North
                _AddBlockLeaves(x, y, z - 1, 4 + _NextInt(2) * 6);
            }
            else // West
            {
                // Все кроме East
                // West
                _AddBlockLeaves(x - 1, y, z, 3 + _NextInt(2) * 6);
                // North
                _AddBlockLeaves(x, y, z - 1, 4 + _NextInt(2) * 6);
                // South
                _AddBlockLeaves(x, y, z + 1, 5 + _NextInt(2) * 6);
            }
        }

        /// <summary>
        /// Сгенерировать стартовое положение в чанке
        /// </summary>
        protected override Vector2i _GetRandomPosBegin(Rand rand)
            => new Vector2i(rand.Next(8) * 2 + 1, rand.Next(8) * 2 + 1);
    }
}
