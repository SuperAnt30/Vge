using System.Collections.Generic;
using Vge.World.Block;
using Vge.World.Chunk;
using WinGL.Util;

namespace Vge.World.Light
{
    /// <summary>
    /// Обработка освещения для мира
    /// </summary>
    public class WorldLight
    {
        /// <summary>
        /// Первая группа
        /// Расчёт группы, ромб 31*31, это 481 пиксель. Умножаем на высоту блоков.
        /// </summary>
        private readonly int _indexStep2; // 388096
        /// <summary>
        /// Вторая группа
        /// </summary>
        private readonly int _indexStep3; // 776192
        /// <summary>
        /// Максималька из трёх групп
        /// </summary>
        private readonly int _indexStepMax; // 1164288

        /// <summary>
        /// Вспомогательный массив, значения в битах 000000LL LLzzzzzz yyyyyyyy yyxxxxxx
        /// index = x | y << 6 | z << 16 | L << 22; 
        /// x = index & 63;
        /// y = index >> 6 & 511;
        /// z = index >> 16 & 63;
        /// L = index >> 22 & 15;
        /// Это старый рассчёт с первой части, ощущение, что я ошибся! Должно быть меньше. 2024-12-05
        /// 388096 * 3, где 388096 максимально возможное количество блоков в чанке плюс соседние блоки
        /// (16 * 16 + (14 * 16 + 13 * 6 + 13) * 4) * 256 = 388 096 * 3 (для смещения и затемнения)
        /// </summary>
        private readonly int[] _arCache;
        /// <summary>
        /// Начальный индекс вспомогательного массива
        /// </summary>
        private int _indexBegin;
        /// <summary>
        /// Конечный индекс вспомогательного массива
        /// </summary>
        private int _indexEnd;
        /// <summary>
        /// Тикущий индекс вспомогательного массива
        /// </summary>
        private int _indexActive;

        /// <summary>
        /// Объект обрабатываемого мира
        /// </summary>
        private readonly WorldBase _world;
        /// <summary>
        /// Соседние чанки, заполняются перед рендером
        /// </summary>
        private readonly ChunkBase[] _chunks = new ChunkBase[8];
        /// <summary>
        /// Чанк в котором был запуск обработки освещения
        /// </summary>
        private ChunkBase _chunk;
        /// <summary>
        /// Координаты чанка, в котором был запуск освещения
        /// </summary>
        private int _chBeginX, _chBeginY;
        /// <summary>
        /// Смещение блока от основнока чанка
        /// </summary>
        private int _bOffsetX, _bOffsetZ;
        /// <summary>
        /// Количество изменённых блоков
        /// </summary>
        private int _countBlock = 0;
        /// <summary>
        /// Координаты изменения блоков
        /// </summary>
        private int _axisX0, _axisY0, _axisZ0, _axisX1, _axisY1, _axisZ1;
        /// <summary>
        /// Минимальная высота для соседнего блока
        /// </summary>
        private readonly int[] _yMin = new int[4];
        /// <summary>
        /// Максимальная высота для соседнего блока
        /// </summary>
        private readonly int[] _yMax = new int[4];

        private readonly byte[] _xz = new byte[] { 1, 0, 1, 0, 15, 0, 15, 0 };
        private readonly byte[] _poleX = new byte[] { 1, 0, 2, 0, 1, 0, 2, 0 };

        /// <summary>
        /// Отладочный стринг
        /// </summary>
        private string _debugStr = "";

        public WorldLight(WorldBase world, int numberBlocks)
        {
            _world = world;
            _indexStep2 = 481 * numberBlocks;
            _indexStep3 = _indexStep2 * 2;
            _indexStepMax = _indexStep2 + _indexStep3;
            _arCache = new int[_indexStepMax];
        }

        #region Debug

        /// <summary>
        /// Очистить отладочную строку
        /// </summary>
        public void ClearDebugString() => _debugStr = "";
        /// <summary>
        /// Вернуть отладочную строку
        /// </summary>
        public string ToDebugString() => _debugStr;
        /// <summary>
        /// Количество обработанных блоков для отладки
        /// </summary>
        public int GetCountBlock() => _countBlock;

        #endregion

        #region Public

        /// <summary>
        /// Активировать чанк в котором будет обработка освещения
        /// </summary>
        public void ActionChunk(ChunkBase chunk)
        {
            _chunk = chunk;
            _chBeginX = chunk.CurrentChunkX;
            _chBeginY = chunk.CurrentChunkY;
            _bOffsetX = _chBeginX << 4;
            _bOffsetZ = _chBeginY << 4;
        }

        /// <summary>
        /// Обновить кэш соседних чанков, если не загружен хоть один или отсутствует вернёт false
        /// </summary>
        public bool UpChunks()
        {
            for (int i = 0; i < 8; i++)
            {
                _chunks[i] = _world.ChunkPr.GetChunk(
                    _chBeginX + Ce.AreaOne8X[i], _chBeginY + Ce.AreaOne8Y[i]
                );
                if (_chunks[i] == null || !_chunks[i].IsChunkPresent) return false;
            }
            return true;
        }

        /// <summary>
        /// Небесное боковое освещение
        /// </summary>
        public void HeavenSideLighting()
        {
            _countBlock = 0;
            ChunkBase chunkCache;
            ChunkStorage chunkStorage;
            int i, x, y, z, yh, yh2, xReal, zReal, yhOpacity, lightSky;
            // координата блока
            int x0, z0, indexBlock;
            // смещение координат чанка от стартового
            int xco, zco;
            int xo, yo, zo;
            int yDown, yUp, yMin2, yMax2;
            byte lo;
            int yMax = _chunk.Settings.NumberBlocks + 1;
            bool begin = true;

            for (x = 0; x < 16; x++)
            {
                xReal = _bOffsetX + x;
                for (z = 0; z < 16; z++)
                {
                    zReal = _bOffsetZ + z;
                    // Определяем наивысшый непрозрачный блок текущего ряда
                    yhOpacity = _chunk.Light.GetHeightOpacity(x, z);
                    yh = _chunk.Light.GetHeight(x, z);
                    _yMin[0] = _yMin[1] = _yMin[2] = _yMin[3] = yMax;
                    _yMax[0] = _yMax[1] = _yMax[2] = _yMax[3] = 0;
                    for (i = 0; i < 4; i++) // цикл сторон блока по горизонту
                    {
                        // Позиция соседнего блока, глобальные координаты
                        x0 = Ce.AreaOne4X[i] + xReal;
                        z0 = Ce.AreaOne4Y[i] + zReal;
                        xco = (x0 >> 4) - _chBeginX;
                        zco = (z0 >> 4) - _chBeginY;
                        chunkCache = (xco == 0 && zco == 0) ? _chunk : _chunks[Ce.GetAreaOne8(xco, zco)];
                        // Позиция соседнего блока, координаты чанка
                        xo = x0 & 15;
                        zo = z0 & 15;
                        // Определяем наивысшый непрозрачный блок
                        yh2 = chunkCache.Light.GetHeight(xo, zo);

                        // Если соседний блок выше, начинаем обработку
                        if (yh < yh2)
                        {
                            // Координата Y от которой анализируем, на блок выше вверхней, так-как блок нам не интересен
                            yDown = yh;// + 1;
                            // Координата Y до которой анализируем, на блок ниже, так-как нам надо найти ущелие, а блок является перекрытием
                            yUp = yh2 - 1;
                            // Если нижняя координата ниже вверхней или равны, начинаем анализ
                            if (yDown <= yUp)
                            {
                                for (y = yDown; y <= yUp; y++) // цикл высоты
                                {
                                    yo = y;
                                    chunkStorage = chunkCache.StorageArrays[yo >> 4];
                                    indexBlock = (yo & 15) << 8 | zo << 4 | xo;
                                    if (chunkStorage.CountBlock != 0)
                                    {
                                        lo = Ce.Blocks.BlocksLightOpacity[chunkStorage.Data[indexBlock] & 0xFFF];
                                    }
                                    else lo = 0;
                                    if ((lo >> 4) < 14)
                                    {
                                        // Если блок прозрачный меняем высоты
                                        if (_yMin[i] > y) _yMin[i] = y;
                                        if (_yMax[i] < y) _yMax[i] = y;
                                    }
                                }
                            }
                        }
                    }
                    // Готовимся проверять высоты для изменения
                    yMin2 = yMax;
                    yMax2 = 0;
                    for (i = 0; i < 4; i++)
                    {
                        if (_yMin[i] < yMin2) yMin2 = _yMin[i];
                        if (_yMax[i] > yMax2) yMax2 = _yMax[i];
                    }
                    // Если перепад высот имеется, запускаем правку небесного освещения ввиде столба
                    if (yMin2 != yMax)
                    {
                        // Тест столба для визуализации ввиде стекла
                        if (begin)
                        {
                            begin = false;
                            _axisX0 = _axisX1 = xReal;
                            _axisY0 = _axisY1 = yMin2;
                            _axisZ0 = _axisZ1 = zReal;
                            _indexBegin = _indexEnd = 0;
                        }
                        for (y = yMin2; y <= yMax2; y++)
                        {
                            if (xReal < _axisX0) _axisX0 = xReal; else if (xReal > _axisX1) _axisX1 = xReal;
                            if (y < _axisY0) _axisY0 = y; else if (y > _axisY1) _axisY1 = y;
                            if (zReal < _axisZ0) _axisZ0 = zReal; else if (zReal > _axisZ1) _axisZ1 = zReal;
                            _arCache[_indexEnd++] = (xReal - _bOffsetX + 32 | y << 6 | zReal - _bOffsetZ + 32 << 16 | 15 << 22);
                            //World.SetBlockDebug(new BlockPos(xReal, y, zReal), EnumBlock.Glass);
                        }

                    }
                    // Проверка после полупрозрачных блоков
                    if (yhOpacity < yh)
                    {
                        if (begin)
                        {
                            begin = false;
                            _axisX0 = _axisX1 = xReal;
                            _axisY0 = _axisY1 = Mth.Min(yMin2, yh);
                            _axisZ0 = _axisZ1 = zReal;
                            _indexBegin = _indexEnd = 0;
                        }

                        for (y = yhOpacity; y < yh; y++)
                        {

                            if (xReal < _axisX0) _axisX0 = xReal; else if (xReal > _axisX1) _axisX1 = xReal;
                            if (y < _axisY0) _axisY0 = y; else if (y > _axisY1) _axisY1 = y;
                            if (zReal < _axisZ0) _axisZ0 = zReal; else if (zReal > _axisZ1) _axisZ1 = zReal;
                            lightSky = _chunk.StorageArrays[y >> 4].Light[(y & 15) << 8 | z << 4 | x] & 15;
                            _arCache[_indexEnd++] = (xReal - _bOffsetX + 32 | y << 6 | zReal - _bOffsetZ + 32 << 16 | lightSky << 22);
                            //World.SetBlockDebug(new BlockPos(xReal, y, zReal), EnumBlock.Glass);
                        }
                    }
                }
            }

            //World.SetBlockDebug(new BlockPos(bOffsetX + 8, 90, bOffsetZ + 8), EnumBlock.Glass);
            _BrighterLightSky();
        }

        /// <summary>
        /// Проверяем освещение блоков при старте
        /// </summary>
        public void CheckBrighterLightBlocks(List<uint> lightBlocks)
        {
            _countBlock = 0;
            ChunkStorage chunkStorage;
            int i, x, y, z, indexBlock;
            // псевдочанк
            int yco;
            // значения LightValue и LightOpacity
            byte lo;
            uint index = lightBlocks[0];
            x = (int)(index & 15) + _bOffsetX;
            y = (int)index >> 8;
            z = (int)((index >> 4) & 15) + _bOffsetZ;
            _axisX0 = _axisX1 = x;
            _axisY0 = _axisY1 = y;
            _axisZ0 = _axisZ1 = z;
            _indexBegin = _indexEnd = 0;
            _countBlock = 0;

            for (i = 0; i < lightBlocks.Count; i++)
            {
                index = lightBlocks[i];
                x = (int)(index & 15) + _bOffsetX;
                y = (int)index >> 8;
                z = (int)((index >> 4) & 15) + _bOffsetZ;
                if (x < _axisX0) _axisX0 = x; else if (x > _axisX1) _axisX1 = x;
                if (y < _axisY0) _axisY0 = y; else if (y > _axisY1) _axisY1 = y;
                if (z < _axisZ0) _axisZ0 = z; else if (z > _axisZ1) _axisZ1 = z;
                yco = y >> 4;
                chunkStorage = _chunk.StorageArrays[yco];
                if (chunkStorage.CountBlock != 0)
                {
                    indexBlock = (y & 15) << 8 | (z & 15) << 4 | (x & 15);
                    lo = Ce.Blocks.BlocksLightOpacity[chunkStorage.Data[indexBlock] & 0xFFF];
                    chunkStorage.Light[indexBlock] = (byte)((lo & 0xF) << 4 | chunkStorage.Light[indexBlock] & 15);
                    _arCache[_indexEnd++] = x - _bOffsetX + 32 | y << 6 | z - _bOffsetZ + 32 << 16 | lo << 22;
                }
            }

            _BrighterLightBlock();
        }

        /// <summary>
        /// Проверяем освещение блока и неба при изменении блока
        /// </summary>
        /// <param name="x">глобальная позиция блока x</param>
        /// <param name="y">глобальная позиция блока y</param>
        /// <param name="z">глобальная позиция блока z</param>
        /// <param name="differenceOpacity">Разница в непрозрачности, только для небесного освещения</param>
        /// <param name="replaceAir">блок заменён на воздух или на оборот воздух заменён на блок</param>
        public void CheckLightFor(BlockPos blockPos, bool differenceOpacity, bool isModify, bool isModifyRender)
        {
            int x = blockPos.X;
            int y = blockPos.Y;
            int z = blockPos.Z;
            if (y < 0 || y > _world.ChunkPr.Settings.NumberBlocks) return;

            long timeBegin = _world.ElapsedTicks();

            if (!UpChunks()) return;

            ChunkStorage chunkStorage = _chunk.StorageArrays[y >> 4];
            int index = (y & 15) << 8 | (z & 15) << 4 | (x & 15);
            byte lo = 0;
            if (chunkStorage.CountBlock != 0)
            {
                lo = Ce.Blocks.BlocksLightOpacity[chunkStorage.Data[index] & 0xFFF];
            }
            // текущаяя яркость
            byte lightOld = (byte)(chunkStorage.Light[index] >> 4);
            // яркость от блока
            byte lightBlock = (byte)(lo & 0xF);
            // яркость от соседних блоков
            byte lightBeside = _GetLevelBrightBlock(x, y, z, lo);
            // яркость какая по итогу
            byte lightNew = lightBeside > lightBlock ? lightBeside : lightBlock;

            // SET яркость блока
            chunkStorage.Light[index] = (byte)(lightNew << 4 | chunkStorage.Light[index] & 15);
            _axisX0 = _axisX1 = x;
            _axisY0 = _axisY1 = y;
            _axisZ0 = _axisZ1 = z;
            _indexBegin = _indexEnd = 0;
            _countBlock = 0;

            if (lightOld < lightNew)
            {
                // Осветлить
                _arCache[_indexEnd++] = (x - _bOffsetX + 32 | y << 6 | z - _bOffsetZ + 32 << 16 | lightNew << 22);
                _BrighterLightBlock();
            }
            else if (lightOld > lightNew)
            {
                // Затемнить
                _arCache[_indexEnd++] = (x - _bOffsetX + 32 | y << 6 | z - _bOffsetZ + 32 << 16 | lightOld << 22);
                _DarkenLightBlock();
                _BrighterLightBlock();
            }
            int c1 = _countBlock;
            // Проверка яркости неба
            if (differenceOpacity)// || replaceAir)
            {
                _chunk.Light.CheckLightSky(x, y, z, lo);
            }

            // Обновление для рендера чанков
            if (isModifyRender)
            {
                if (_countBlock <= 1 && _axisX0 == _axisX1 && _axisY0 == _axisY1 && _axisZ0 == _axisZ1)
                {
                    _world.MarkBlockForUpdate(_axisX0, _axisY0, _axisZ0);
                }
                else
                {
                    _world.MarkBlockRangeForUpdate(_axisX0, _axisY0, _axisZ0, _axisX1, _axisY1, _axisZ1);
                }
            }
            // Сохранение чанков
            if (isModify)
            {
                _world.MarkBlockRangeForModified(_axisX0, _axisZ0, _axisX1, _axisZ1);
            }

            long timeEnd = _world.ElapsedTicks();
            _debugStr = string.Format("Count B/S: {1}/{2} Light: {0:0.00}ms",
                (timeEnd - timeBegin) / (float)Ce.FrequencyMs, c1, _countBlock - c1);
        }

        /// <summary>
        /// Проверка блока небесного освещения
        /// </summary>
        /// <param name="x">глобальная позиция блока x</param>
        /// <param name="y">глобальная позиция блока y</param>
        /// <param name="z">глобальная позиция блока z</param>
        /// <param name="lo">прозрачности и излучаемости освещения</param>
        public void CheckLightSky(int x, int y, int z, byte lo)
        {
            ChunkStorage chunkStorage = _chunk.StorageArrays[y >> 4];
            int index = (y & 15) << 8 | (z & 15) << 4 | (x & 15);
            // Один блок проверки, не видящий неба
            byte lightOld = (byte)(chunkStorage.Light[index] & 15);
            // яркость от соседних блоков
            byte lightBeside = _GetLevelBrightSky(x, y, z, lo);

            if (lightOld != lightBeside)
            {
                _indexBegin = _indexEnd = 0;
                if (x < _axisX0) _axisX0 = x; else if (x > _axisX1) _axisX1 = x;
                if (y < _axisY0) _axisY0 = y; else if (y > _axisY1) _axisY1 = y;
                if (z < _axisZ0) _axisZ0 = z; else if (z > _axisZ1) _axisZ1 = z;
            }

            if (lightOld < lightBeside)
            {
                // Осветлить
                chunkStorage.Light[index] = (byte)(chunkStorage.Light[index] >> 4 << 4 | lightBeside);
                _arCache[_indexEnd++] = x - _bOffsetX + 32 | y << 6 | z - _bOffsetZ + 32 << 16 | lightBeside << 22;
                _BrighterLightSky();
            }
            else
            {
                // Затемнить
                chunkStorage.Light[index] = (byte)(chunkStorage.Light[index] >> 4 << 4);
                _arCache[_indexEnd++] = x - _bOffsetX + 32 | y << 6 | z - _bOffsetZ + 32 << 16 | lightOld << 22;
                _DarkenLightSky();
                _BrighterLightSky();
            }
        }

        /// <summary>
        /// Осветление столба блоков неба
        /// </summary>
        public void BrighterLightColumnSky(int x, int y0, int z, int y1)
        {
            _indexBegin = _indexEnd = 0;
            int index;
            for (int y = y0; y < y1; y++)
            {
                index = (y & 15) << 8 | (z & 15) << 4 | (x & 15);
                _chunk.StorageArrays[y >> 4].Light[index] = (byte)(_chunk.StorageArrays[y >> 4].Light[index] >> 4 << 4 | 15);
                _arCache[_indexEnd++] = x - _bOffsetX + 32 | y << 6 | z - _bOffsetZ + 32 << 16 | 15 << 22;
                if (x < _axisX0) _axisX0 = x; else if (x > _axisX1) _axisX1 = x;
                if (y < _axisY0) _axisY0 = y; else if (y > _axisY1) _axisY1 = y;
                if (z < _axisZ0) _axisZ0 = z; else if (z > _axisZ1) _axisZ1 = z;
            }
            _BrighterLightSky();
        }

        /// <summary>
        /// Затемнение столба блоков неба
        /// </summary>
        public void DarkenLightColumnSky(int x, int y0, int z, int y1)
        {
            _indexBegin = _indexEnd = 0;
            int yco = y1 >> 4;
            if (yco > _world.ChunkPr.Settings.NumberSectionsLess)
            {
                yco = _world.ChunkPr.Settings.NumberSectionsLess;
            }
            int index, yco2;
            for (int y = y0; y < y1; y++)
            {
                index = (y & 15) << 8 | (z & 15) << 4 | (x & 15);
                yco2 = y >> 4;
                byte l = (byte)(_chunk.StorageArrays[yco2].Light[index] & 15);
                _chunk.StorageArrays[yco2].Light[index] = (byte)(_chunk.StorageArrays[yco2].Light[index] >> 4 << 4);
                _arCache[_indexEnd++] = x - _bOffsetX + 32 | y << 6 | z - _bOffsetZ + 32 << 16 | l << 22;
                if (x < _axisX0) _axisX0 = x; else if (x > _axisX1) _axisX1 = x;
                if (y < _axisY0) _axisY0 = y; else if (y > _axisY1) _axisY1 = y;
                if (z < _axisZ0) _axisZ0 = z; else if (z > _axisZ1) _axisZ1 = z;
            }
            if (y1 > _world.ChunkPr.Settings.NumberBlocks)
            {
                y1 = _world.ChunkPr.Settings.NumberBlocks;
            }
            _DarkenLightSky();
            index = (y1 & 15) << 8 | (z & 15) << 4 | (x & 15);
            _chunk.StorageArrays[yco].Light[index] = (byte)(_chunk.StorageArrays[yco].Light[index] >> 4 << 4 | 15);
            _arCache[_indexEnd++] = x - _bOffsetX + 32 | y1 << 6 | z - _bOffsetZ + 32 << 16 | 15 << 22;
            _BrighterLightSky();
        }

        #endregion

        #region Sky

        /// <summary>
        /// Осветляем небесный массив
        /// </summary>
        private bool _BrighterLightSky()
        {
            ChunkBase chunkCache;
            ChunkStorage chunkStorage;
            byte lightB;
            int lightNew, lightCheck;
            int iSide, iList;
            // вектор стороны
            Vector3i vec;
            // координаты с листа
            int x, y, z;
            // свет от листа
            int light;
            // значение индекса
            int listIndex;
            // координата блока
            int x0, y0, z0, indexBlock;
            // смещение координат чанка от стартового
            int xco, zco;
            // псевдочанк
            int yco;
            // значения LightValue и LightOpacity
            byte lo;
            _indexActive = _indexBegin == 0 ? _indexStep2 : 0;

            int numberBlocks = _world.ChunkPr.Settings.NumberBlocks;

            if (_indexEnd - _indexBegin == 0) return false;
            // Цикл обхода по древу, уровневым метод (он же ширину (breadth-first search, BFS))
            while (_indexEnd - _indexBegin > 0)
            {
                for (iList = _indexBegin; iList < _indexEnd; iList++)
                {
                    listIndex = _arCache[iList];
                    x = (listIndex & 63) - 32 + _bOffsetX;
                    y = (listIndex >> 6 & 511);
                    z = (listIndex >> 16 & 63) - 32 + _bOffsetZ;
                    light = listIndex >> 22 & 15;
                    // соседние блоки
                    for (iSide = 0; iSide < 6; iSide++) // Цикл сторон
                    {
                        vec = BlockPos.DirectionVectors[iSide];
                        y0 = y + vec.Y;
                        if (y0 < 0 || y0 > numberBlocks) continue;
                        x0 = x + vec.X;
                        z0 = z + vec.Z;
                        yco = y0 >> 4;
                        xco = (x0 >> 4) - _chBeginX;
                        zco = (z0 >> 4) - _chBeginY;
                        chunkCache = (xco == 0 && zco == 0) ? _chunk : _chunks[Ce.GetAreaOne8(xco, zco)];
                        chunkStorage = chunkCache.StorageArrays[yco];
                        indexBlock = (y0 & 15) << 8 | (z0 & 15) << 4 | (x0 & 15);
                        if (chunkStorage.CountBlock != 0)
                        {
                            lo = Ce.Blocks.BlocksLightOpacity[chunkStorage.Data[indexBlock] & 0xFFF];
                        }
                        else lo = 0;
                        lightNew = chunkStorage.Light[indexBlock] & 15;
                        // Определяем яркость, какая должна
                        lightCheck = light - (lo >> 4) - 1;
                        if (lightCheck < 0) lightCheck = 0;
                        if (lightNew >= lightCheck) continue;
                        // Если тикущая темнее, осветляем её
                        lightB = (byte)lightCheck;

                        chunkStorage.Light[indexBlock] = (byte)(chunkStorage.Light[indexBlock] >> 4 << 4 | lightB);
                        _countBlock++;

                        if (x0 < _axisX0) _axisX0 = x0; else if (x0 > _axisX1) _axisX1 = x0;
                        if (y0 < _axisY0) _axisY0 = y0; else if (y0 > _axisY1) _axisY1 = y0;
                        if (z0 < _axisZ0) _axisZ0 = z0; else if (z0 > _axisZ1) _axisZ1 = z0;
                        _arCache[_indexActive++] = (x0 - _bOffsetX + 32 | y0 << 6 | z0 - _bOffsetZ + 32 << 16 | lightB << 22);
                    }
                }

                _indexEnd = _indexActive;
                if (_indexBegin == 0)
                {
                    _indexBegin = _indexStep2;
                    _indexActive = 0;
                }
                else
                {
                    _indexBegin = 0;
                    _indexActive = _indexStep2;
                }
            }

            return true;
        }

        /// <summary>
        /// Затемнить небесный массив
        /// </summary>
        private void _DarkenLightSky()
        {
            ChunkBase chunkCache;
            ChunkStorage chunkStorage;
            byte lightB;
            int lightNew;
            int iSide, iList;
            int numberBlocks = _world.ChunkPr.Settings.NumberBlocks;
            // вектор стороны
            Vector3i vec;
            // координаты с листа
            int x, y, z;
            // свет от листа
            int light;
            // значение индекса
            int listIndex;
            // координата блока
            int x0, y0, z0, indexBlock;
            // смещение координат чанка от стартового
            int xco, zco;
            // псевдочанк
            int yco;
            _indexActive = _indexBegin == 0 ? _indexStep2 : 0;
            // Индекс для массива осветления
            int indexBrighter = _indexStep3;
            bool isAgainstSky;
            // Цикл обхода по древу, уровневым метод (он же ширину (breadth-first search, BFS))
            while (_indexEnd - _indexBegin > 0)
            {
                for (iList = _indexBegin; iList < _indexEnd; iList++)
                {
                    listIndex = _arCache[iList];
                    x = (listIndex & 63) - 32 + _bOffsetX;
                    y = (listIndex >> 6 & 511);
                    z = (listIndex >> 16 & 63) - 32 + _bOffsetZ;
                    light = listIndex >> 22 & 15;
                    // соседние блоки
                    for (iSide = 0; iSide < 6; iSide++) // Цикл сторон
                    {
                        vec = BlockPos.DirectionVectors[iSide];
                        y0 = y + vec.Y;
                        if (y0 < 0 || y0 > numberBlocks) continue;
                        x0 = x + vec.X;
                        z0 = z + vec.Z;
                        yco = y0 >> 4;
                        xco = (x0 >> 4) - _chBeginX;
                        zco = (z0 >> 4) - _chBeginY;
                        chunkCache = (xco == 0 && zco == 0) ? _chunk : _chunks[Ce.GetAreaOne8(xco, zco)];
                        chunkStorage = chunkCache.StorageArrays[yco];
                        indexBlock = (y0 & 15) << 8 | (z0 & 15) << 4 | (x0 & 15);
                        lightNew = chunkStorage.Light[indexBlock] & 15;
                        // Если фактическая яркость больше уровня прохода,
                        // значит зацепили соседний источник света, 
                        // прерываем с будущей пометкой на проход освещения
                        isAgainstSky = chunkCache.Light.IsAgainstSky(x0, y0, z0);
                        if ((lightNew >= light && lightNew > 1) || isAgainstSky)
                        {
                            if (isAgainstSky) lightNew = 15;
                            lightB = (byte)lightNew;
                            _arCache[indexBrighter++] = x0 - _bOffsetX + 32 | y0 << 6 | z0 - _bOffsetZ + 32 << 16 | lightB << 22;
                        }
                        // Проход затемнения без else от прошлого, из-за плафонов, они в обоих случаях могут быть
                        if (lightNew < light && lightNew > 0)
                        {
                            lightNew = light - 1;
                            if (lightNew > 0)
                            {
                                lightB = (byte)lightNew;
                                chunkStorage.Light[indexBlock] = (byte)(chunkStorage.Light[indexBlock] >> 4 << 4 | (isAgainstSky ? 15 : 0));
                                _countBlock++;
                                if (x0 < _axisX0) _axisX0 = x0; else if (x0 > _axisX1) _axisX1 = x0;
                                if (y0 < _axisY0) _axisY0 = y0; else if (y0 > _axisY1) _axisY1 = y0;
                                if (z0 < _axisZ0) _axisZ0 = z0; else if (z0 > _axisZ1) _axisZ1 = z0;
                                _arCache[_indexActive++] = x0 - _bOffsetX + 32 | y0 << 6 | z0 - _bOffsetZ + 32 << 16 | lightB << 22;
                            }
                        }
                    }
                }
                _indexEnd = _indexActive;
                if (_indexBegin == 0)
                {
                    _indexBegin = _indexStep2;
                    _indexActive = 0;
                }
                else
                {
                    _indexBegin = 0;
                    _indexActive = _indexStep2;
                }
            }
            _indexEnd = indexBrighter;
            _indexActive = 0;
            _indexBegin = _indexStep3;
        }

        #endregion

        #region Block

        /// <summary>
        /// Осветляем блочный массив
        /// </summary>
        private bool _BrighterLightBlock()
        {
            ChunkBase chunkCache;
            ChunkStorage chunkStorage;
            byte lightB;
            int lightNew, lightCheck;
            int iSide, iList;
            // вектор стороны
            Vector3i vec;
            // координаты с листа
            int x, y, z;
            // свет от листа
            int light;
            // значение индекса
            int listIndex;
            // координата блока
            int x0, y0, z0, indexBlock;
            // смещение координат чанка от стартового
            int xco, zco;
            // псевдочанк
            int yco;
            // значения LightValue и LightOpacity
            byte lo;
            _indexActive = _indexBegin == 0 ? _indexStep2 : 0;

            int numberBlocks = _world.ChunkPr.Settings.NumberBlocks;

            if (_indexEnd - _indexBegin == 0) return false;
            // Цикл обхода по древу, уровневым метод (он же ширину (breadth-first search, BFS))
            while (_indexEnd - _indexBegin > 0)
            {
                for (iList = _indexBegin; iList < _indexEnd; iList++)
                {
                    listIndex = _arCache[iList];
                    x = (listIndex & 63) - 32 + _bOffsetX;
                    y = (listIndex >> 6 & 511);
                    z = (listIndex >> 16 & 63) - 32 + _bOffsetZ;
                    light = listIndex >> 22 & 15;
                    // соседние блоки
                    for (iSide = 0; iSide < 6; iSide++) // Цикл сторон
                    {
                        vec = BlockPos.DirectionVectors[iSide];
                        y0 = y + vec.Y;
                        if (y0 < 0 || y0 > numberBlocks) continue;
                        x0 = x + vec.X;
                        z0 = z + vec.Z;
                        yco = y0 >> 4;
                        xco = (x0 >> 4) - _chBeginX;
                        zco = (z0 >> 4) - _chBeginY;
                        chunkCache = (xco == 0 && zco == 0) ? _chunk : _chunks[Ce.GetAreaOne8(xco, zco)];
                        chunkStorage = chunkCache.StorageArrays[yco];
                        indexBlock = (y0 & 15) << 8 | (z0 & 15) << 4 | (x0 & 15);
                        if (chunkStorage.CountBlock != 0)
                        {
                            lo = Ce.Blocks.BlocksLightOpacity[chunkStorage.Data[indexBlock] & 0xFFF];
                        }
                        else lo = 0;
                        lightNew = chunkStorage.Light[indexBlock] >> 4;
                        // Определяем яркость, какая должна
                        lightCheck = light - (lo >> 4) - 1;
                        if (lightCheck < 0) lightCheck = 0;
                        if (lightNew >= lightCheck) continue;
                        // Если тикущая темнее, осветляем её
                        lightB = (byte)lightCheck;

                        chunkStorage.Light[indexBlock] = (byte)(lightB << 4 | chunkStorage.Light[indexBlock] & 15);
                        _countBlock++;

                        if (x0 < _axisX0) _axisX0 = x0; else if (x0 > _axisX1) _axisX1 = x0;
                        if (y0 < _axisY0) _axisY0 = y0; else if (y0 > _axisY1) _axisY1 = y0;
                        if (z0 < _axisZ0) _axisZ0 = z0; else if (z0 > _axisZ1) _axisZ1 = z0;
                        _arCache[_indexActive++] = (x0 - _bOffsetX + 32 | y0 << 6 | z0 - _bOffsetZ + 32 << 16 | lightB << 22);
                    }
                }

                _indexEnd = _indexActive;
                if (_indexBegin == 0)
                {
                    _indexBegin = _indexStep2;
                    _indexActive = 0;
                }
                else
                {
                    _indexBegin = 0;
                    _indexActive = _indexStep2;
                }
            }
            return true;
        }

        /// <summary>
        /// Затемнить блочный массив
        /// </summary>
        private void _DarkenLightBlock()
        {
            ChunkBase chunkCache;
            ChunkStorage chunkStorage;
            byte lightB;
            int lightNew, lightCheck;
            int iSide, iList;
            int numberBlocks = _world.ChunkPr.Settings.NumberBlocks;
            // вектор стороны
            Vector3i vec;
            // координаты с листа
            int x, y, z;
            // свет от листа
            int light;
            // значение индекса
            int listIndex;
            // координата блока
            int x0, y0, z0, indexBlock;
            // смещение координат чанка от стартового
            int xco, zco;
            // псевдочанк
            int yco;
            // значения LightValue и LightOpacity
            byte lo;
            _indexActive = _indexBegin == 0 ? _indexStep2 : 0;
            // Индекс для массива осветления
            int indexBrighter = _indexStep3;
            // Цикл обхода по древу, уровневым метод (он же ширину (breadth-first search, BFS))
            while (_indexEnd - _indexBegin > 0)
            {
                for (iList = _indexBegin; iList < _indexEnd; iList++)
                {
                    listIndex = _arCache[iList];
                    x = (listIndex & 63) - 32 + _bOffsetX;
                    y = (listIndex >> 6 & 511);
                    z = (listIndex >> 16 & 63) - 32 + _bOffsetZ;
                    light = listIndex >> 22 & 15;
                    // соседние блоки
                    for (iSide = 0; iSide < 6; iSide++) // Цикл сторон
                    {
                        vec = BlockPos.DirectionVectors[iSide];
                        y0 = y + vec.Y;
                        if (y0 < 0 || y0 > numberBlocks) continue;
                        x0 = x + vec.X;
                        z0 = z + vec.Z;
                        yco = y0 >> 4;
                        xco = (x0 >> 4) - _chBeginX;
                        zco = (z0 >> 4) - _chBeginY;
                        chunkCache = (xco == 0 && zco == 0) ? _chunk : _chunks[Ce.GetAreaOne8(xco, zco)];
                        chunkStorage = chunkCache.StorageArrays[yco];
                        indexBlock = (y0 & 15) << 8 | (z0 & 15) << 4 | (x0 & 15);
                        if (chunkStorage.CountBlock != 0)
                        {
                            lo = Ce.Blocks.BlocksLightOpacity[chunkStorage.Data[indexBlock] & 0xFFF];
                        }
                        else lo = 0;
                        lightNew = chunkStorage.Light[indexBlock] >> 4;
                        lightCheck = lo & 15;
                        // Если фактическая яркость больше уровня прохода,
                        // значит зацепили соседний источник света, 
                        // прерываем с будущей пометкой на проход освещения
                        if (lightNew >= light && lightNew > 1 || lightCheck > 0)
                        {
                            if (lightCheck > 0) lightNew = lightCheck;
                            lightB = (byte)lightNew;
                            _arCache[indexBrighter++] = x0 - _bOffsetX + 32 | y0 << 6 | z0 - _bOffsetZ + 32 << 16 | lightB << 22;
                        }
                        // Проход затемнения без else от прошлого, из-за плафонов, они в обоих случаях могут быть
                        if (lightNew < light && lightNew > 0)
                        {
                            lightNew = light - 1;
                            if (lightNew > 0)
                            {
                                lightB = (byte)lightNew;
                                chunkStorage.Light[indexBlock] = (byte)(lightCheck << 4 | chunkStorage.Light[indexBlock] & 15);
                                _countBlock++;
                                if (x0 < _axisX0) _axisX0 = x0; else if (x0 > _axisX1) _axisX1 = x0;
                                if (y0 < _axisY0) _axisY0 = y0; else if (y0 > _axisY1) _axisY1 = y0;
                                if (z0 < _axisZ0) _axisZ0 = z0; else if (z0 > _axisZ1) _axisZ1 = z0;
                                _arCache[_indexActive++] = x0 - _bOffsetX + 32 | y0 << 6 | z0 - _bOffsetZ + 32 << 16 | lightB << 22;
                            }
                        }
                    }
                }
                _indexEnd = _indexActive;
                if (_indexBegin == 0)
                {
                    _indexBegin = _indexStep2;
                    _indexActive = 0;
                }
                else
                {
                    _indexBegin = 0;
                    _indexActive = _indexStep2;
                }
            }
            _indexEnd = indexBrighter;
            _indexActive = 0;
            _indexBegin = _indexStep3;
        }

        #endregion

        #region private Get

        /// <summary>
        /// Получить уровень яркости блока, глобальные  координаты
        /// </summary>
        private byte _GetLightBlock(int x, int y, int z)
        {
            int yc = y >> 4;
            int xc = (x >> 4) - _chBeginX;
            int zc = (z >> 4) - _chBeginY;
            if (xc == 0 && zc == 0)
            {
                return (byte)(_chunk.StorageArrays[yc].Light[(y & 15) << 8 | (z & 15) << 4 | (x & 15)] >> 4);
            }
            return (byte)(_chunks[Ce.GetAreaOne8(xc, zc)].StorageArrays[yc].Light[(y & 15) << 8 | (z & 15) << 4 | (x & 15)] >> 4);
        }

        /// <summary>
        /// Получить уровень яркости блока, глобальные  координаты
        /// </summary>
        private byte _GetLightSky(int x, int y, int z)
        {
            int yc = y >> 4;
            int xc = (x >> 4) - _chBeginX;
            int zc = (z >> 4) - _chBeginY;
            if (xc == 0 && zc == 0)
            {
                return (byte)(_chunk.StorageArrays[yc].Light[(y & 15) << 8 | (z & 15) << 4 | (x & 15)] & 15);
            }
            return (byte)(_chunks[Ce.GetAreaOne8(xc, zc)].StorageArrays[yc].Light[(y & 15) << 8 | (z & 15) << 4 | (x & 15)] & 15);
        }

        /// <summary>
        /// Возвращает уровень яркости блока, анализируя соседние блоки, глобальные  координаты
        /// </summary>
        private byte _GetLevelBrightBlock(int x, int y, int z, byte lo)
        {
            // Количество излучаемого света
            int light = lo & 0xF;
            // Сколько света вычитается для прохождения этого блока
            int opacity = lo >> 4;
            if (opacity >= 15 && light > 0) opacity = 1;
            if (opacity < 1) opacity = 1;

            // Если блок не проводит свет, значит темно
            if (opacity >= 15) return 0;
            // Если блок яркий выводим значение
            if (light >= 14) return (byte)light;

            int numberBlocks = _world.ChunkPr.Settings.NumberBlocks;

            // вектор стороны
            Vector3i vec;
            int lightNew, y2;
            // обрабатываем соседние блоки, вдруг рядом плафон ярче, чтоб не затемнить
            for (int iSide = 0; iSide < 6; iSide++)
            {
                vec = BlockPos.DirectionVectors[iSide];
                y2 = y + vec.Y;
                if (y2 >= 0 && y2 <= numberBlocks)
                {
                    lightNew = _GetLightBlock(x + vec.X, y2, z + vec.Z) - opacity;
                    // Если соседний блок ярче текущего блока
                    if (lightNew > light) light = lightNew;
                    // Если блок яркий выводим значение
                    if (light >= 14) return (byte)light;
                }
            }
            return (byte)light;
        }

        /// <summary>
        /// Возвращает уровень яркости блока, анализируя соседние блоки, глобальные  координаты
        /// </summary>
        private byte _GetLevelBrightSky(int x, int y, int z, byte lo)
        {
            // Сколько света вычитается для прохождения этого блока
            int opacity = lo >> 4;
            if (opacity < 1 || opacity >= 15) opacity = 1;

            int numberBlocks = _world.ChunkPr.Settings.NumberBlocks;
            // Количество излучаемого света
            int light = 0;
            // вектор стороны
            Vector3i vec;
            int lightNew, y2;
            // обрабатываем соседние блоки, вдруг рядом плафон ярче, чтоб не затемнить
            for (int iSide = 0; iSide < 6; iSide++)
            {
                vec = BlockPos.DirectionVectors[iSide];
                y2 = y + vec.Y;
                if (y2 >= 0 && y2 <= numberBlocks)
                {
                    lightNew = _GetLightSky(x + vec.X, y2, z + vec.Z) - opacity;
                    // Если соседний блок ярче текущего блока
                    if (lightNew > light) light = lightNew;
                    // Если блок яркий выводим значение
                    if (light >= 14) return (byte)light;
                }
                else
                {
                    return 15;
                }
            }
            return (byte)light;
        }

        #endregion

        #region CheckLoaded

        /// <summary>
        /// Проверяем освещение если соседний чанк загружен, основной сгенериорван.
        /// В загруженном может быть изменение по свету, которое перетикает в соседний чанк, который только, что сгенерировали.
        /// </summary>
        public void CheckBrighterLightLoaded()
        {
            for (int i = 0; i < 8; i += 2)
            {
                if (_chunks[i].IsLoaded)
                {
                    // Соседний загружен с ним и работаем
                    if (_poleX[i] == 2)
                    {
                        _CheckBrighterLightLoadedBlockX(_chunks[i], _xz[i]);
                        _CheckBrighterLightLoadedSkyX(_chunks[i], _xz[i]);
                    }
                    if (_poleX[i] == 1)
                    {
                        _CheckBrighterLightLoadedBlockZ(_chunks[i], _xz[i]);
                        _CheckBrighterLightLoadedSkyZ(_chunks[i], _xz[i]);
                    }
                }
            }
        }

        // Для ускорения 4 метода похожи, чтоб дополнительных if-ов

        /// <summary>
        /// Проверка блочного по X
        /// </summary>
        private void _CheckBrighterLightLoadedBlockX(ChunkBase chunk, int x)
        {
            int yh, yco, indexBlock;
            byte light;
            ChunkStorage chunkStorage;
            _indexBegin = _indexEnd = 0;
            int offset = x == 1 ? -15 : 16;
            for (int z = 0; z < 16; z++)
            {
                yh = chunk.Light.GetHeight(x, z);
                for (int y = 0; y < yh; y++)
                {
                    yco = y >> 4;
                    chunkStorage = chunk.StorageArrays[yco];
                    if (chunkStorage.CountBlock != 0)
                    {
                        indexBlock = (y & 15) << 8 | (z & 15) << 4 | (x & 15);
                        light = (byte)(chunkStorage.Light[indexBlock] >> 4);
                        if (light > 0)
                        {
                            _arCache[_indexEnd++] = x - offset + 32 | y << 6 | z + 32 << 16 | light << 22;
                        }
                    }
                }
            }
            _BrighterLightBlock();
        }
        /// <summary>
        /// Проверка блочного по Z
        /// </summary>
        private void _CheckBrighterLightLoadedBlockZ(ChunkBase chunk, int z)
        {
            int yh, yco, indexBlock;
            byte light;
            ChunkStorage chunkStorage;
            _indexBegin = _indexEnd = 0;
            int offset = z == 1 ? -15 : 16;
            for (int x = 0; x < 16; x++)
            {
                yh = chunk.Light.GetHeight(x, z);
                for (int y = 0; y < yh; y++)
                {
                    yco = y >> 4;
                    chunkStorage = chunk.StorageArrays[yco];
                    if (chunkStorage.CountBlock != 0)
                    {
                        indexBlock = (y & 15) << 8 | (z & 15) << 4 | (x & 15);
                        light = (byte)(chunkStorage.Light[indexBlock] >> 4);
                        if (light > 0)
                        {
                            _arCache[_indexEnd++] = x + 32 | y << 6 | z - offset + 32 << 16 | light << 22;
                        }
                    }
                }
            }
            _BrighterLightBlock();
        }
        /// <summary>
        /// Проверка небесного по X
        /// </summary>
        private void _CheckBrighterLightLoadedSkyX(ChunkBase chunk, int x)
        {
            int yco, indexBlock;
            byte light;
            ChunkStorage chunkStorage;
            _indexBegin = _indexEnd = 0;
            int offset = x == 1 ? -15 : 16;
            int numberBlocks = _world.ChunkPr.Settings.NumberBlocks;
            for (int z = 0; z < 16; z++)
            {
                for (int y = 0; y <= numberBlocks; y++)
                {
                    yco = y >> 4;
                    chunkStorage = chunk.StorageArrays[yco];
                    if (chunkStorage.CountBlock != 0)
                    {
                        indexBlock = (y & 15) << 8 | (z & 15) << 4 | (x & 15);
                        light = (byte)(chunkStorage.Light[indexBlock] & 15);
                        if (light > 0)
                        {
                            _arCache[_indexEnd++] = x - offset + 32 | y << 6 | z + 32 << 16 | light << 22;
                        }
                    }
                }
            }
            _BrighterLightSky();
        }
        /// <summary>
        /// Проверка небесного по Z
        /// </summary>
        private void _CheckBrighterLightLoadedSkyZ(ChunkBase chunk, int z)
        {
            int yco, indexBlock;
            byte light;
            ChunkStorage chunkStorage;
            _indexBegin = _indexEnd = 0;
            int offset = z == 1 ? -15 : 16;
            int numberBlocks = _world.ChunkPr.Settings.NumberBlocks;
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y <= numberBlocks; y++)
                {
                    yco = y >> 4;
                    chunkStorage = chunk.StorageArrays[yco];
                    if (chunkStorage.CountBlock != 0)
                    {
                        indexBlock = (y & 15) << 8 | (z & 15) << 4 | (x & 15);
                        light = (byte)(chunkStorage.Light[indexBlock] & 15);
                        if (light > 0)
                        {
                            _arCache[_indexEnd++] = x + 32 | y << 6 | z - offset + 32 << 16 | light << 22;
                        }
                    }
                }
            }
            _BrighterLightSky();
        }

        #endregion

        #region Fix

        /// <summary>
        /// Починить чанк блочного освещения по позиции чанка, вернуть ответ количество блоков с ошибкой
        /// Покуда только те, где остался светится, но не должен
        /// </summary>
        public int FixChunkLightBlock(int chX, int chY)
        {
            ChunkBase chunk = _world.GetChunk(chX, chY);
            if (chunk == null)
            {
                // Выбранный чанк отсутствует
                return -1;
            }
            ActionChunk(chunk);
            if (!UpChunks())
            {
                // Один из соседних чанков отсутствует
                return -1;
            }

            int x, y, z, xx, yy, zz, index;
            // яркость от блока
            byte lb, ys;
            // излучаемая яркость блока
            byte lo;
            // яркость от соседних блоков
            byte lightBeside;
            int count = 0;
            int xb = chX << 4;
            int zb = chY << 4;
            bool begin = true;
            int numberSections = _world.ChunkPr.Settings.NumberSections;
            _countBlock = 0;
            ChunkStorage chunkStorage;
            _indexBegin = _indexEnd = 0;
            List<long> list = new List<long>();
            for (ys = 0; ys < numberSections; ys++)
            {
                chunkStorage = chunk.StorageArrays[ys];
                for (y = 0; y < 16; y++)
                {
                    yy = (ys << 4) | y;
                    for (x = 0; x < 16; x++)
                    {
                        xx = xb | x;
                        for (z = 0; z < 16; z++)
                        {
                            index = y << 8 | z << 4 | x;
                            lb = (byte)(chunkStorage.Light[index] >> 4);
                            if (lb > 0) // у блока имеется яркость от блока
                            {
                                zz = zb | z;
                                // яркость от соседних блоков
                                lightBeside = _GetLevelBrightBlock(xx, yy, zz);
                                if (lb >= lightBeside) // яркость блока ярче соседних
                                {
                                    // проверяем блок на яркость, совпадает с lb или нет
                                    lo = (byte)(chunkStorage.IsEmptyData() ? 0
                                        : (Ce.Blocks.BlocksLightOpacity[chunkStorage.Data[index] & 0xFFF] & 0xF));
                                    if (lo != lb)
                                    {
                                        // затемняем
                                        count++;
                                        if (begin)
                                        {
                                            begin = false;
                                            _axisX0 = _axisX1 = xx;
                                            _axisY0 = _axisY1 = yy;
                                            _axisZ0 = _axisZ1 = zz;
                                            _indexBegin = _indexEnd = 0;
                                        }
                                        else
                                        {
                                            if (xx < _axisX0) _axisX0 = xx; else if (xx > _axisX1) _axisX1 = xx;
                                            if (yy < _axisY0) _axisY0 = yy; else if (yy > _axisY1) _axisY1 = yy;
                                            if (zz < _axisZ0) _axisZ0 = zz; else if (zz > _axisZ1) _axisZ1 = zz;
                                        }
                                        list.Add(ys | (lo << 8) | (index << 16));
                                        _arCache[_indexEnd++] = xx - _bOffsetX + 32 | yy << 6 | zz - _bOffsetZ + 32 << 16 | lb << 22;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (_indexEnd > 0)
            {
                long cache;
                byte l2;
                for (x = 0; x < list.Count; x++)
                {
                    cache = list[x];
                    y = (int)(cache & 255);
                    index = (int)(cache >> 16);
                    l2 = (byte)((cache >> 8) & 15);
                    chunk.StorageArrays[y].Light[index] = (byte)(l2 << 4 | chunk.StorageArrays[y].Light[index] & 15);
                }
                _DarkenLightBlock();
                _BrighterLightBlock();
                if (!_world.IsRemote && _world is WorldServer worldServer)
                {
                    worldServer.MarkBlockRangeForModified(_axisX0, _axisZ0, _axisX1, _axisZ1);
                    worldServer.ServerMarkChunkRangeForRenderUpdate(_axisX0 >> 4, _axisY0 >> 4, 
                        _axisZ0 >> 4, _axisX1 >> 4, _axisY1 >> 4, _axisZ1 >> 4);
                }
            }
            return count;
        }

        /// <summary>
        /// Возращает наивысший уровень яркости, глобальные координаты
        /// </summary>
        private byte _GetLevelBrightBlock(int x, int y, int z)
        {
            // обрабатываем соседние блоки, вдруг рядом плафон ярче, чтоб не затемнить
            // вектор стороны
            Vector3i vec;
            int iSide, y2;
            byte lightResult = 0;
            byte lightCache;
            int numberBlocks = _world.ChunkPr.Settings.NumberBlocks;

            for (iSide = 0; iSide < 6; iSide++)
            {
                vec = BlockPos.DirectionVectors[iSide];
                y2 = y + vec.Y;
                if (y2 >= 0 && y2 <= numberBlocks)
                {
                    lightCache = _GetLightBlock(x + vec.X, y2, z + vec.Z);
                    // Если соседний блок ярче текущего блока
                    if (lightCache > lightResult) lightResult = lightCache;
                    // Если блок яркий выводим значение
                    if (lightResult == 15) return 15;
                }
            }
            return lightResult;
        }

        #endregion
    }
}
