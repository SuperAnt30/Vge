using System;
using Vge.Util;
using Vge.World;
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
        /// Буфер для склейки рендера
        /// </summary>
        private readonly VertexBuffer _vertex;

        /// <summary>
        /// Соседние чанки, заполняются перед рендером
        /// </summary>
        private readonly ChunkRender[] _chunks = new ChunkRender[8];

        public ChunkRender(WorldClient worldClient, int chunkPosX, int chunkPosY) 
            : base(worldClient, chunkPosX, chunkPosY)
        {
            _worldClient = worldClient;
            _vertex = Gi.Vertex;
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
        public/* override */void ModifiedToRender(int y)
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

            _vertex.Clear();
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

            for (int i = 64; i < 128; i++)
            {
                _vertex.AddVertex(1, i, 0, 0, 0, 255, 255, 255, 255);
                _vertex.AddVertex(15, i, 0, .1f, 0, 255, 255, 255, 255);
                _vertex.AddVertex(1, i, 16, 0, .1f, 255, 255, 255, 255);
                _vertex.AddVertex(15, i, 16, .1f, .1f, 255, 255, 255, 255);
            }
            Debug.Burden(1f);
            // _meshDense.SetBuffer(_bufferFloat.ToArray(), _buffer.ToArray());

            _meshDense.SetBuffer(_vertex);

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
            //for (int i = 0; i < 8; i++)
            //{
            //    _chunks[i] = World.ChunkPr.GetChunk(Position + MvkStatic.AreaOne8[i]) as ChunkRender;
            //}
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
        public ChunkRender Chunk(int x, int y) => null;// chunks[MvkStatic.GetAreaOne8(x, y)];

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
