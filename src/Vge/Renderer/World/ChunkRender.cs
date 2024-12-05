using System.Collections.Generic;
using Vge.Util;
using Vge.World;
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

        /// <summary>
        /// Буфер сетки секций чанков сплошных блоков
        /// </summary>
        private readonly ChunkSectionBuffer[] _sectionsBuffer;
        /// <summary>
        /// Массив какие секции сплошных чанков надо рендерить, для потока где идёт рендер
        /// Двойной флаг нужен, для того, что рендер идёт в другом потоке! (первый флаг в _sectionsBuffer.IsModifiedRender)
        /// </summary>
        private readonly bool[] _isRenderingSection;


        public ChunkRender(WorldClient worldClient, int chunkPosX, int chunkPosY) 
            : base(worldClient, worldClient.ChunkPr.Settings, chunkPosX, chunkPosY)
        {
            _worldClient = worldClient;
            _meshDense = new MeshVoxel(_worldClient.WorldRender.GetOpenGL());
            _meshAlpha = new MeshVoxel(_worldClient.WorldRender.GetOpenGL());

            _listAlphaBlock = new List<ushort>[NumberSections];
            _sectionsBuffer = new ChunkSectionBuffer[NumberSections];
            _isRenderingSection = new bool[NumberSections];
            for (int index = 0; index < NumberSections; index++)
            {
                _sectionsBuffer[index] = new ChunkSectionBuffer();
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
        {
            _meshDense.IsModifiedRender = true;
            if (y >= 0 && y < NumberSections)
            {
                _sectionsBuffer[y].IsModifiedRender = true;
            }
        }

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
            for (int y = 0; y < NumberSections; y++)
            {
                _isRenderingSection[y] = false;
            }
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

            ChunkStorage chunkStorage;
            int cbX = CurrentChunkX << 4;
            int cbZ = CurrentChunkY << 4;
            int cbY, realY, index, yb, x, z, indexY, indexYZ;
            ushort data, id;

            for (cbY = 0; cbY < NumberSections; cbY++)
            {
                if (_sectionsBuffer[cbY].BufferFloat == null)
                {
                    _sectionsBuffer[cbY].BufferFloat = new float[0];
                    _sectionsBuffer[cbY].BufferByte = new byte[0];
                }
            }

            Gi.BlockRendFull.InitChunk(this);
            Gi.BlockLiquidRendFull.InitChunk(this);
            Gi.BlockAlphaRendFull.InitChunk(this);
            Gi.BlockLiquidAlphaRendFull.InitChunk(this);

            for (cbY = 0; cbY < NumberSections; cbY++)
            {
                if (_isRenderingSection[cbY])
                {
                    _listAlphaBlock[cbY].Clear();
                    chunkStorage = StorageArrays[cbY];
                    if (chunkStorage.Data != null && !chunkStorage.IsEmptyData())
                    {
                        Gi.BlockRendFull.InitStorage(cbY);
                        Gi.BlockLiquidRendFull.InitStorage(cbY);
                        Gi.BlockAlphaRendFull.InitStorage(cbY);
                        Gi.BlockLiquidAlphaRendFull.InitStorage(cbY);
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
                                    data = chunkStorage.Data[index];
                                    // Если блок воздуха, то пропускаем рендер сразу
                                    if (data == 0) continue;
                                    // Определяем id блока
                                    id = (ushort)(data & 0xFFF);
                                    // Определяем объект блока
                                    Gi.Block = Ce.Blocks.BlockObjects[id];

                                    if (Gi.Block.Alpha)
                                    {
                                        // Альфа
                                        Gi.Block.BlockRender.PosChunkX = x;
                                        Gi.Block.BlockRender.PosChunkY = realY;
                                        Gi.Block.BlockRender.PosChunkZ = z;
                                        // Определяем met блока
                                        Gi.Block.BlockRender.Met = Gi.Block.IsMetadata ? chunkStorage.Metadata[(ushort)index] : (uint)(data >> 12);

                                        if (Gi.Block.BlockRender.CheckSide())
                                        {
                                            // Имеется видимый альфа блок, заносим в буфер для отельного рендера альфы
                                            _listAlphaBlock[cbY].Add((ushort)(yb << 8 | z << 4 | x));
                                        }
                                    }
                                    else
                                    {
                                        Gi.Block.BlockRender.PosChunkX = x;
                                        Gi.Block.BlockRender.PosChunkY = realY;
                                        Gi.Block.BlockRender.PosChunkZ = z;
                                        Gi.Block.BlockRender.Met = Gi.Block.IsMetadata ? chunkStorage.Metadata[(ushort)index] : (uint)(data >> 12);
                                        if (Gi.Block.BlockRender.CheckSide())
                                        {
                                            // Определяем met блока
                                            Gi.Block.BlockRender.LightBlockSky = chunkStorage.LightBlock[index] << 4 | chunkStorage.LightSky[index] & 0xF;
                                            Gi.Block.BlockRender.RenderSide();
                                        }
                                    }
                                }
                            }
                        }
                        _sectionsBuffer[cbY].BufferFloat = Gi.VertexDense.BufferFloat.ToArray();
                        _sectionsBuffer[cbY].BufferByte = Gi.VertexDense.BufferByte.ToArray();
                        Gi.VertexDense.Clear();
                    }
                    else
                    {
                        // Нет блоков, все блоки воздуха очищаем буфера
                        if (_sectionsBuffer[cbY].BufferFloat.Length != 0)
                        {
                            _sectionsBuffer[cbY].BufferFloat = new float[0];
                            _sectionsBuffer[cbY].BufferByte = new byte[0];
                        }
                    }
                    _isRenderingSection[cbY] = false;
                }
            }
            // Пересчитываем количество альфа блоков
            _countAlpha = 0;
            // Клеим секции
            for (cbY = 0; cbY < NumberSections; cbY++)
            {
                _countAlpha += _listAlphaBlock[cbY].Count;
                if (_sectionsBuffer[cbY].BufferFloat.Length != 0)
                {
                    Gi.VertexDense.BufferFloat.AddRange(_sectionsBuffer[cbY].BufferFloat);
                    Gi.VertexDense.BufferByte.AddRange(_sectionsBuffer[cbY].BufferByte);
                }
            }
            _meshDense.SetBuffer(Gi.VertexDense);

            // Для отладочной статистики
            float time = (_worldClient.Game.ElapsedTicks() - timeBegin) / (float)Ticker.TimerFrequency;
            Debug.RenderChunckTime8 = time;// (Debug.RenderChunckTime8 * 100f + time) / 101f;
            Debug.RenderChunckTime[(++Debug.dct) % 32] = time;

            // Если был рендер сплошных блоков, обязательно надо сделать рендер альфа блоков
            RenderAlpha();
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
                Gi.BlockLiquidAlphaRendFull.InitChunk(this);

                for (cbY = 0; cbY < NumberSections; cbY++)
                {
                    count = _listAlphaBlock[cbY].Count;
                    if (count > 0)
                    {
                        chunkStorage = StorageArrays[cbY];
                        Gi.BlockAlphaRendFull.InitStorage(cbY);
                        Gi.BlockLiquidAlphaRendFull.InitStorage(cbY);
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
                            Gi.Block = Ce.Blocks.BlockObjects[id];
                            Gi.Block.BlockRender.PosChunkX = x;
                            Gi.Block.BlockRender.PosChunkY = realY;
                            Gi.Block.BlockRender.PosChunkZ = z;
                            // Определяем met блока
                            Gi.Block.BlockRender.Met = Gi.Block.IsMetadata ? chunkStorage.Metadata[(ushort)index] : (uint)(data >> 12);

                            if (Gi.Block.BlockRender.CheckSide())
                            {
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
            for (int y = 0; y < NumberSections; y++)
            {
                _isRenderingSection[y] = _sectionsBuffer[y].IsModifiedRender;
                _sectionsBuffer[y].IsModifiedRender = false;
            }
            _meshDense.StatusRendering();
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
