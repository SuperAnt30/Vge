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
        public BlockState BlockSt;
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
        /// Тень на углах
        /// </summary>
        private bool _ambientOcclusion = true;
        private readonly int[] _resultSide = new int[] { -1, -1, -1, -1, -1, -1 };
        /// <summary>
        /// Объект рендера чанков
        /// </summary>
        private ChunkRender _chunkRender;
        /// <summary>
        /// Объект блока для проверки
        /// </summary>
        private BlockBase _blockCheck;
        private int _metCheck;
        private ChunkRender _chunkCheck;
        /// <summary>
        /// Высота блоков в чанке, 127
        /// </summary>
        private int _numberBlocksY;

        private ChunkStorage _storage;
        private bool _isDraw;
        private int _stateLight, _stateLightHis;
        private QuadSide[] _rectangularSides;
        private QuadSide _rectangularSide;
        private readonly AmbientOcclusionLights _ambient = new AmbientOcclusionLights();
        private float _lightPole;
        private Vector3 _color;
        private readonly ColorsLights _colorLight = new ColorsLights();

        private int i, count, index, id, s1, s2, s3, s4, s5, s6, ntdbv, rs, side;
        private int xc, yc, zc, xb, yb, zb, xcn, zcn, pX, pY, pZ;

        /// <summary>
        /// Создание блока генерации для мира
        /// </summary>
        public BlockRenderFull()
        {
            blockUV.Buffer = Gi.Vertex;
        }

        public virtual void InitChunk(ChunkRender chunkRender)
        {
            _chunkRender = chunkRender;
            _numberBlocksY = _chunkRender.Settings.NumberBlocks;
        }

        /// <summary>
        /// Получть cетку сплошного блока
        /// </summary>
        public void RenderMesh()
        {
            // 0.7 - 0.8
            // 0.8 - 0.9 virtual
            
            xc = _chunkRender.X;
            zc = _chunkRender.Y;

            // 0.75 - 0.85
            if (PosChunkY + 1 > _numberBlocksY) rs = _resultSide[0] = 0x0F;
            else rs = _resultSide[0] = _GetBlockSideState(PosChunkX, PosChunkY + 1, PosChunkZ, false, true);

          //  return;

            if (PosChunkY - 1 < 0) rs += _resultSide[1] = 0x0F;
            else rs += _resultSide[1] = _GetBlockSideState(PosChunkX, PosChunkY - 1, PosChunkZ, false);

            yc = PosChunkY >> 4;
            rs += _resultSide[2] = _GetBlockSideState(PosChunkX + 1, PosChunkY, PosChunkZ, true);
            rs += _resultSide[3] = _GetBlockSideState(PosChunkX - 1, PosChunkY, PosChunkZ, true);
            rs += _resultSide[4] = _GetBlockSideState(PosChunkX, PosChunkY, PosChunkZ - 1, true);
            rs += _resultSide[5] = _GetBlockSideState(PosChunkX, PosChunkY, PosChunkZ + 1, true);

         //   return;
            _stateLightHis = -1;
            if (rs != -6)
            {
                if (Block.UseNeighborBrightness) _stateLightHis = BlockSt.LightBlock << 4 | BlockSt.LightSky & 0xF;
                //damagedBlocksValue = chunk.GetDestroyBlocksValue(posChunkX, posChunkY, posChunkZ);
                _RenderMeshBlock();
            }
        }


        /// <summary>
        /// Подготовить кэш blockSideCache, прорисовывается ли сторона и её яркость
        /// </summary>
        private int _GetBlockSideState(int x, int y, int z, bool isNearbyChunk, bool isUp = false)
        {
            if (isNearbyChunk)
            {
                xcn = x >> 4;
                zcn = z >> 4;
                xc = _chunkRender.CurrentChunkX;
                zc = _chunkRender.CurrentChunkY;

                
                // Определяем рабочий чанк соседнего блока
                if (xcn == 0 && zcn == 0)
                {
                    _storage = _chunkRender.StorageArrays[yc];
                }
                else
                {
                    xc += xcn;
                    zc += zcn;
                    _chunkCheck = _chunkRender.Chunk(xcn, zcn);
                    if (_chunkCheck == null || !_chunkCheck.IsChunkPresent)
                    {
                        return 0x0F; // Только яркость неба макс
                    }
                    _storage = _chunkCheck.StorageArrays[yc];
                }
            }
            else
            {
                _storage = _chunkRender.StorageArrays[y >> 4];
            }

            xb = x & 15;
            yb = y & 15;
            zb = z & 15;
            i = yb << 8 | zb << 4 | xb;

            if (_storage.CountBlock > 0)
            {
                id = _storage.Data[i];
                _metCheck = id >> 12;
                id = id & 0xFFF;
            }
            else
            {
                _metCheck = 0;
                id = 0;
            }

            if (id == 0)
            {
                return _storage.LightBlock[i] << 4 | _storage.LightSky[i] & 0xF;
            }

            _blockCheck = Blocks.BlockObjects[id];

            if (_blockCheck.AllSideForcibly || (isUp && Block.Liquid))
            {
                if (!(!_blockCheck.BlocksNotSame))// && _blockCheck.Material == Block.Material))
                {
                    //EnumMaterial material = _blockCheck.Material.EMaterial;
                    return (_storage.LightBlock[i] << 4 | _storage.LightSky[i] & 0xF);
                      //  + ((block.Material.EMaterial == EnumMaterial.Water && (material == EnumMaterial.Glass || material == EnumMaterial.Oil)) ? 1024 : 0);
                }
            }
            return -1;
        }


        /// <summary>
        /// Рендер сеток сторон блока которые требуются для прорисовки
        /// </summary>
        private void _RenderMeshBlock()
        {
            _rectangularSides = Block.GetQuads(Met, xc, zc, PosChunkX, PosChunkZ);
            count = _rectangularSides.Length;
            for (i = 0; i < count; i++)
            {
                _rectangularSide = _rectangularSides[i];
                side = _rectangularSide.Side;
                _stateLight = _resultSide[side];
                if (_stateLight != -1)
                {
                    // Смекшировать яркость, в зависимости от требований самой яркой
                    if (_stateLight != _stateLightHis
                        && Block.UseNeighborBrightness && _stateLightHis != -1)
                    {
                        _stateLight = _stateLight & 0xFF;
                        s1 = _stateLight >> 4;
                        s2 = _stateLight & 0x0F;
                        s3 = _stateLightHis >> 4;
                        s4 = _stateLightHis & 0x0F;

                        s5 = s1 > s3 ? s1 : s3;
                        s6 = s2 > s4 ? s2 : s4;

                        _stateLight = (s5 << 4 | s6 & 0xF);
                    }

                    _lightPole = _rectangularSide.LightPole;
                    _GenColors((byte)(_stateLight & 0xFF));

                    blockUV.ColorsR = _colorLight.ColorR;
                    blockUV.ColorsG = _colorLight.ColorG;
                    blockUV.ColorsB = _colorLight.ColorB;
                    blockUV.Lights = _colorLight.Light;
                    blockUV.PosCenterX = PosChunkX;
                    blockUV.PosCenterY = PosChunkY;
                    blockUV.PosCenterZ = PosChunkZ;
                    blockUV.AnimationFrame = _rectangularSide.AnimationFrame;
                    blockUV.AnimationPause = _rectangularSide.AnimationPause;
                    blockUV.Vertex = _rectangularSide.Vertex;
                    blockUV.Building();

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

        /// <summary>
        /// Сгенерировать цвета на каждый угол, если надо то AmbientOcclusion
        /// </summary>
        private void _GenColors(byte light)
        {
            _color = _GetBiomeColor(_chunkRender, PosChunkX, PosChunkZ);
            if (_ambientOcclusion && (Block.АmbientOcclusion || Block.BiomeColor || Block.Liquid))
            {
                _GetAmbientOcclusionLights();
                _lightPole *= .5f;
                if (Block.Liquid)
                {
                    _colorLight.InitColorsLights(
                        _ambient.GetColorNotAO(0, _color, _lightPole), _ambient.GetColorNotAO(1, _color, _lightPole),
                        _ambient.GetColorNotAO(2, _color, _lightPole), _ambient.GetColorNotAO(3, _color, _lightPole),
                        _ambient.GetLight(0, light), _ambient.GetLight(1, light),
                        _ambient.GetLight(2, light), _ambient.GetLight(3, light)
                    );
                }
                else
                {
                    _colorLight.InitColorsLights(
                        _ambient.GetColor(0, _color, _lightPole), _ambient.GetColor(1, _color, _lightPole),
                        _ambient.GetColor(2, _color, _lightPole), _ambient.GetColor(3, _color, _lightPole),
                        _ambient.GetLight(0, light), _ambient.GetLight(1, light),
                        _ambient.GetLight(2, light), _ambient.GetLight(3, light));
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
                _colorLight.Light[0] = _colorLight.Light[1] = _colorLight.Light[2] = _colorLight.Light[3] = light;
            }
        }

        /// <summary>
        /// Получить цвет в зависимости от биома, цвет определяем потипу
        /// </summary>
        /// <param name="bx">0-15</param>
        /// <param name="bz">0-15</param>
        /// <returns></returns>
        protected virtual Vector3 _GetBiomeColor(ChunkBase chunk, int bx, int bz)
        {
            // подготовка для теста плавности цвета
            //if (_rectangularSide.IsBiomeColor())
            //{
            //    if (_rectangularSide.IsBiomeColorGrass())
            //    {
            //        return BlockColorBiome.Grass(chunk.biome[bx << 4 | bz]);
            //    }
            //    if (_rectangularSide.IsYourColor())
            //    {
            //        return Block.GetColorGuiOrPartFX();
            //    }
            //}
            return ColorWhite;
        }

        #region AmbientOcclusion

        /// <summary>
        /// Получить все 4 вершины AmbientOcclusion и яркости от блока и неба
        /// </summary>
        private void _GetAmbientOcclusionLights()
        {
            switch (side)
            {
                case 0:
                    _ambient.Aos[0] = _GetAmbientOcclusionLight(1, 1, 0);
                    _ambient.Aos[1] = _GetAmbientOcclusionLight(0, 1, 1);
                    _ambient.Aos[2] = _GetAmbientOcclusionLight(-1, 1, 0);
                    _ambient.Aos[3] = _GetAmbientOcclusionLight(0, 1, -1);
                    _ambient.Aos[4] = _GetAmbientOcclusionLight(-1, 1, -1);
                    _ambient.Aos[5] = _GetAmbientOcclusionLight(-1, 1, 1);
                    _ambient.Aos[6] = _GetAmbientOcclusionLight(1, 1, 1);
                    _ambient.Aos[7] = _GetAmbientOcclusionLight(1, 1, -1);
                    break;
                case 1:
                    _ambient.Aos[2] = _GetAmbientOcclusionLight(1, -1, 0);
                    _ambient.Aos[1] = _GetAmbientOcclusionLight(0, -1, 1);
                    _ambient.Aos[0] = _GetAmbientOcclusionLight(-1, -1, 0);
                    _ambient.Aos[3] = _GetAmbientOcclusionLight(0, -1, -1);
                    _ambient.Aos[7] = _GetAmbientOcclusionLight(-1, -1, -1);
                    _ambient.Aos[6] = _GetAmbientOcclusionLight(-1, -1, 1);
                    _ambient.Aos[5] = _GetAmbientOcclusionLight(1, -1, 1);
                    _ambient.Aos[4] = _GetAmbientOcclusionLight(1, -1, -1);
                    break;
                case 2:
                    _ambient.Aos[1] = _GetAmbientOcclusionLight(1, 1, 0);
                    _ambient.Aos[0] = _GetAmbientOcclusionLight(1, 0, 1);
                    _ambient.Aos[3] = _GetAmbientOcclusionLight(1, -1, 0);
                    _ambient.Aos[2] = _GetAmbientOcclusionLight(1, 0, -1);
                    _ambient.Aos[4] = _GetAmbientOcclusionLight(1, -1, -1);
                    _ambient.Aos[7] = _GetAmbientOcclusionLight(1, -1, 1);
                    _ambient.Aos[6] = _GetAmbientOcclusionLight(1, 1, 1);
                    _ambient.Aos[5] = _GetAmbientOcclusionLight(1, 1, -1);
                    break;
                case 3:
                    _ambient.Aos[1] = _GetAmbientOcclusionLight(-1, 1, 0);
                    _ambient.Aos[2] = _GetAmbientOcclusionLight(-1, 0, 1);
                    _ambient.Aos[3] = _GetAmbientOcclusionLight(-1, -1, 0);
                    _ambient.Aos[0] = _GetAmbientOcclusionLight(-1, 0, -1);
                    _ambient.Aos[7] = _GetAmbientOcclusionLight(-1, -1, -1);
                    _ambient.Aos[4] = _GetAmbientOcclusionLight(-1, -1, 1);
                    _ambient.Aos[5] = _GetAmbientOcclusionLight(-1, 1, 1);
                    _ambient.Aos[6] = _GetAmbientOcclusionLight(-1, 1, -1);
                    break;
                case 4:
                    _ambient.Aos[1] = _GetAmbientOcclusionLight(0, 1, -1);
                    _ambient.Aos[0] = _GetAmbientOcclusionLight(1, 0, -1);
                    _ambient.Aos[3] = _GetAmbientOcclusionLight(0, -1, -1);
                    _ambient.Aos[2] = _GetAmbientOcclusionLight(-1, 0, -1);
                    _ambient.Aos[4] = _GetAmbientOcclusionLight(-1, -1, -1);
                    _ambient.Aos[7] = _GetAmbientOcclusionLight(1, -1, -1);
                    _ambient.Aos[6] = _GetAmbientOcclusionLight(1, 1, -1);
                    _ambient.Aos[5] = _GetAmbientOcclusionLight(-1, 1, -1);
                    break;
                case 5:
                    _ambient.Aos[1] = _GetAmbientOcclusionLight(0, 1, 1);
                    _ambient.Aos[2] = _GetAmbientOcclusionLight(1, 0, 1);
                    _ambient.Aos[3] = _GetAmbientOcclusionLight(0, -1, 1);
                    _ambient.Aos[0] = _GetAmbientOcclusionLight(-1, 0, 1);
                    _ambient.Aos[7] = _GetAmbientOcclusionLight(-1, -1, 1);
                    _ambient.Aos[4] = _GetAmbientOcclusionLight(1, -1, 1);
                    _ambient.Aos[5] = _GetAmbientOcclusionLight(1, 1, 1);
                    _ambient.Aos[6] = _GetAmbientOcclusionLight(-1, 1, 1);
                    break;
            }

            _ambient.InitAmbientOcclusionLights();
        }

        /// <summary>
        /// Подготовить кэш blockSideCache, прорисовывается ли сторона и её яркость
        /// Получть данные (AmbientOcclusion и яркость) одно стороны для вершины
        /// </summary>
        private AmbientOcclusionLight _GetAmbientOcclusionLight(int x, int y, int z)
        {
            pX = PosChunkX + x;
            pY = PosChunkY + y;
            pZ = PosChunkZ + z;
            AmbientOcclusionLight aoLight = new AmbientOcclusionLight();

            xcn = pX >> 4;
            zcn = pZ >> 4;
            xc = PosChunkX + xcn;
            zc = PosChunkY + zcn;
            xb = pX & 15;
            zb = pZ & 15;

            // проверка высоты
            if (pY < 0)
            {
                aoLight.LightSky = 0;
                aoLight.Color = ColorWhite;
                return aoLight;
            }
            // Определяем рабочий чанк соседнего блока
            _chunkCheck = (xc == PosChunkX && zc == PosChunkY) ? _chunkRender : _chunkRender.Chunk(xcn, zcn);
            if (_chunkCheck == null || !_chunkCheck.IsChunkPresent)
            {
                aoLight.LightSky = 15;
                aoLight.Color = _color;
                aoLight.Aol = 1;
                return aoLight;
            }
            aoLight.Color = _GetBiomeColor(_chunkCheck, xb, zb);
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
                aoLight.LightSky = _storage.LightSky[index];
                aoLight.LightBlock = _storage.LightBlock[index];
                aoLight.Aol = 1;
                return aoLight;
            }

            id = _storage.Data[index];
            _metCheck = id >> 12;
            id = id & 0xFFF;
            _blockCheck = Blocks.BlockObjects[id];
            aoLight.Aoc = _blockCheck.АmbientOcclusion ? 1 : 0;
            aoLight.Aol = _blockCheck.IsNotTransparent || (_blockCheck.Liquid && Block.Liquid) ? 0 : 1;

            _isDraw = id == 0 || _blockCheck.AllSideForcibly;
            //if (_isDraw && (_blockCheck.Material == Block.Material && !_blockCheck.BlocksNotSame)) _isDraw = false;

            if (_isDraw)
            {
                // Яркость берётся из данных блока
                aoLight.LightBlock = _storage.LightBlock[index];
                aoLight.LightSky = _storage.LightSky[index];
            }

            if (aoLight.Aol == 0)
            {
                aoLight.LightBlock = 0;
                aoLight.LightSky = 0;
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
