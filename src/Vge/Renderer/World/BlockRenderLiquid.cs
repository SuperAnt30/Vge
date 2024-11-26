﻿using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World.Block;
using Vge.World.Block.List;
using Vge.World.Chunk;
using WinGL.Util;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Объект рендера жидкого блока
    /// </summary>
    public class BlockRenderLiquid : BlockRenderFull
    {
        private float u1, u2, u3, u4, v1, v2, v3, v4;
        private float h00, h10, h01, h11;
        private int up00, up10, up20, up01, up11, up21, up02, up12, up22;
        private int l00, l10, l20, l01, l11, l21, l02, l12, l22;

        private float _angleFlow;
        /// <summary>
        /// Структура активной стороны жидкого блока
        /// </summary>
        private SideLiquid _sideLiquid;

        public BlockRenderLiquid(VertexBuffer vertex) : base(vertex)
        {

        }

        public override void RenderSide()
        {
            if (Gi.Block.UseNeighborBrightness) _stateLightHis = LightBlockSky;
            else _stateLightHis = -1;

            int y = PosChunkY + 1;
            // Получаем данные вверхнего уровня блока, в облости 3*3
            up00 = _GetLiquidBlock(PosChunkX - 1, y, PosChunkZ - 1, true);
            up10 = _GetLiquidBlock(PosChunkX, y, PosChunkZ - 1, true);
            up20 = _GetLiquidBlock(PosChunkX + 1, y, PosChunkZ - 1, true);
            up01 = _GetLiquidBlock(PosChunkX - 1, y, PosChunkZ, true);
            up11 = _GetLiquidBlock(PosChunkX, y, PosChunkZ, false);
            up21 = _GetLiquidBlock(PosChunkX + 1, y, PosChunkZ, true);
            up02 = _GetLiquidBlock(PosChunkX - 1, y, PosChunkZ + 1, true);
            up12 = _GetLiquidBlock(PosChunkX, y, PosChunkZ + 1, true);
            up22 = _GetLiquidBlock(PosChunkX + 1, y, PosChunkZ + 1, true);

            // Получаем данные этого уровня блока, в облости 3*3
            l00 = _GetLiquidBlock(PosChunkX - 1, PosChunkY, PosChunkZ - 1, true);
            l10 = _GetLiquidBlock(PosChunkX, PosChunkY, PosChunkZ - 1, true);
            l20 = _GetLiquidBlock(PosChunkX + 1, PosChunkY, PosChunkZ - 1, true);
            l01 = _GetLiquidBlock(PosChunkX - 1, PosChunkY, PosChunkZ, true);
            l11 = _GetLiquidBlock(PosChunkX, PosChunkY, PosChunkZ, false);
            l21 = _GetLiquidBlock(PosChunkX + 1, PosChunkY, PosChunkZ, true);
            l02 = _GetLiquidBlock(PosChunkX - 1, PosChunkY, PosChunkZ + 1, true);
            l12 = _GetLiquidBlock(PosChunkX, PosChunkY, PosChunkZ + 1, true);
            l22 = _GetLiquidBlock(PosChunkX + 1, PosChunkY, PosChunkZ + 1, true);

            // Получить высоту вершины 0..1
            h00 = _HeightVertexLiquid(up00, up10, up01, up11, new int[] { l00, l10, l01, l11 });
            h10 = _HeightVertexLiquid(up10, up20, up11, up21, new int[] { l10, l20, l11, l21 });
            h01 = _HeightVertexLiquid(up01, up11, up02, up12, new int[] { l01, l11, l02, l12 });
            h11 = _HeightVertexLiquid(up11, up21, up12, up22, new int[] { l11, l21, l12, l22 });

            // Сначало изнутри
            if (_resultSide[0] != -1)
            {
                // Условие, если текучая вода или стоячая но над стоячей вверхний блок 
                // не может быть стоячей водной под сплошным блоком
                if (l11 != 15 || (l11 == 15 && (up00 > -2 || up10 > -2 || up20 > -2
                    || up01 > -2 || up11 > -2 || up21 > -2
                    || up02 > -2 || up12 > -2 || up22 > -2)))
                {
                    _Up();
                    blockUV.BuildingLiquidInside();
                }

            }
            if (_resultSide[1] != -1)
            {
                _Down();
                blockUV.BuildingLiquidInside();
            }
            if (_resultSide[2] != -1)
            {
                _East();
                blockUV.BuildingLiquidInside();
            }
            if (_resultSide[3] != -1)
            {
                _West();
                blockUV.BuildingLiquidInside();
            }
            if (_resultSide[4] != -1)
            {
                _North();
                blockUV.BuildingLiquidInside();
            }
            if (_resultSide[5] != -1)
            {
                _South();
                blockUV.BuildingLiquidInside();
            }

            // TODO::2024-11-26 потестировать, что быстрее, может создать отдельный буфер, и потом совмещать.
            // Не будет повторных просчётов для определения размеров фигур

            // Потом снаружи
            if (_resultSide[0] != -1)
            {
                _Up();
                blockUV.BuildingLiquidOutside();
            }
            if (_resultSide[1] != -1)
            {
                _Down();
                blockUV.BuildingLiquidOutside();
            }
            if (_resultSide[2] != -1)
            {
                _East();
                blockUV.BuildingLiquidOutside();
            }
            if (_resultSide[3] != -1)
            {
                _West();
                blockUV.BuildingLiquidOutside();
            }
            if (_resultSide[4] != -1)
            {
                _North();
                blockUV.BuildingLiquidOutside();
            }
            if (_resultSide[5] != -1)
            {
                _South();
                blockUV.BuildingLiquidOutside();
            }
        }

        private void _Up()
        {
            _sideLiquid = Gi.Block.GetSideLiquid(0);
            _stateLight = _resultSide[0];
            _angleFlow = BlockLiquid.GetAngleFlow(l11, l01, l10, l12, l21);
            _GenLightMix();

            if (_angleFlow < -999)
            {
                // Без вращения
                blockUV.AnimationFrame = _sideLiquid.AnimationFrame;
                blockUV.AnimationPause = _sideLiquid.AnimationPause;
                u1 = _sideLiquid.U;
                v1 = _sideLiquid.V;
                u2 = _sideLiquid.U;
                v2 = _sideLiquid.V + Ce.ShaderAnimOffset;
                u3 = _sideLiquid.U + Ce.ShaderAnimOffset;
                v3 = _sideLiquid.V + Ce.ShaderAnimOffset;
                u4 = _sideLiquid.U + Ce.ShaderAnimOffset;
                v4 = _sideLiquid.V;
            }
            else
            {
                // Вращаем
                SideLiquid sideLiquidCache = Gi.Block.GetSideLiquid(2);
                blockUV.AnimationFrame = sideLiquidCache.AnimationFrame;
                blockUV.AnimationPause = sideLiquidCache.AnimationPause;

                float fs = Glm.Sin(_angleFlow) * .25f;
                float fc = Glm.Cos(_angleFlow) * .25f;
                float k = Ce.ShaderAnimOffset * 2f;
                float fu = sideLiquidCache.U + Ce.ShaderAnimOffset;
                float fv = sideLiquidCache.V + Ce.ShaderAnimOffset;
                float fmm = (-fc - fs) * k;
                float fmp = (-fc + fs) * k;
                float fpp = (fc + fs) * k;
                float fpm = (fc - fs) * k;

                u1 = fu + fmm;
                v1 = fv + fmp;
                u2 = fu + fmp;
                v2 = fv + fpp;
                u3 = fu + fpp;
                v3 = fv + fpm;
                u4 = fu + fpm;
                v4 = fv + fmm;
            }
            _lightPole = _sideLiquid.LightPole;
            _GenColors();

            blockUV.ColorsR = _colorLight.ColorR;
            blockUV.ColorsG = _colorLight.ColorG;
            blockUV.ColorsB = _colorLight.ColorB;
            blockUV.Lights = _colorLight.Light;

            // Вверх
            blockUV.Vertex = new Vertex3d[]
            {
                new Vertex3d(PosChunkX, PosChunkY + h00, PosChunkZ, u1, v1),
                new Vertex3d(PosChunkX, PosChunkY + h01, PosChunkZ + 1,u2, v2),
                new Vertex3d(PosChunkX + 1, PosChunkY + h11, PosChunkZ + 1, u3, v3),
                new Vertex3d(PosChunkX + 1, PosChunkY + h10, PosChunkZ, u4, v4)
            };
        }

        #region SideAssign

        private void _Down()
        {
            _SideAssign(1);

            u1 = _sideLiquid.U;
            v1 = _sideLiquid.V;
            u2 = u1 + Ce.ShaderAnimOffset;
            v2 = v1 + Ce.ShaderAnimOffset;

            blockUV.Vertex = new Vertex3d[]
            {
                new Vertex3d(PosChunkX + 1, PosChunkY, PosChunkZ, u2, v1),
                new Vertex3d(PosChunkX + 1, PosChunkY, PosChunkZ + 1, u2, v2),
                new Vertex3d(PosChunkX, PosChunkY, PosChunkZ + 1, u1, v2),
                new Vertex3d(PosChunkX, PosChunkY, PosChunkZ, u1, v1)
            };
        }

        private void _East()
        {
            _SideAssign(2);

            u3 = u4 = _sideLiquid.U;
            u1 = u2 = u3 + Ce.ShaderAnimOffset;
            v1 = v4 = _sideLiquid.V + Ce.ShaderAnimOffset;
            v2 = v1 - Ce.ShaderAnimOffset * h10;
            v3 = v1 - Ce.ShaderAnimOffset * h11;

            blockUV.Vertex = new Vertex3d[]
            {
                new Vertex3d(PosChunkX + 1, PosChunkY, PosChunkZ, u1, v1),
                new Vertex3d(PosChunkX + 1, PosChunkY + h10, PosChunkZ,u2, v2),
                new Vertex3d(PosChunkX + 1, PosChunkY + h11, PosChunkZ + 1, u3, v3),
                new Vertex3d(PosChunkX + 1, PosChunkY, PosChunkZ + 1, u4, v4)
            };
        }

        private void _West()
        {
            _SideAssign(3);

            u3 = u4 = _sideLiquid.U;
            u1 = u2 = u3 + Ce.ShaderAnimOffset;
            v1 = v4 = _sideLiquid.V + Ce.ShaderAnimOffset;
            v2 = v1 - Ce.ShaderAnimOffset * h01;
            v3 = v1 - Ce.ShaderAnimOffset * h00;

            blockUV.Vertex = new Vertex3d[]
            {
                new Vertex3d(PosChunkX, PosChunkY, PosChunkZ + 1, u1, v1),
                new Vertex3d(PosChunkX, PosChunkY + h01, PosChunkZ + 1,u2, v2),
                new Vertex3d(PosChunkX, PosChunkY + h00, PosChunkZ, u3, v3),
                new Vertex3d(PosChunkX, PosChunkY, PosChunkZ, u4, v4)
            };
        }

        private void _North()
        {
            _SideAssign(4);

            u3 = u4 = _sideLiquid.U;
            u1 = u2 = u3 + Ce.ShaderAnimOffset;
            v1 = v4 = _sideLiquid.V + Ce.ShaderAnimOffset;
            v2 = v1 - Ce.ShaderAnimOffset * h00;
            v3 = v1 - Ce.ShaderAnimOffset * h10;

            blockUV.Vertex = new Vertex3d[]
            {
                new Vertex3d(PosChunkX, PosChunkY, PosChunkZ, u1, v1),
                new Vertex3d(PosChunkX, PosChunkY + h00, PosChunkZ,u2, v2),
                new Vertex3d(PosChunkX + 1, PosChunkY + h10, PosChunkZ, u3, v3),
                new Vertex3d(PosChunkX + 1, PosChunkY, PosChunkZ, u4, v4)
            };
        }

        private void _South()
        {
            _SideAssign(5);

            u3 = u4 = _sideLiquid.U;
            u1 = u2 = u3 + Ce.ShaderAnimOffset;
            v1 = v4 = _sideLiquid.V + Ce.ShaderAnimOffset;
            v2 = v1 - Ce.ShaderAnimOffset * h11;
            v3 = v1 - Ce.ShaderAnimOffset * h01;

            blockUV.Vertex = new Vertex3d[]
            {
                new Vertex3d(PosChunkX + 1, PosChunkY, PosChunkZ + 1, u1, v1),
                new Vertex3d(PosChunkX + 1, PosChunkY + h11, PosChunkZ + 1,u2, v2),
                new Vertex3d(PosChunkX, PosChunkY + h01, PosChunkZ + 1, u3, v3),
                new Vertex3d(PosChunkX, PosChunkY, PosChunkZ + 1, u4, v4)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _SideAssign(int index)
        {
            _sideLiquid = Gi.Block.GetSideLiquid(index);
            _stateLight = _resultSide[index];
            _GenLightMix();
            _lightPole = _sideLiquid.LightPole;
            _GenColors();

            blockUV.AnimationFrame = _sideLiquid.AnimationFrame;
            blockUV.AnimationPause = _sideLiquid.AnimationPause;
            blockUV.ColorsR = _colorLight.ColorR;
            blockUV.ColorsG = _colorLight.ColorG;
            blockUV.ColorsB = _colorLight.ColorB;
            blockUV.Lights = _colorLight.Light;
        }

        #endregion

        /// <summary>
        /// Смекшировать яркость, в зависимости от требований самой яркой
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _GenLightMix()
        {
            if (_stateLight != _stateLightHis
                && Gi.Block.UseNeighborBrightness && _stateLightHis != -1)
            {
                s1 = _stateLight >> 4;
                s2 = _stateLight & 0x0F;
                s3 = _stateLightHis >> 4;
                s4 = _stateLightHis & 0x0F;
                _stateLight = (s1 > s3 ? s1 : s3) << 4 | (s2 > s4 ? s2 : s4) & 0xF;
            }
        }

        /// <summary>
        /// Получить значение блока жидкости мет данных.
        /// Где 15 это стоячаяя или целый растекаемы, 1 минимум в воде, 3 в нефте и лаве минимум,
        /// -1 блок не жидкости, -2 сплошной блок не жидкости
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int _GetLiquidBlock(int x, int y, int z, bool isNearbyChunk)
        {
            if (y >= _numberBlocksY) return -1;

            yc = y >> 4;
            if (isNearbyChunk)
            {
                if (_storage.KeyCash != _keyCash)
                {
                    // Если разный кеш, значит разные чанки, берём из соседей
                    _chunkCheck = _chunkRender.Chunk(x >> 4, z >> 4);
                    if (_chunkCheck == null || !_chunkCheck.IsChunkPresent)
                    {
                        return -1;
                    }
                    _storage = _chunkCheck.StorageArrays[yc];
                }
                else if (_storage.Index != yc)
                {
                    // Если изменён только вверх
                    _storage = _chunkRender.StorageArrays[yc];
                }
            }
            else
            {
                _storage = _chunkRender.StorageArrays[y >> 4];
            }

            xb = x & 15;
            yb = y & 15;
            zb = z & 15;
            int i = yb << 8 | zb << 4 | xb;

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

            if (_blockCheck.Id != id)
            {
                _blockCheck = Ce.Blocks.BlockObjects[id];
            }

            if (_blockCheck.Liquid)
            {
                if (_metCheck == 0) return 15;
                return _metCheck;
            }

            if (_blockCheck.CullFaceAll) return -2;

            
            
            //EnumMaterial eMaterial = blockCheck.Material.EMaterial;
            //if ((materialLiquid != EnumMaterial.Lava && (eMaterial == EnumMaterial.Oil || eMaterial == EnumMaterial.Water))
            //    || (materialLiquid == EnumMaterial.Lava && eMaterial == EnumMaterial.Lava))
            //{
            //    if (id == 13 || id == 15 || id == 17)
            //    {
            //        // стоячая жидкость
            //        return 15;
            //    }
            //    return _metCheck;
            //}

            return -1;
        }

        /// <summary>
        /// Получить высоту вершины жидкости 0..1
        /// </summary>
        private float _HeightVertexLiquid(int up00, int up10, int up01, int up11, int[] vs)
        {
            if (up00 > 0 || up10 > 0 || up01 > 0 || up11 > 0)
            {
                return 1f;
            }
            float count = 0;
            float value = 0;
            int metLevel;
            for (int i = 0; i < 4; i++)
            {
                metLevel = vs[i];
                if (metLevel > 0)
                {
                    if (metLevel == 15)
                    {
                        // value чем больше тут цифра, тем ниже уровень 
                        // (.5 почти на уровне, 2.0 около двух пикселей от верха)
                        value++;
                        count += 10;
                    }
                    else
                    {
                        value += (15 - metLevel) / 16f;
                        count++;
                    }
                }
                else if (metLevel == -1)
                {
                    value++;
                    count++;
                }
            }
            return 1f - value / count;
        }

        /// <summary>
        /// Получить цвет в зависимости от биома, цвет определяем потипу
        /// </summary>
        /// <param name="bx">0-15</param>
        /// <param name="bz">0-15</param>
        /// <returns></returns>
        protected override Vector3 _GetBiomeColor(ChunkBase chunk, int bx, int bz)
        {
            // подготовка для теста плавности цвета
            if (_sideLiquid.TypeColor == 0)
            {
                // Нет цвета
                return ColorWhite;
            }
            if (_sideLiquid.TypeColor == 4)
            {
                // Свой цвет
                return Gi.Block.Color;
            }
            return ColorWhite;
        }
    }
}
