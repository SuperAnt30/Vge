using System;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Объект рендера чанка, он же клиентский чанк
    /// </summary>
    public class ChunkRender : ChunkBase, IDisposable
    {
        /// <summary>
        /// Клиентский мир
        /// </summary>
        private readonly WorldClient _worldClient;
        /// <summary>
        /// Сетка чанка сплошных блоков
        /// </summary>
        private readonly MeshVoxel _meshDense;
        /// <summary>
        /// Сетка чанков уникальных блоков
        /// </summary>
        private readonly MeshVoxel _meshUnique;
        /// <summary>
        /// Сетка чанка альфа блоков
        /// </summary>
        private readonly MeshVoxel _meshAlpha;

        /// <summary>
        /// Соседние чанки, заполняются перед рендером
        /// </summary>
        private readonly ChunkRender[] _chunks = new ChunkRender[8];

        public ChunkRender(WorldClient worldClient, int chunkPosX, int chunkPosY) 
            : base(worldClient, worldClient.ChunkPr.Settings, chunkPosX, chunkPosY)
        {
            _worldClient = worldClient;
            _meshDense = new MeshVoxel(_worldClient.WorldRender.GetOpenGL());
            _meshUnique = new MeshVoxel(_worldClient.WorldRender.GetOpenGL());
            _meshAlpha = new MeshVoxel(_worldClient.WorldRender.GetOpenGL());
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
        {
            _meshDense.IsModifiedRender = true;
            //if (y >= 0 && y < COUNT_HEIGHT) meshSectionDense[y].isModifiedRender = true;
        }

        /// <summary>
        /// Пометить что надо перерендерить сетку чанка альфа блоков
        /// </summary>
        public bool ModifiedToRenderAlpha(int y)
        {
            //if (y >= 0 && y < COUNT_HEIGHT && countSectionAlpha[y] > 0)
            //{
            //    meshAlpha.SetModifiedRender();
            //    return true;
            //}
            return false;
        }

        #endregion

        /// <summary>
        /// Прорисовка сплошных и уникальных блоков чанка
        /// </summary>
        public void DrawDenseUnique()
        {
            _meshDense.Draw();
            _meshUnique.Draw();
        }

        /// <summary>
        /// Прорисовка альфа блоков чанка
        /// </summary>
        public void DrawAlpha() => _meshAlpha.Draw();

        public void Dispose()
        {
            _meshDense.Dispose();
            _meshUnique.Dispose();
            _meshAlpha.Dispose();
        }

        /// <summary>
        /// Рендер псевдо чанка, сплошных и альфа блоков
        /// </summary>
        public void Render(bool isDense)
        {
            long timeBegin = _worldClient.Game.ElapsedTicks();

            Gi.Vertex.Clear();

            ChunkStorage chunkStorage;
            int cbY, realY, realZ, index, yb, x, z;
            ushort data, id;
            uint met;
            BlockBase block;
          //  BlockRenderFull blockRender = Gi.BlockRendFull;
            Gi.BlockRendFull.InitChunk(this);
            Gi.BlockRendUnique.InitChunk(this);
            Gi.BlockRendLiquid.InitChunk(this);

            int indexY, indexYZ;
            for (cbY = 0; cbY < NumberSections; cbY++)
            {
                chunkStorage = StorageArrays[cbY];
                if (chunkStorage.Data != null && !chunkStorage.IsEmptyData())
                {
                    // Имекется хоть один блок
                    for (yb = 0; yb < 16; yb++)
                    {
                        realY = cbY << 4 | yb;
                        indexY = yb << 8;
                        for (z = 0; z < 16; z++)
                        {
                            indexYZ = indexY | z << 4;
                            for (x = 0; x < 16; x++)
                            {
                                index = indexYZ | x;
                                //index = yb << 8 | z << 4 | x;
                                data = chunkStorage.Data[index];
                                // Если блок воздуха, то пропускаем рендер сразу
                                if (data == 0 || data == 4096) continue;
                                // 0.125 - 0.145
                                
                                // Определяем id блока
                                id = (ushort)(data & 0xFFF);
                                // 0.135 - 0.150

                                // Определяем met блока
                                met = Blocks.BlocksMetadata[id]
                                    ? chunkStorage.Metadata[(ushort)index] : (uint)(data >> 12);
                                // 0.180 - 0.190
                                
                                // Определяем объект блока
                                block = Blocks.BlockObjects[id];
                                // 0.195 - 0.225

                                if (block.Translucent)
                                {
                                    // Альфа
                                }
                                else if (isDense)
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
                                    // 0.230 - 0.250

                                    block.BlockRender.BlockSt.Id = id;

                                    block.BlockRender.Met = block.BlockRender.BlockSt.Met = met;
                                    block.BlockRender.BlockSt.LightBlock = chunkStorage.LightBlock[index];
                                    block.BlockRender.BlockSt.LightSky = chunkStorage.LightSky[index];
                                    block.BlockRender.PosChunkX = x;
                                    block.BlockRender.PosChunkY = realY;
                                    block.BlockRender.PosChunkZ = z;
                                    block.BlockRender.Block = block;

                                    // 0.450 - 0.550
                                    //continue;
                                    block.BlockRender.RenderMesh();
                                }
                                //_vertex.AddVertex(x, realY, z, .046875f, 0, 255, 255, 255, 255);
                                //_vertex.AddVertex(x + 1, realY, z, .0625f, 0, 255, 255, 255, 255);
                                //_vertex.AddVertex(x, realY, z + 1, .046875f, .015625f, 255, 255, 255, 255);
                                //_vertex.AddVertex(x + 1, realY, z + 1, .0625f, .015625f, 255, 255, 255, 255);
                            }
                        }
                    }
                }
            }
                /*
                if (_worldClient.Game.Player.IdWorld == 0)
                {
                    _vertex.AddVertex(0, 0, 0, 0, 0, 0, (byte)(CurrentChunkX % 3 == 0 ? 255 : 0), 0, 255);
                }
                else
                {
                    _vertex.AddVertex(0, 0, 0, 0, 0, 255, 255, 255, 255);
                }
                _vertex.AddVertex(16, 0, 0, .1f, 0, 255, 255, 255, 255);
                _vertex.AddVertex(0, 0, 16, 0, .1f, 0, (byte)(CurrentChunkY % 3 == 0 ? 255 : 0), (byte)(CurrentChunkY % 3 == 0 ? 255 : 0), 255);
                _vertex.AddVertex(16, 0, 16, .1f, .1f, 255, 255, 255, 255);

                for (int j = 1; j < NumberSections; j++)
                {
                    int i = j * 16;
                    _vertex.AddVertex(1, i, 0, 0, 0, 255, 255, 255, 255);
                    _vertex.AddVertex(15, i, 0, .1f, 0, 255, 255, 255, 255);
                    _vertex.AddVertex(1, i, 16, 0, .1f, 255, 255, 255, 255);
                    _vertex.AddVertex(15, i, 16, .1f, .1f, 255, 255, 255, 255);
                }
                */
                // Debug.Burden(1f);
                // _meshDense.SetBuffer(_bufferFloat.ToArray(), _buffer.ToArray());

            _meshDense.SetBuffer(Gi.Vertex);

            // Для отладочной статистики
            float time = (_worldClient.Game.ElapsedTicks() - timeBegin) / (float)Ticker.TimerFrequency;
            if (isDense)
            {
                Debug.RenderChunckTime8 = (Debug.RenderChunckTime8 * 7f + time) / 8f;
            }
            else
            {
                Debug.RenderChunckTimeAlpha8 = (Debug.RenderChunckTimeAlpha8 * 7f + time) / 8f;
            }
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
        /// Занести буфер сплошных блоков псевдо чанка если это требуется
        /// </summary>
        public void BindBufferDense()
        {
            _meshUnique.BindBuffer();
            _meshDense.BindBuffer();
        }
        /// <summary>
        /// Занести буфер альфа блоков псевдо чанка если это требуется
        /// </summary>
        public void BindBufferAlpha() => _meshAlpha.BindBuffer();

        #region BufferChunks

        /// <summary>
        /// Заполнить буфе боковых чанков
        /// </summary>
        public void UpBufferChunks()
        {
            // TODO::2024-11-01 надо вынести за пределы потока
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
        public bool NotNullMeshDenseOrUnique => _meshDense.Status != MeshVoxel.StatusMesh.Null
            || _meshUnique.Status != MeshVoxel.StatusMesh.Null;
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
