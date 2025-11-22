using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.World.Block;
using Vge.World.Chunk;

namespace Vge.World.Light
{
    /// <summary>
    /// Данные и обработки освещения для чанка
    /// </summary>
    public class ChunkLight
    {
        /// <summary>
        /// Объект обрабатываемого чанка
        /// </summary>
        public readonly ChunkBase Chunk;

        /// <summary>
        /// Карта высот по чанку, z << 4 | x
        /// </summary>
        public readonly ushort[] HeightMap = new ushort[256];
        /// <summary>
        /// Карта высот по чанку с учётом прозрачности, для стартового бокового освещения, z << 4 | x
        /// </summary>
        private readonly ushort[] HeightMapOpacity = new ushort[256];
        /// <summary>
        /// Высотная карта самый высокий блок в чанке от неба
        /// </summary>
        private int _heightMapMax = 0;
        /// <summary>
        /// Объект обработки освещения для мира
        /// </summary>
        private readonly WorldLight _worldLight;
        /// <summary>
        /// Массив блоков которые светятся
        /// Координаты в битах 0000 0000 0000 000y  yyyy yyyy zzzz xxxx
        /// </summary>
        private List<uint> _lightBlocks = new List<uint>();

        public ChunkLight(ChunkBase chunk)
        {
            Chunk = chunk;
            _worldLight = Chunk.World.Light;
        }

        /// <summary>
        /// Добавить блок с блочным освещением, xz 0..15, y 0..510
        /// </summary>
        public void SetLightBlock(int x, int y, int z) => _lightBlocks.Add((uint)(y << 8 | z << 4 | x));
        /// <summary>
        /// Добавить массив блок с блочным освещением, xz 0..15, y 0..510
        /// </summary>
        public void SetLightBlocks(uint[] lightBlocks) => _lightBlocks.AddRange(lightBlocks);

        /// <summary>
        /// Может ли видеть небо
        /// </summary>
        /// <param name="pos">глобальная координата мира</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAgainstSky(int x, int y, int z) => y >= HeightMap[(z & 15) << 4 | (x & 15)];

        #region HeightMap

        /// <summary>
        /// Возвращает значение карты высот в этой координате x, z в чанке. 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetHeight(int x, int z) => HeightMap[z << 4 | x];
        /// <summary>
        /// Копия высот
        /// </summary>
        //public byte[] CloneHeightMap()
        //{
        //    byte[] map = new byte[256];
        //    for (int i = 0; i < 256; i++) map[i] = HeightMap[i];
        //    return map;
        //}

        /// <summary>
        /// Карта высот по чанку с учётом прозрачности, для стартового бокового освещения, z << 4 | x
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetHeightOpacity(int x, int z) => HeightMapOpacity[z << 4 | x];

        /// <summary>
        /// Проверка высот
        /// </summary>
        public void CheckHeightMap(BlockPos blockPos, byte lightOpacity)
        {
            int x = blockPos.X & 15;
            int z = blockPos.Z & 15;
            int index = z << 4 | x;
            int yNew = blockPos.Y + 1;
            int yOld = HeightMap[index];

            if (yNew == _heightMapMax)
            {
                // обновляем максимальные высоты
                GenerateHeightMap();
            }
            else if (yNew >= yOld)
            {
                // обновляем только столб высот
                ChunkStorage chunkStorage;
                int y, y1;
                HeightMap[index] = 0;
                for (y = yNew; y > 0; y--)
                {
                    y1 = y - 1;
                    chunkStorage = Chunk.StorageArrays[y1 >> 4];
                    if (chunkStorage.CountBlock != 0 
                        && !Ce.Blocks.IsTransparent(chunkStorage.Data[(y1 & 15) << 8 | z << 4 | x]))
                    {
                        // первый блок препятствия сверху
                        HeightMap[index] = (ushort)y;
                        if (_heightMapMax < y) _heightMapMax = y;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Создает карту высот для блока с нуля
        /// </summary>
        public void GenerateHeightMap()
        {
            ChunkStorage chunkStorage;
            _heightMapMax = 0;
            int yb = Chunk.GetTopFilledSegment() + 17;
            if (yb > Chunk.Settings.NumberBlocks + 1) yb = Chunk.Settings.NumberBlocks + 1;
            int x, y, z, y1;
            for (x = 0; x < 16; x++)
            {
                for (z = 0; z < 16; z++)
                {
                    HeightMap[z << 4 | x] = 0;
                    for (y = yb; y > 0; y--)
                    {
                        y1 = y - 1;
                        chunkStorage = Chunk.StorageArrays[y1 >> 4];
                        if (chunkStorage.CountBlock != 0 
                            && !Ce.Blocks.IsTransparent(chunkStorage.Data[(y1 & 15) << 8 | z << 4 | x]))
                        {
                            // первый блок препятствия сверху
                            HeightMap[z << 4 | x] = (ushort)y;
                            if (_heightMapMax < y) _heightMapMax = y;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Создает карту высот для блока с нуля и тут же осветляет небесные блоки
        /// Тут ещё нет ни одного блочного освещения
        /// </summary>
        public void GenerateHeightMapSky()
        {
            _heightMapMax = 0;
            int x, y, z, y1, opacity, light, y2, index;
            int yb = Chunk.Settings.NumberBlocks;
            int y1c = Chunk.Settings.NumberSectionsLess;
            ChunkStorage chunkStorage = Chunk.StorageArrays[y1c];
            ChunkStorage chunkStorage2;
            for (x = 0; x < 16; x++)
            {
                for (z = 0; z < 16; z++)
                {
                    HeightMap[z << 4 | x] = 0;
                    HeightMapOpacity[z << 4 | x] = 0;
                    for (y = yb; y > 0; y--)
                    {
                        y1 = y - 1;
                        if (y1c != y1 >> 4)
                        {
                            y1c = y1 >> 4;
                            chunkStorage = Chunk.StorageArrays[y1c];
                        }
                        if (chunkStorage.CountBlock != 0)
                        {
                            if ((Ce.Blocks.IsTransparent(chunkStorage.Data[(y1 & 15) << 8 | z << 4 | x])))
                            {
                                // Небо, осветляем
                                Chunk.StorageArrays[y >> 4].Light[(y & 15) << 8 | z << 4 | x] = 0xF;
                            }
                            else
                            {
                                // первый блок препятствия сверху
                                HeightMap[z << 4 | x] = (ushort)y;
                                if (_heightMapMax < y) _heightMapMax = y;

                                light = 15;
                                y2 = y;
                                Chunk.StorageArrays[y2 >> 4].Light[(y2 & 15) << 8 | z << 4 | x] = 0xF;
                                y2--;
                                // Запускаем цикл затемнения блоков
                                while (light > 0 && y2 > 0)
                                {
                                    chunkStorage2 = Chunk.StorageArrays[y2 >> 4];
                                    index = (y2 & 15) << 8 | z << 4 | x;
                                    if (chunkStorage2.CountBlock != 0)
                                    {
                                        opacity = Ce.Blocks.GetOpacity(chunkStorage2.Data[index]);
                                    }
                                    else opacity = 0;
                                    light = light - opacity - 1;
                                    if (light < 0) light = 0;
                                    chunkStorage2.Light[index] = (byte)light;
                                    y2--;
                                }
                                HeightMapOpacity[z << 4 | x] = (ushort)(y2 + 2);
                                break;
                            }
                        }
                        else
                        {
                            // Небо, осветляем
                            Chunk.StorageArrays[y >> 4].Light[(y & 15) << 8 | z << 4 | x] = 0xF;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Запуск проверки бокового небесного освещения
        /// </summary>
        /// <param name="hasNoSky">Неба нет</param>
        public void StartRecheckGaps(bool hasNoSky)
        {
            _worldLight.ActionChunk(Chunk);
            if (!_worldLight.UpChunks()) return;

            if (!hasNoSky)
            {
                // Небесное боковое освещение
                _worldLight.HeavenSideLighting();
            }

            if (!Chunk.IsLoaded)
            {
                // Для проверки не корректного освещения
                _worldLight.CheckBrighterLightLoaded();
            }

            if (_lightBlocks.Count > 0)
            {
                _worldLight.CheckBrighterLightBlocks(_lightBlocks);
                _lightBlocks.Clear();
            }
        }

        #endregion

        /// <summary>
        /// Проверить небесный свет
        /// </summary>
        public void CheckLightSky(int x, int y, int z, byte lo)
        {
            int yh = GetHeight(x & 15, z & 15);
            if (lo >> 4 > 0) // Не небо
            {
                // закрываем небо
                if (y >= yh)
                {
                    // Проверка столба неба
                    _CheckLightColumnSky(x, y + 1, z, yh);
                }
                else
                {
                    // Проверка блока небесного освещения
                    _worldLight.CheckLightSky(x, y, z, lo);
                }
            }
            else if (y == yh - 1 || y == Chunk.Settings.NumberBlocks)
            {
                // Открываем небо, проверка столба неба
                _CheckLightColumnSky(x, y, z, yh);
            }
            else
            {
                // Проверка блока небесного освещения
                _worldLight.CheckLightSky(x, y, z, lo);
            }
        }

        /// <summary>
        /// Проверить небесный столб света
        /// </summary>
        private void _CheckLightColumnSky(int x, int y0, int z, int yh)
        {
            int yh0 = yh;
            // Определяем нижний блок
            if (y0 > yh) yh0 = y0;

            ChunkStorage chunkStorage;
            int xb = x & 15;
            int zb = z & 15;
            int opacity = 0;
            int yh1;
            int numberBlocks = Chunk.Settings.NumberBlocks;

            while (yh0 > 0 && opacity == 0)
            {
                yh1 = yh0 - 1;
                if (yh1 < 0 || yh1 > numberBlocks)
                {
                    opacity = 0;
                }
                else
                {
                    chunkStorage = Chunk.StorageArrays[yh1 >> 4];
                    if (chunkStorage.CountBlock != 0)
                    {
                        opacity = Ce.Blocks.GetOpacity(chunkStorage.Data[(yh1 & 15) << 8 | zb << 4 | xb]);
                    }
                    else
                    {
                        opacity = 0;
                    }
                }
                if (opacity == 0) yh0--;
            }

            // Если блок равен высотной игнорируем
            if (yh == y0) return;

            int yh2 = yh0;
            if (yh2 > numberBlocks + 1) yh2 = numberBlocks + 1;
            HeightMap[zb << 4 | xb] = (ushort)yh2;
            if (_heightMapMax < yh2) _heightMapMax = yh2;

            if (yh < yh0)
            {
                // закрыли небо, надо затемнять
                int yDown = yh;// + 1;
                int yUp = yh0;// - 1;
                _worldLight.DarkenLightColumnSky(x, yDown, z, yUp);
                //  for (int y = yDown; y < yUp; y++) World.SetBlockDebug(new BlockPos(x, y, z), EnumBlock.Glass);
            }
            else
            {
                // открыли небо, надо осветлять
                int yDown = yh0;
                int yUp = yh;
                // пометка что убераем вверхний блок
                bool hMax = yh == _heightMapMax;
                _worldLight.BrighterLightColumnSky(x, yDown, z, yUp);
                // for (int y = yDown; y < yUp; y++) World.SetBlockDebug(new BlockPos(x, y, z), EnumBlock.Glass);
                // обновляем максимальные высоты
                if (hMax) GenerateHeightMap();
            }
        }
    }
}
