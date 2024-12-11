using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World.Block;
using Vge.World.Chunk;
using WinGL.Util;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Объект рендера блока
    /// </summary>
    public class BlockRenderFull
    {
        public readonly static Vector3 ColorWhite = new Vector3(1);

        /// <summary>
        /// Построение стороны блока
        /// </summary>
        public BlockSide blockUV = new BlockSide();

        /// <summary>
        /// Объект блока кэш
        /// </summary>
        public BlockBase Block;
        /// <summary>
        /// Освещение блочное и небесное
        /// </summary>
        public int Light;
        /// <summary>
        /// Метданные текущего блока
        /// </summary>
        public uint Met;

        /// <summary>
        /// Позиция блока в чанке 0..15
        /// </summary>
        public int PosChunkX;
        /// <summary>
        /// Позиция блока в чанке 0..255
        /// </summary>
        public int PosChunkY;
        /// <summary>
        /// Позиция блока в чанке 0..15
        /// </summary>
        public int PosChunkZ;

        /// <summary>
        /// Ключ кэш координат чанка (ulong)(uint)x  32) | ((uint)y
        /// </summary>
        protected ulong _keyCash;
        /// <summary>
        /// Тень на углах + ~0.200 мс к рендеру на чанк
        /// </summary>
        protected bool _ambientOcclusion = true;
        /// <summary>
        /// Индекс стороны для массива, который храним наличие и свет
        /// </summary>
        private int _indexSide = -1;
        /// <summary>
        /// Массив сторон, который храним наличие и свет
        /// </summary>
        protected readonly int[] _resultSide = new int[] { -1, -1, -1, -1, -1, -1 };
        /// <summary>
        /// Объект рендера чанков
        /// </summary>
        protected ChunkRender _chunkRender;
        /// <summary>
        /// Объект блока для проверки
        /// </summary>
        protected BlockBase _blockCheck;
        protected int _metCheck;
        protected ChunkRender _chunkCheck;
        /// <summary>
        /// Высота блоков в чанке, 127
        /// </summary>
        protected int _numberBlocksY;

        protected ChunkStorage _storage;
        private bool _isDraw;
        protected int _stateLight, _stateLightHis;
        protected byte _stateLightByte;
        private QuadSide[] _rectangularSides;
        private QuadSide _rectangularSide;
        private readonly AmbientOcclusionLights _ambient = new AmbientOcclusionLights();
        protected float _lightPole;
        private Vector3 _color;
        protected readonly ColorsLights _colorLight = new ColorsLights();
        /// <summary>
        /// Отбраковка всех сторон во всех вариантах
        /// </summary>
        private bool _isCullFaceAll;
        /// <summary>
        /// Принудительное рисование всех сторон
        /// </summary>
        private bool _isForceDrawFace;
        protected int i, count, index, id, s1, s2, s3, s4, side;
        protected int xc, yc, zc, xb, yb, zb, xcn, zcn, pX, pY, pZ;

        /// <summary>
        /// Проверяем вверхний блок, для жидкости
        /// _GetBlockSideState
        /// </summary>
        private bool _isUp;
        /// <summary>
        /// Пустые ли стороны
        /// _GetBlockSideState
        /// </summary>
        private bool _emptySide;


        /// <summary>
        /// Создание блока генерации для мира
        /// </summary>
        public BlockRenderFull(VertexBuffer vertex)
        {
            blockUV.Buffer = vertex;
            InitAmbientOcclusion();
        }

        #region Init

        /// <summary>
        /// Инициализация чанка
        /// </summary>
        public void InitChunk(ChunkRender chunkRender)
        {
            _chunkRender = chunkRender;
            _keyCash = chunkRender.KeyCash;
            _blockCheck = Ce.Blocks.BlockObjects[0];
            _storage = _chunkRender.StorageArrays[0];
            _numberBlocksY = _chunkRender.Settings.NumberBlocks;
        }

        /// <summary>
        /// Инициализация сектора данных чанка
        /// </summary>
        public void InitStorage(int y) => _storage = _chunkRender.StorageArrays[y];
        /// <summary>
        /// Задать Псевдо чанк с данными вокселей
        /// </summary>
        public void InitStorage(ChunkStorage chunkStorage)
        {
            if (chunkStorage.KeyCash != _storage.KeyCash)
            {
                _storage = chunkStorage;
            }
        }

        /// <summary>
        /// Инициализация AmbientOcclusion
        /// </summary>
        public void InitAmbientOcclusion() => _ambientOcclusion = Options.Qualitatively;

        #endregion

        /// <summary>
        /// Проверяем видны ли стороны, если хоть одна видна вернём true
        /// </summary>
        public bool CheckSide()
        {
            _isForceDrawFace = Gi.Block.ForceDrawFace;
            _isCullFaceAll = Gi.Block.CullFaceAll;

            // ~0.12
            _emptySide = true;
            // Up
            PosChunkY++;
            if (PosChunkY > _numberBlocksY)
            {
                _resultSide[0] = 0x0F;
                _emptySide = false;
            }
            else
            {
                _isUp = true;
                if (_storage.Index != PosChunkY >> 4 || _storage.KeyCash != _keyCash)
                {
                    _storage = _chunkRender.StorageArrays[PosChunkY >> 4];
                }
                _indexSide = 0;
                _GetBlockSideState();
                _isUp = false;
            }
            // Down
            PosChunkY -= 2;
            if (PosChunkY < 0)
            {
                _resultSide[1] = 0x0F;
                _emptySide = false;
            }
            else
            {
                if (_storage.Index != PosChunkY >> 4 || _storage.KeyCash != _keyCash)
                {
                    _storage = _chunkRender.StorageArrays[PosChunkY >> 4];
                }
                _indexSide = 1;
                _GetBlockSideState();
            }
            PosChunkY++;
            yc = PosChunkY >> 4;

            // East
            PosChunkX++;

            _indexSide = 2;
            _GetBlockSideStateCheck();

            // West
            PosChunkX -= 2;

            _indexSide = 3;
            _GetBlockSideStateCheck();

            PosChunkX++;
            // North
            PosChunkZ--;

            _indexSide = 4;
            _GetBlockSideStateCheck();

            // South
            PosChunkZ += 2;

            _indexSide = 5;
            _GetBlockSideStateCheck();

            if (!_emptySide)
            {
                PosChunkZ--;
                return true;
            }
            return false;
            // Когда _GetBlockSideState сразу -1  ~0.33-0.4
        }

        /// <summary>
        /// Рендерим стороны, после метода CheckSide
        /// </summary>
        public virtual void RenderSide()
        {
            if (Gi.Block.UseNeighborBrightness) _UseNeighborBrightness();
            else _stateLightHis = -1;
            // ~1.100-1.150

            _rectangularSides = Gi.Block.GetQuads(Met, PosChunkX, PosChunkZ);
            count = _rectangularSides.Length;

            for (i = 0; i < count; i++)
            {
                _rectangularSide = _rectangularSides[i];
                side = _rectangularSide.Side;
                _stateLight = _resultSide[side];
                if (_stateLight != -1)
                {
                    // Доп проверка стороны на её не прорисовку
                    if (_stateLight > 255 && !_rectangularSide.NotExtremeSide)
                    {
                        continue;
                    }

                    // Смекшировать яркость, в зависимости от требований самой яркой
                    if (_stateLight != _stateLightHis
                        && Gi.Block.UseNeighborBrightness && _stateLightHis != -1
                        && !Gi.Block.IsCullFace(Met, side)) // Отсеять сторону которая целиком закрыла сторону
                    {
                        _stateLight = _stateLightHis;
                    }

                    _lightPole = _rectangularSide.LightPole;
                    _stateLightByte = (byte)_stateLight;
                    _GenColors();

                    blockUV.ColorsR = _colorLight.ColorR;
                    blockUV.ColorsG = _colorLight.ColorG;
                    blockUV.ColorsB = _colorLight.ColorB;
                    blockUV.Lights = _colorLight.Light;
                    blockUV.PosCenterX = PosChunkX;
                    blockUV.PosCenterY = PosChunkY;
                    blockUV.PosCenterZ = PosChunkZ;
                    blockUV.AnimationFrame = _rectangularSide.AnimationFrame;
                    blockUV.AnimationPause = _rectangularSide.AnimationPause;
                    blockUV.Sharpness = _rectangularSide.Sharpness;
                    blockUV.Vertex = _rectangularSide.Vertex;
                    if (_rectangularSide.Wind == 0)
                    {
                        blockUV.Building();
                    }
                    else
                    {
                        blockUV.BuildingWind(_rectangularSide.Wind);
                    }


                    //if (damagedBlocksValue != -1)
                    //{
                    //    int i1 = i + 1;
                    //    if ((i1 < count && rectangularSides[i1].side != side) || i1 == count)
                    //    {
                    //        // Разрушение блока
                    //        blockUV.colorsr = colorsFF;
                    //        blockUV.colorsg = colorsFF;
                    //        blockUV.colorsb = colorsFF;
                    //        blockUV.BuildingDamaged((block.IsDamagedBlockBlack ? 3840 : 3968) + damagedBlocksValue * 2);
                    //    }
                    //}
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _GetBlockSideStateCheck()
        {
            // Определяем рабочий чанк соседнего блока
            if (PosChunkX >> 4 == 0 && PosChunkZ >> 4 == 0)
            {
                // Текущий чанк
                if (_storage.KeyCash != _keyCash || _storage.Index != yc)
                {
                    // Прошли проверку, что прошлый раз в кеше другой, надо заменить
                    _storage = _chunkRender.StorageArrays[yc];
                }
            }
            else
            {
                // Соседний чанк
                _chunkCheck = _chunkRender.Chunk(PosChunkX >> 4, PosChunkZ >> 4);
                if (_chunkCheck == null || !_chunkCheck.IsChunkPresent)
                {
                    _emptySide = false;
                    _resultSide[_indexSide] = 0x0F; // Только яркость неба макс
                    return;
                }
                // Сразу присваеваю без проверок, так-как если соседний чанк, то в кеше маловероятно, что он же
                _storage = _chunkCheck.StorageArrays[yc];
            }

            _GetBlockSideState();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _GetBlockSideState()
        {
            // На старте ~0.580
            i = (PosChunkY & 15) << 8 | (PosChunkZ & 15) << 4 | (PosChunkX & 15);

            if (_isForceDrawFace)
            {
                // Принудительное рисование всех сторон, модель которые все стороны не касаются краёв
                _emptySide = false;
                _resultSide[_indexSide] = _storage.Light[i];
            }
            else if (_storage.CountBlock > 0)
            {
                id = _storage.Data[i];
                if (id == 0)
                {
                    // Воздух
                    _emptySide = false;
                    _resultSide[_indexSide] = _storage.Light[i];
                }
                else if (_isCullFaceAll && id == Gi.Block.Id)
                {
                    // Одинаково типа, убираем прорисовку, вода, стекло без метданых!
                    // Чистый должен быть id, для воды
                    _resultSide[_indexSide] = -1;
                }
                else
                {
                    // Собираем данные соседнего блока
                    _metCheck = id >> 12;
                    id = id & 0xFFF;
                    if (_blockCheck.Id != id)
                    {
                        _blockCheck = Ce.Blocks.BlockObjects[id];
                    }

                    if (_isCullFaceAll && _blockCheck.CullFaceAll && !_blockCheck.Translucent)// || !_blockCheck.Liquid))// && (!_isUp || Gi.Block.Liquid))
                    {
                        // Блоки целые соседний непрозрачный
                        if (_isUp && Gi.Block.Liquid)
                        {
                            // Над водой блок
                            _emptySide = false;
                            _resultSide[_indexSide] = _storage.Light[i];
                        }
                        else
                        {
                            _resultSide[_indexSide] = -1;
                        }
                    }
                    else if (_blockCheck.Translucent)
                    {
                        // Соседний блок прозрачный
                        if (Gi.Block.Id != id)
                        {
                            // Блоки разного типа, то палюбому надо рисовать сторону
                            _emptySide = false;
                            _resultSide[_indexSide] = _storage.Light[i]
                                | (Gi.Block.LiquidOutside - _blockCheck.NotLiquidOutside[_indexSide]);
                        }
                        else
                        {
                            // Одинаково типа, убираем прорисовку, вода, стекло
                            _resultSide[_indexSide] = -1;
                        }
                    }
                    else if (Gi.Block.IsForceDrawFace(Met, _indexSide)
                        || !Gi.Block.ChekMaskCullFace(_indexSide, Met, _blockCheck, _blockCheck.IsMetadata
                                ? _storage.Metadata[(ushort)index] : (uint)_metCheck))
                    {
                        // Принудительное рисование стороны, модель которая сторона не касаются краёв
                        _emptySide = false;
                        if (_blockCheck.IsCullFace(_blockCheck.IsMetadata
                                ? _storage.Metadata[(ushort)index] : (uint)_metCheck, PoleConvert.Reverse[_indexSide]))
                        {
                            _resultSide[_indexSide] = _GetSideUseNeighborBrightness(PosChunkX, PosChunkY, PosChunkZ, _storage.Light[i])
                                | (Gi.Block.LiquidOutside - _blockCheck.NotLiquidOutside[_indexSide]);
                        }
                        else
                        {
                            _resultSide[_indexSide] = _GetSideUseNeighborBrightness(PosChunkX, PosChunkY, PosChunkZ, _storage.Light[i]);
                        }
                    }
                    else if (Gi.Block.IsForceDrawNotExtremeFace(Met, _indexSide))
                    {
                        _emptySide = false;
                        _resultSide[_indexSide] = _GetSideUseNeighborBrightness(PosChunkX, PosChunkY, PosChunkZ, _storage.Light[i]);
                    }
                    else
                    {
                        // Это не видно по маске
                        _resultSide[_indexSide] = -1;
                    }
                }
            }
            else
            {
                // Воздух нет сектора
                _emptySide = false;
                _resultSide[_indexSide] = _storage.Light[i];
            }
        }

        /// <summary>
        /// Сгенерировать цвета на каждый угол, если надо то AmbientOcclusion
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void _GenColors()
        {
            _color = _GetBiomeColor(_chunkRender, PosChunkX, PosChunkZ);
            if (_ambientOcclusion && (Gi.Block.АmbientOcclusion || Gi.Block.BiomeColor || Gi.Block.Liquid))
            {
                _GetAmbientOcclusionLights();
                _lightPole *= .5f;
                if (Gi.Block.Liquid)
                {
                    _colorLight.InitColorsLights(
                        _ambient.GetColorNotAO(0, _color, _lightPole), _ambient.GetColorNotAO(1, _color, _lightPole),
                        _ambient.GetColorNotAO(2, _color, _lightPole), _ambient.GetColorNotAO(3, _color, _lightPole),
                        _ambient.GetLight(0, _stateLightByte), _ambient.GetLight(1, _stateLightByte),
                        _ambient.GetLight(2, _stateLightByte), _ambient.GetLight(3, _stateLightByte)
                    );
                }
                else
                {
                    _colorLight.InitColorsLights(
                        _ambient.GetColor(0, _color, _lightPole), _ambient.GetColor(1, _color, _lightPole),
                        _ambient.GetColor(2, _color, _lightPole), _ambient.GetColor(3, _color, _lightPole),
                        _ambient.GetLight(0, _stateLightByte), _ambient.GetLight(1, _stateLightByte),
                        _ambient.GetLight(2, _stateLightByte), _ambient.GetLight(3, _stateLightByte));
                }
            }
            else
            {
                _color.X -= _lightPole; if (_color.X < 0) _color.X = 0;
                _color.Y -= _lightPole; if (_color.Y < 0) _color.Y = 0;
                _color.Z -= _lightPole; if (_color.Z < 0) _color.Z = 0;

                _colorLight.ColorR[0] = _colorLight.ColorR[1] = _colorLight.ColorR[2] = _colorLight.ColorR[3] = (byte)(_color.X * 255);
                _colorLight.ColorG[0] = _colorLight.ColorG[1] = _colorLight.ColorG[2] = _colorLight.ColorG[3] = (byte)(_color.Y * 255);
                _colorLight.ColorB[0] = _colorLight.ColorB[1] = _colorLight.ColorB[2] = _colorLight.ColorB[3] = (byte)(_color.Z * 255);
                _colorLight.Light[0] = _colorLight.Light[1] = _colorLight.Light[2] = _colorLight.Light[3] = _stateLightByte;
            }
        }

        /// <summary>
        /// Получить цвет в зависимости от биома, цвет определяем потипу
        /// </summary>
        /// <param name="bx">0-15</param>
        /// <param name="bz">0-15</param>
        protected virtual Vector3 _GetBiomeColor(ChunkRender chunk, int bx, int bz)
        {
            // подготовка для теста плавности цвета
            // Нет цвета
            if (_rectangularSide.TypeColor == 0) return ColorWhite;
            // Свой цвет
            if (_rectangularSide.TypeColor == 4) return Gi.Block.Color;
            // Цвет от биома
            return chunk.GetColorSideFromBiom(_rectangularSide.TypeColor, bx, bz);
        }

        /// <summary>
        /// Получаем в _stateLightHis самое яркое значение соседнего света как свое собственное
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _UseNeighborBrightness()
        {
            _stateLightHis = Light;
            // Микс
            for (i = 0; i < 6; i++)
            {
                _stateLight = _resultSide[i];
                // Смекшировать яркость, в зависимости от требований самой яркой
                if (_stateLight != -1 && _stateLight != _stateLightHis)
                {
                    s1 = _stateLightHis >> 4;
                    s2 = _stateLightHis & 0x0F;
                    s3 = _stateLight >> 4;
                    s4 = _stateLight & 0x0F;
                    _stateLightHis = (s1 > s3 ? s1 : s3) << 4 | (s2 > s4 ? s2 : s4) & 0xF;
                }
            }
        }

        /// <summary>
        /// Получаем самое яркое значение соседнего света как свое собственное конкретного блока
        /// </summary>
        private int _GetSideUseNeighborBrightness(int x, int y, int z, int light)
        {
            Vector3i vec;
            int x0, y0, z0;
            ChunkStorage chunkStorage;
            int lightSide;
            for (int iSide = 0; iSide < 6; iSide++) // Цикл сторон
            {
                vec = BlockPos.DirectionVectors[iSide];
                x0 = x + vec.X;
                y0 = y + vec.Y;
                z0 = z + vec.Z;
                chunkStorage = _GetChunkStorage(x0, y0, z0);
                if (chunkStorage != null)
                {
                    lightSide = chunkStorage.Light[(y0 & 15) << 8 | (z0 & 15) << 4 | (x0 & 15)];
                    if (light == -1) light = lightSide;
                    else if (lightSide != light)
                    {
                        s1 = light >> 4;
                        s2 = light & 0x0F;
                        s3 = lightSide >> 4;
                        s4 = lightSide & 0x0F;
                        light = (s1 > s3 ? s1 : s3) << 4 | (s2 > s4 ? s2 : s4) & 0xF;
                    }
                }
            }
            return light;
        }

        /// <summary>
        /// Получить чанк данных по локальным координатам -1..16
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ChunkStorage _GetChunkStorage(int x, int y, int z)
        {
            if (x >> 4 == 0 && z >> 4 == 0)
            {
                return _chunkRender.StorageArrays[y >> 4];
            }
            ChunkRender chunkRender = _chunkRender.Chunk(x >> 4, z >> 4);
            if (chunkRender != null && chunkRender.IsChunkPresent)
            {
                return chunkRender.StorageArrays[y >> 4];
            }
            return null;
        }

        #region AmbientOcclusion

        /// <summary>
        /// Получить все 4 вершины AmbientOcclusion и яркости от блока и неба
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _GetAmbientOcclusionLights()
        {
            if (side == 0)
            {
                _ambient.Aos[0] = _GetAmbientOcclusionLight(1, 1, 0);
                _ambient.Aos[1] = _GetAmbientOcclusionLight(0, 1, 1);
                _ambient.Aos[2] = _GetAmbientOcclusionLight(-1, 1, 0);
                _ambient.Aos[3] = _GetAmbientOcclusionLight(0, 1, -1);
                _ambient.Aos[4] = _GetAmbientOcclusionLight(-1, 1, -1);
                _ambient.Aos[5] = _GetAmbientOcclusionLight(-1, 1, 1);
                _ambient.Aos[6] = _GetAmbientOcclusionLight(1, 1, 1);
                _ambient.Aos[7] = _GetAmbientOcclusionLight(1, 1, -1);
            }
            else if (side == 1)
            {
                _ambient.Aos[2] = _GetAmbientOcclusionLight(1, -1, 0);
                _ambient.Aos[1] = _GetAmbientOcclusionLight(0, -1, 1);
                _ambient.Aos[0] = _GetAmbientOcclusionLight(-1, -1, 0);
                _ambient.Aos[3] = _GetAmbientOcclusionLight(0, -1, -1);
                _ambient.Aos[7] = _GetAmbientOcclusionLight(-1, -1, -1);
                _ambient.Aos[6] = _GetAmbientOcclusionLight(-1, -1, 1);
                _ambient.Aos[5] = _GetAmbientOcclusionLight(1, -1, 1);
                _ambient.Aos[4] = _GetAmbientOcclusionLight(1, -1, -1);
            }
            else if (side == 2)
            {
                _ambient.Aos[1] = _GetAmbientOcclusionLight(1, 1, 0);
                _ambient.Aos[0] = _GetAmbientOcclusionLight(1, 0, 1);
                _ambient.Aos[3] = _GetAmbientOcclusionLight(1, -1, 0);
                _ambient.Aos[2] = _GetAmbientOcclusionLight(1, 0, -1);
                _ambient.Aos[4] = _GetAmbientOcclusionLight(1, -1, -1);
                _ambient.Aos[7] = _GetAmbientOcclusionLight(1, -1, 1);
                _ambient.Aos[6] = _GetAmbientOcclusionLight(1, 1, 1);
                _ambient.Aos[5] = _GetAmbientOcclusionLight(1, 1, -1);
            }
            else if (side == 3)
            {
                _ambient.Aos[1] = _GetAmbientOcclusionLight(-1, 1, 0);
                _ambient.Aos[2] = _GetAmbientOcclusionLight(-1, 0, 1);
                _ambient.Aos[3] = _GetAmbientOcclusionLight(-1, -1, 0);
                _ambient.Aos[0] = _GetAmbientOcclusionLight(-1, 0, -1);
                _ambient.Aos[7] = _GetAmbientOcclusionLight(-1, -1, -1);
                _ambient.Aos[4] = _GetAmbientOcclusionLight(-1, -1, 1);
                _ambient.Aos[5] = _GetAmbientOcclusionLight(-1, 1, 1);
                _ambient.Aos[6] = _GetAmbientOcclusionLight(-1, 1, -1);
            }
            else if (side == 4)
            {
                _ambient.Aos[1] = _GetAmbientOcclusionLight(0, 1, -1);
                _ambient.Aos[0] = _GetAmbientOcclusionLight(1, 0, -1);
                _ambient.Aos[3] = _GetAmbientOcclusionLight(0, -1, -1);
                _ambient.Aos[2] = _GetAmbientOcclusionLight(-1, 0, -1);
                _ambient.Aos[4] = _GetAmbientOcclusionLight(-1, -1, -1);
                _ambient.Aos[7] = _GetAmbientOcclusionLight(1, -1, -1);
                _ambient.Aos[6] = _GetAmbientOcclusionLight(1, 1, -1);
                _ambient.Aos[5] = _GetAmbientOcclusionLight(-1, 1, -1);
            }
            else// if ( side == 5)
            {
                _ambient.Aos[1] = _GetAmbientOcclusionLight(0, 1, 1);
                _ambient.Aos[2] = _GetAmbientOcclusionLight(1, 0, 1);
                _ambient.Aos[3] = _GetAmbientOcclusionLight(0, -1, 1);
                _ambient.Aos[0] = _GetAmbientOcclusionLight(-1, 0, 1);
                _ambient.Aos[7] = _GetAmbientOcclusionLight(-1, -1, 1);
                _ambient.Aos[4] = _GetAmbientOcclusionLight(1, -1, 1);
                _ambient.Aos[5] = _GetAmbientOcclusionLight(1, 1, 1);
                _ambient.Aos[6] = _GetAmbientOcclusionLight(-1, 1, 1);
            }

            //switch (side)
            //{
            //    case 0:
            //        _ambient.Aos[0] = _GetAmbientOcclusionLight(1, 1, 0);
            //        _ambient.Aos[1] = _GetAmbientOcclusionLight(0, 1, 1);
            //        _ambient.Aos[2] = _GetAmbientOcclusionLight(-1, 1, 0);
            //        _ambient.Aos[3] = _GetAmbientOcclusionLight(0, 1, -1);
            //        _ambient.Aos[4] = _GetAmbientOcclusionLight(-1, 1, -1);
            //        _ambient.Aos[5] = _GetAmbientOcclusionLight(-1, 1, 1);
            //        _ambient.Aos[6] = _GetAmbientOcclusionLight(1, 1, 1);
            //        _ambient.Aos[7] = _GetAmbientOcclusionLight(1, 1, -1);
            //        break;
            //    case 1:
            //        _ambient.Aos[2] = _GetAmbientOcclusionLight(1, -1, 0);
            //        _ambient.Aos[1] = _GetAmbientOcclusionLight(0, -1, 1);
            //        _ambient.Aos[0] = _GetAmbientOcclusionLight(-1, -1, 0);
            //        _ambient.Aos[3] = _GetAmbientOcclusionLight(0, -1, -1);
            //        _ambient.Aos[7] = _GetAmbientOcclusionLight(-1, -1, -1);
            //        _ambient.Aos[6] = _GetAmbientOcclusionLight(-1, -1, 1);
            //        _ambient.Aos[5] = _GetAmbientOcclusionLight(1, -1, 1);
            //        _ambient.Aos[4] = _GetAmbientOcclusionLight(1, -1, -1);
            //        break;
            //    case 2:
            //        _ambient.Aos[1] = _GetAmbientOcclusionLight(1, 1, 0);
            //        _ambient.Aos[0] = _GetAmbientOcclusionLight(1, 0, 1);
            //        _ambient.Aos[3] = _GetAmbientOcclusionLight(1, -1, 0);
            //        _ambient.Aos[2] = _GetAmbientOcclusionLight(1, 0, -1);
            //        _ambient.Aos[4] = _GetAmbientOcclusionLight(1, -1, -1);
            //        _ambient.Aos[7] = _GetAmbientOcclusionLight(1, -1, 1);
            //        _ambient.Aos[6] = _GetAmbientOcclusionLight(1, 1, 1);
            //        _ambient.Aos[5] = _GetAmbientOcclusionLight(1, 1, -1);
            //        break;
            //    case 3:
            //        _ambient.Aos[1] = _GetAmbientOcclusionLight(-1, 1, 0);
            //        _ambient.Aos[2] = _GetAmbientOcclusionLight(-1, 0, 1);
            //        _ambient.Aos[3] = _GetAmbientOcclusionLight(-1, -1, 0);
            //        _ambient.Aos[0] = _GetAmbientOcclusionLight(-1, 0, -1);
            //        _ambient.Aos[7] = _GetAmbientOcclusionLight(-1, -1, -1);
            //        _ambient.Aos[4] = _GetAmbientOcclusionLight(-1, -1, 1);
            //        _ambient.Aos[5] = _GetAmbientOcclusionLight(-1, 1, 1);
            //        _ambient.Aos[6] = _GetAmbientOcclusionLight(-1, 1, -1);
            //        break;
            //    case 4:
            //        _ambient.Aos[1] = _GetAmbientOcclusionLight(0, 1, -1);
            //        _ambient.Aos[0] = _GetAmbientOcclusionLight(1, 0, -1);
            //        _ambient.Aos[3] = _GetAmbientOcclusionLight(0, -1, -1);
            //        _ambient.Aos[2] = _GetAmbientOcclusionLight(-1, 0, -1);
            //        _ambient.Aos[4] = _GetAmbientOcclusionLight(-1, -1, -1);
            //        _ambient.Aos[7] = _GetAmbientOcclusionLight(1, -1, -1);
            //        _ambient.Aos[6] = _GetAmbientOcclusionLight(1, 1, -1);
            //        _ambient.Aos[5] = _GetAmbientOcclusionLight(-1, 1, -1);
            //        break;
            //    case 5:
            //        _ambient.Aos[1] = _GetAmbientOcclusionLight(0, 1, 1);
            //        _ambient.Aos[2] = _GetAmbientOcclusionLight(1, 0, 1);
            //        _ambient.Aos[3] = _GetAmbientOcclusionLight(0, -1, 1);
            //        _ambient.Aos[0] = _GetAmbientOcclusionLight(-1, 0, 1);
            //        _ambient.Aos[7] = _GetAmbientOcclusionLight(-1, -1, 1);
            //        _ambient.Aos[4] = _GetAmbientOcclusionLight(1, -1, 1);
            //        _ambient.Aos[5] = _GetAmbientOcclusionLight(1, 1, 1);
            //        _ambient.Aos[6] = _GetAmbientOcclusionLight(-1, 1, 1);
            //        break;
            //}

            _ambient.InitAmbientOcclusionLights();
        }

        /// <summary>
        /// Подготовить кэш blockSideCache, прорисовывается ли сторона и её яркость
        /// Получть данные (AmbientOcclusion и яркость) одно стороны для вершины
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AmbientOcclusionLight _GetAmbientOcclusionLight(int x, int y, int z)
        {
            pY = PosChunkY + y;
            // проверка высоты
            if (pY < 0)
            {
                return new AmbientOcclusionLight() { Color = ColorWhite };
            }

            pX = PosChunkX + x;
            pZ = PosChunkZ + z;

            xcn = pX >> 4;
            zcn = pZ >> 4;
            xc = PosChunkX + xcn;
            zc = PosChunkY + zcn;

            // Определяем рабочий чанк соседнего блока
            _chunkCheck = (xc == PosChunkX && zc == PosChunkY) ? _chunkRender : _chunkRender.Chunk(xcn, zcn);
            if (_chunkCheck == null || !_chunkCheck.IsChunkPresent)
            {
                return new AmbientOcclusionLight()
                {
                    LightSky = 15,
                    Color = ColorWhite,
                    Aol = 1
                };
            }

            xb = pX & 15;
            zb = pZ & 15;
            AmbientOcclusionLight aoLight = new AmbientOcclusionLight()
            {
                Color = _GetBiomeColor(_chunkCheck, xb, zb)
            };
            if (pY >= _numberBlocksY)
            {
                aoLight.LightSky = 15;
                aoLight.Aol = 1;
                return aoLight;
            }
            yc = pY >> 4;
            _storage = _chunkCheck.StorageArrays[yc];

            yb = pY & 15;
            index = yb << 8 | zb << 4 | xb;

            if (_storage.CountBlock == 0)
            {
                // Только яркость неба
                aoLight.LightSky = (byte)(_storage.Light[index] & 15);
                aoLight.LightBlock = (byte)(_storage.Light[index] >> 4);
                aoLight.Aol = 1;
                return aoLight;
            }

            id = _storage.Data[index];
            if (id == 0)
            {
                // Воздух берём яркость
                aoLight.LightSky = (byte)(_storage.Light[index] & 15);
                aoLight.LightBlock = (byte)(_storage.Light[index] >> 4);
                aoLight.Aol = 1;
                return aoLight;
            }

            id = id & 0xFFF;
            _blockCheck = Ce.Blocks.BlockObjects[id];
            aoLight.Aol = _blockCheck.IsNotTransparent || (_blockCheck.Liquid && Gi.Block.Liquid) ? 0 : 1;
            aoLight.Aoc = _blockCheck.АmbientOcclusion ? 1 : 0;

            if (aoLight.Aol == 0)
            {
                aoLight.LightBlock = 0;
                aoLight.LightSky = 0;
                return aoLight;
            }
            //_isDraw = id == 0 || _blockCheck.AllSideForcibly;
            _isDraw = id == 0 /*|| !_blockCheck.GetCullFace(Met, _indexSide)*/ || _blockCheck.Translucent;
            //if (_isDraw && (_blockCheck.Material == Block.Material && !_blockCheck.BlocksNotSame)) _isDraw = false;

            if (_isDraw)
            {
                // Яркость берётся из данных блока
                aoLight.LightSky = (byte)(_storage.Light[index] & 15);
                aoLight.LightBlock = (byte)(_storage.Light[index] >> 4);
            }
            return aoLight;
        }

        /// <summary>
        /// Структура для 4-ёх вершин цвета и освещения
        /// </summary>
        protected class AmbientOcclusionLights
        {
            public readonly AmbientOcclusionLight[] Aos = new AmbientOcclusionLight[8];

            private readonly int[] _lightBlock = new int[4];
            private readonly int[] _lightSky = new int[4];
            private readonly Vector3[] _color = new Vector3[4];
            private readonly int[] _aols = new int[4];
            private readonly int[] _aocs = new int[4];

            private int lb, ls, count;

            public void InitAmbientOcclusionLights()
            {
                //a, b, c, d, e, f, g, h
                //0, 1, 2, 3, 4, 5, 6, 7

                _lightBlock[0] = Aos[2].LightBlock + Aos[3].LightBlock + Aos[4].LightBlock;
                _lightBlock[1] = Aos[1].LightBlock + Aos[2].LightBlock + Aos[5].LightBlock;
                _lightBlock[2] = Aos[0].LightBlock + Aos[1].LightBlock + Aos[6].LightBlock;
                _lightBlock[3] = Aos[0].LightBlock + Aos[3].LightBlock + Aos[7].LightBlock;

                _lightSky[0] = Aos[2].LightSky + Aos[3].LightSky + Aos[4].LightSky;
                _lightSky[1] = Aos[1].LightSky + Aos[2].LightSky + Aos[5].LightSky;
                _lightSky[2] = Aos[0].LightSky + Aos[1].LightSky + Aos[6].LightSky;
                _lightSky[3] = Aos[0].LightSky + Aos[3].LightSky + Aos[7].LightSky;

                _color[0] = Aos[2].Color + Aos[3].Color + Aos[4].Color;
                _color[1] = Aos[1].Color + Aos[2].Color + Aos[5].Color;
                _color[2] = Aos[0].Color + Aos[1].Color + Aos[6].Color;
                _color[3] = Aos[0].Color + Aos[3].Color + Aos[7].Color;

                _aols[0] = Aos[2].Aol + Aos[3].Aol + Aos[4].Aol;
                _aols[1] = Aos[1].Aol + Aos[2].Aol + Aos[5].Aol;
                _aols[2] = Aos[0].Aol + Aos[1].Aol + Aos[6].Aol;
                _aols[3] = Aos[0].Aol + Aos[3].Aol + Aos[7].Aol;

                _aocs[0] = Aos[2].Aoc + Aos[3].Aoc + Aos[4].Aoc;
                _aocs[1] = Aos[1].Aoc + Aos[2].Aoc + Aos[5].Aoc;
                _aocs[2] = Aos[0].Aoc + Aos[1].Aoc + Aos[6].Aoc;
                _aocs[3] = Aos[0].Aoc + Aos[3].Aoc + Aos[7].Aoc;
            }

            public byte GetLight(int index, byte light)
            {
                lb = (light & 0xF0) >> 4;
                ls = light & 0xF;
                count = 1 + _aols[index];
                lb = (_lightBlock[index] + lb) / count;
                ls = (_lightSky[index] + ls) / count;
                return (byte)(lb << 4 | ls);
            }

            public Vector3 GetColor(int index, Vector3 color, float lightPole)
            {
                Vector3 c = (_color[index] + color) / 4f * (1f - _aocs[index] * .2f);
                c.X -= lightPole; if (c.X < 0) c.X = 0;
                c.Y -= lightPole; if (c.Y < 0) c.Y = 0;
                c.Z -= lightPole; if (c.Z < 0) c.Z = 0;
                return c;
            }

            public Vector3 GetColorNotAO(int index, Vector3 color, float lightPole)
            {
                Vector3 c = (_color[index] + color) / 4f;
                c.X -= lightPole; if (c.X < 0) c.X = 0;
                c.Y -= lightPole; if (c.Y < 0) c.Y = 0;
                c.Z -= lightPole; if (c.Z < 0) c.Z = 0;
                return c;
            }
        }
        /// <summary>
        /// Структура данных (AmbientOcclusion и яркости от блока и неба) одно стороны для вершины
        /// </summary>
        protected struct AmbientOcclusionLight
        {
            public byte LightBlock;
            public byte LightSky;
            public Vector3 Color;
            public int Aol;
            public int Aoc;
        }

        #endregion
    }
}
