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
    public class ChunkRender : ChunkBase
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

        public override void Dispose()
        {
            _meshDense.Dispose();
            _meshUnique.Dispose();
            _meshAlpha.Dispose();
            base.Dispose();
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
            //BlockBase block;
            //BlockRenderFull blockRender = Gi.BlockRendFull;
            Gi.BlockRendFull.InitChunk(this);
            //Gi.BlockRendUnique.InitChunk(this);
            //Gi.BlockRendLiquid.InitChunk(this);

            int indexY, indexYZ;
            for (cbY = 0; cbY < NumberSections; cbY++)
            {
                chunkStorage = StorageArrays[cbY];
                if (chunkStorage.Data != null && !chunkStorage.IsEmptyData())
                {
                    Gi.BlockRendFull.InitStorage(cbY);
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
                                }
                                else if (isDense) // Теряю время на ~0.006
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

                                    // ~0.078
                                    Gi.Block.BlockRender.PosChunkX = x;
                                    Gi.Block.BlockRender.PosChunkY = realY;
                                    Gi.Block.BlockRender.PosChunkZ = z;

                                    // ~0.11
                                    //continue;
                                    if (Gi.Block.BlockRender.CheckSide()) // +0.75
                                    {
                                        // ~0.85
                                        //continue;
                                        // Определяем met блока
                                        Gi.Block.BlockRender.Met = Gi.Block.IsMetadata ? chunkStorage.Metadata[(ushort)index] : (uint)(data >> 12);
                                        Gi.Block.BlockRender.LightBlockSky = chunkStorage.LightBlock[index] << 4 | chunkStorage.LightSky[index] & 0xF;
                                        // ~0.87
                                        //continue;
                                        Gi.Block.BlockRender.RenderSide(); // +0.40
                                    }

                                    // ~1.25
                                    //continue;
                                }
                            }
                        }
                    }
                }
            }
            // Debug.Burden(1f);

            _meshDense.SetBuffer(Gi.Vertex);

            // Для отладочной статистики
            float time = (_worldClient.Game.ElapsedTicks() - timeBegin) / (float)Ticker.TimerFrequency;
            if (isDense)
            {
                Debug.RenderChunckTime8 = time;// (Debug.RenderChunckTime8 * 100f + time) / 101f;
                Debug.RenderChunckTime[(++Debug.dct) % 32] = time;
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
