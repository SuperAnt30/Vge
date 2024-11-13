using System.Collections.Generic;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;
using WinGL.Util;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Объект рендера чанка, он же клиентский чанк
    /// </summary>
    public class ChunkRender : ChunkBase
    {
        /// <summary>
        /// Массив буфера блоков альфы
        /// </summary>
        private static readonly ListFast<BlockBufferDistance> _listAlphaBuffer = new ListFast<BlockBufferDistance>();

        /// <summary>
        /// Дистанция до камеры, для рендера, пополняется в момент FrustumCulling
        /// </summary>
        public int Distance;

        /// <summary>
        /// Клиентский мир
        /// </summary>
        private readonly WorldClient _worldClient;
        /// <summary>
        /// Сетка чанка сплошных блоков
        /// </summary>
        private readonly MeshVoxel _meshDense;
        /// <summary>
        /// Сетка чанка альфа блоков
        /// </summary>
        private readonly MeshVoxel _meshAlpha;
        /// <summary>
        /// Список всех видимых альфа блоков
        /// Координаты в битах 0000 yyyy zzzz xxxx
        /// </summary>
        private readonly List<ushort>[] _listAlphaBlock;
        /// <summary>
        /// Количество альфа блоков в чанке
        /// </summary>
        private int _countAlpha;

        /// <summary>
        /// Соседние чанки, заполняются перед рендером
        /// </summary>
        private readonly ChunkRender[] _chunks = new ChunkRender[8];
        

        public ChunkRender(WorldClient worldClient, int chunkPosX, int chunkPosY) 
            : base(worldClient, worldClient.ChunkPr.Settings, chunkPosX, chunkPosY)
        {
            _worldClient = worldClient;
            _meshDense = new MeshVoxel(_worldClient.WorldRender.GetOpenGL());
            _meshAlpha = new MeshVoxel(_worldClient.WorldRender.GetOpenGL());

            _listAlphaBlock = new List<ushort>[NumberSections];
            for (int index = 0; index < NumberSections; index++)
            {
                _listAlphaBlock[index] = new List<ushort>();
            }
        }

        #region Modified

        /// <summary>
        /// Проверка, нужен ли рендер этого псевдо чанка
        /// </summary>
        public bool IsModifiedRender => _meshDense.IsModifiedRender;

        /// <summary>
        /// Проверка, нужен ли рендер этого псевдо чанка  альфа блоков
        /// </summary>
        public bool IsModifiedRenderAlpha => _meshAlpha.IsModifiedRender;

        /// <summary>
        /// Пометить что надо перерендерить сетку чанка
        /// </summary>
        public override void ModifiedToRender(int y)
            => _meshDense.IsModifiedRender = true;

        /// <summary>
        /// Пометить что надо перерендерить сетку чанка альфа блоков
        /// </summary>
        public bool ModifiedToRenderAlpha(int y)
        {
            if (y >= 0 && y < NumberSections && _listAlphaBlock[y].Count > 0)
            {
                _meshAlpha.IsModifiedRender = true;
                return true;
            }
            return false;
        }

        #endregion

        /// <summary>
        /// Прорисовка сплошных блоков чанка
        /// </summary>
        public void DrawDense() => _meshDense.Draw();

        /// <summary>
        /// Прорисовка альфа блоков чанка
        /// </summary>
        public void DrawAlpha() => _meshAlpha.Draw();

        public void DisposeMesh()
        {
            _meshDense.Dispose();
            _meshAlpha.Dispose();
        }

        /// <summary>
        /// Рендер чанка, сплошных и альфа блоков
        /// </summary>
        public void Render()
        {
            long timeBegin = _worldClient.Game.ElapsedTicks();

            Vector3i posPlayer = _worldClient.Game.Player.PositionAlphaBlock;
            Gi.VertexDense.Clear();
            Gi.VertexAlpha.Clear();
            _listAlphaBuffer.Clear();
            _countAlpha = 0;
            for (int i = 0; i < NumberSections; i++)
            {
                _listAlphaBlock[i].Clear();
            }

            ChunkStorage chunkStorage;
            int cbX = CurrentChunkX << 4;
            int cbZ = CurrentChunkY << 4;
            int cbY, realY, index, yb, x, z, indexY, indexYZ;
            ushort data, id;

            //BlockRenderFull blockRender = Gi.BlockRendFull;
            Gi.BlockRendFull.InitChunk(this);
            //Gi.BlockRendLiquid.InitChunk(this);
            Gi.BlockAlphaRendFull.InitChunk(this);
            
            for (cbY = 0; cbY < NumberSections; cbY++)
            {
                chunkStorage = StorageArrays[cbY];
                if (chunkStorage.Data != null && !chunkStorage.IsEmptyData())
                {
                    Gi.BlockRendFull.InitStorage(cbY);
                    Gi.BlockAlphaRendFull.InitStorage(cbY);
                    // Имекется хоть один блок
                    for (yb = 0; yb < 16; yb++)
                    {
                        realY = cbY << 4 | yb;
                        indexY = yb << 8;
                        for (z = 0; z < 16; z++)
                        {
                            indexYZ = indexY | z << 4;
                            // ~0.002
                            for (x = 0; x < 16; x++)
                            {
                                // ~0.011
                                index = indexYZ | x;
                                // ~0.012
                                data = chunkStorage.Data[index];
                                // Если блок воздуха, то пропускаем рендер сразу
                                if (data == 0) continue;
                                // ~0.020
                                // Определяем id блока
                                id = (ushort)(data & 0xFFF);
                                // ~0.021
                                // Определяем объект блока
                                Gi.Block = Blocks.BlockObjects[id];
                                // ~0.066
                                
                                if (Gi.Block.Translucent) //+0.006
                                {
                                    // Альфа
                                    Gi.Block.BlockRender.PosChunkX = x;
                                    Gi.Block.BlockRender.PosChunkY = realY;
                                    Gi.Block.BlockRender.PosChunkZ = z;
                                    if (Gi.Block.BlockRender.CheckSide())
                                    {
                                        // Определяем met блока
                                        Gi.Block.BlockRender.Met = Gi.Block.IsMetadata ? chunkStorage.Metadata[(ushort)index] : (uint)(data >> 12);
                                        Gi.Block.BlockRender.LightBlockSky = chunkStorage.LightBlock[index] << 4 | chunkStorage.LightSky[index] & 0xF;
                                        Gi.Block.BlockRender.RenderSide();

                                        if (!Gi.VertexAlpha.Empty())
                                        {
                                            _listAlphaBlock[cbY].Add((ushort)(yb << 8 | z << 4 | x));
                                            _listAlphaBuffer.Add(new BlockBufferDistance()
                                            {
                                                BufferFloat = Gi.VertexAlpha.BufferFloat.ToArray(),
                                                BufferByte = Gi.VertexAlpha.BufferByte.ToArray(),
                                                Distance = Glm.Distance(posPlayer,
                                                    new Vector3i(cbX | x, realY, cbZ | z))
                                            });
                                            Gi.VertexAlpha.Clear();
                                        }
                                    }
                                }
                                else
                                {
                                    // Сплошной
                                    // Рендер сплошных, не прозрачных блоков
                                    //if (block.IsUnique)
                                    //{
                                    //    // Уникальный блок
                                    //    blockRender = Gi.BlockRendUnique;
                                    //}
                                    //else if (block.FullBlock)
                                    //{
                                    //    // Сплошной блок
                                    //    blockRender = Gi.BlockRendFull;
                                    //}
                                    //else
                                    //{
                                    //    // Жидкость
                                    //    blockRender = Gi.BlockRendLiquid;
                                    //}

                                    Gi.Block.BlockRender.PosChunkX = x;
                                    Gi.Block.BlockRender.PosChunkY = realY;
                                    Gi.Block.BlockRender.PosChunkZ = z;
                                    if (Gi.Block.BlockRender.CheckSide())
                                    {
                                        // Определяем met блока
                                        Gi.Block.BlockRender.Met = Gi.Block.IsMetadata ? chunkStorage.Metadata[(ushort)index] : (uint)(data >> 12);
                                        Gi.Block.BlockRender.LightBlockSky = chunkStorage.LightBlock[index] << 4 | chunkStorage.LightSky[index] & 0xF;
                                        Gi.Block.BlockRender.RenderSide();
                                    }
                                }
                            }
                        }
                    }
                    if (_listAlphaBlock[cbY].Count > 0)
                    {
                        _countAlpha += _listAlphaBlock[cbY].Count;
                    }
                }
            }
            // Debug.Burden(1f);

            _meshDense.SetBuffer(Gi.VertexDense);
            _AlphaBlocksSort();

            // Для отладочной статистики
            float time = (_worldClient.Game.ElapsedTicks() - timeBegin) / (float)Ticker.TimerFrequency;
            Debug.RenderChunckTime8 = time;// (Debug.RenderChunckTime8 * 100f + time) / 101f;
            Debug.RenderChunckTime[(++Debug.dct) % 32] = time;
        }

        /// <summary>
        /// Рендер видимых альфа блоков
        /// </summary>
        public void RenderAlpha()
        {
            if (_countAlpha > 0)
            {
                long timeBegin = _worldClient.Game.ElapsedTicks();

                int cbX = CurrentChunkX << 4;
                int cbZ = CurrentChunkY << 4;

                int cbY, i, x, y, z, realY, index, count;
                ushort data, id;
                ChunkStorage chunkStorage;
                Vector3i posPlayer = _worldClient.Game.Player.PositionAlphaBlock;
                Gi.VertexAlpha.Clear();
                _listAlphaBuffer.Clear();
                Gi.BlockAlphaRendFull.InitChunk(this);

                for (cbY = 0; cbY < NumberSections; cbY++)
                {
                    count = _listAlphaBlock[cbY].Count;
                    if (count > 0)
                    {
                        chunkStorage = StorageArrays[cbY];
                        Gi.BlockAlphaRendFull.InitStorage(cbY);
                        for (i = 0; i < count; i++)
                        {
                            index = _listAlphaBlock[cbY][i];
                            x = index & 0xF;
                            z = (index >> 4) & 0xF;
                            y = index >> 8;
                            realY = cbY << 4 | y;
                            
                            index = index & 0xFFF;
                            data = chunkStorage.Data[index];
                            id = (ushort)(data & 0xFFF);
                            Gi.Block = Blocks.BlockObjects[id];
                            Gi.Block.BlockRender.PosChunkX = x;
                            Gi.Block.BlockRender.PosChunkY = realY;
                            Gi.Block.BlockRender.PosChunkZ = z;
                            if (Gi.Block.BlockRender.CheckSide())
                            {
                                // Определяем met блока
                                Gi.Block.BlockRender.Met = Gi.Block.IsMetadata ? chunkStorage.Metadata[(ushort)index] : (uint)(data >> 12);
                                Gi.Block.BlockRender.LightBlockSky = chunkStorage.LightBlock[index] << 4 | chunkStorage.LightSky[index] & 0xF;
                                Gi.Block.BlockRender.RenderSide();
                                _listAlphaBuffer.Add(new BlockBufferDistance()
                                {
                                    BufferFloat = Gi.VertexAlpha.BufferFloat.ToArray(),
                                    BufferByte = Gi.VertexAlpha.BufferByte.ToArray(),
                                    Distance = Glm.Distance(posPlayer,
                                        new Vector3i(cbX | x, realY, cbZ | z))
                                });
                                Gi.VertexAlpha.Clear();
                            }
                        }
                    }
                }
                _AlphaBlocksSort();

                float time = (_worldClient.Game.ElapsedTicks() - timeBegin) / (float)Ticker.TimerFrequency;
                Debug.RenderChunckTimeAlpha8 = (Debug.RenderChunckTimeAlpha8 * 7f + time) / 8f;
            }
        }

        /// <summary>
        /// Отсортировать альфа блоки и подготовить буфер к бинду
        /// </summary>
        private void _AlphaBlocksSort()
        {
            int count = _listAlphaBuffer.Count;
            if (count > 0)
            {
                _listAlphaBuffer.Sort();
                for (int i = count - 1; i >= 0; i--)
                {
                    Gi.VertexAlpha.AddBlockBufferDistance(_listAlphaBuffer[i]);
                }
            }
            _meshAlpha.SetBuffer(Gi.VertexAlpha);
        }

        /// <summary>
        /// Старт рендеринга
        /// </summary>
        public void StartRendering()
        {
            _meshDense.StatusRendering();
            //for (int y = 0; y < COUNT_HEIGHT; y++)
            //{
            //    isRenderingSectionDense[y] = meshSectionDense[y].isModifiedRender;
            //    meshSectionDense[y].isModifiedRender = false;
            //}
            _meshAlpha.StatusRendering();
        }

        /// <summary>
        /// Старт рендеринга только альфа
        /// </summary>
        public void StartRenderingAlpha() => _meshAlpha.StatusRendering();

        /// <summary>
        /// Занести буфер сплошных блоков чанка если это требуется
        /// </summary>
        public void BindBufferDense() => _meshDense.BindBuffer();

        /// <summary>
        /// Занести буфер альфа блоков чанка если это требуется
        /// </summary>
        public void BindBufferAlpha() => _meshAlpha.BindBuffer();

        #region BufferChunks

        /// <summary>
        /// Заполнить буфе боковых чанков
        /// </summary>
        public void UpBufferChunks()
        {
            for (int i = 0; i < 8; i++)
            {
                _chunks[i] = _worldClient.ChunkPrClient.GetChunkRender(
                    CurrentChunkX + Ce.AreaOne8X[i], CurrentChunkY + Ce.AreaOne8Y[i]
                );
            }
        }

        /// <summary>
        /// Очистить буфер соседних чанков
        /// </summary>
        public void ClearBufferChunks()
        {
            for (int i = 0; i < 8; i++) _chunks[i] = null;
        }

        /// <summary>
        /// Получить соседний чанк, где x и y -1..1
        /// </summary>
        public ChunkRender Chunk(int x, int y) => _chunks[Ce.GetAreaOne8(x, y)];

        #endregion

        #region Status

        /// <summary>
        /// Статсус возможности для рендера сплошных блоков
        /// </summary>
        public bool IsMeshDenseWait => _meshDense.Status == MeshVoxel.StatusMesh.Wait 
            || _meshDense.Status == MeshVoxel.StatusMesh.Null;
        /// <summary>
        /// Статсус возможности для рендера альфа блоков
        /// </summary>
        public bool IsMeshAlphaWait => _meshAlpha.Status == MeshVoxel.StatusMesh.Wait 
            || _meshAlpha.Status == MeshVoxel.StatusMesh.Null;
        /// <summary>
        /// Статсус связывания сетки с OpenGL для рендера сплошных блоков
        /// </summary>
        public bool IsMeshDenseBinding => _meshDense.Status == MeshVoxel.StatusMesh.Binding;
        /// <summary>
        /// Статсус связывания сетки с OpenGL для рендера альфа блоков
        /// </summary>
        public bool IsMeshAlphaBinding => _meshAlpha.Status == MeshVoxel.StatusMesh.Binding;
        /// <summary>
        /// Статсус не пустой для рендера сплошных или уникальных блоков
        /// </summary>
        public bool NotNullMeshDense => _meshDense.Status != MeshVoxel.StatusMesh.Null;
        /// <summary>
        /// Статсус не пустой для рендера альфа блоков
        /// </summary>
        public bool NotNullMeshAlpha => _meshAlpha.Status != MeshVoxel.StatusMesh.Null;
        /// <summary>
        /// Изменить статус на отмена рендеринга альфа блоков
        /// </summary>
        public void NotRenderingAlpha() => _meshAlpha.IsModifiedRender = false;

        #endregion
    }
}
