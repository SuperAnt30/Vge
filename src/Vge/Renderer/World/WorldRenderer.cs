using System.Threading;
using Vge.Games;
using Vge.Renderer.Shaders;
using Vge.Util;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Объект рендера мира
    /// </summary>
    public class WorldRenderer : WarpRenderer
    {
        /// <summary>
        /// Буфер для склейки рендера. покуда тест в будущем для чанка
        /// </summary>
        public readonly VertexBuffer Vertex = new VertexBuffer(1000);

        /// <summary>
        /// Флаг потока рендера чанков
        /// </summary>
        private bool _flagRenderLoopRunning = false;
        /// <summary>
        /// Массив очередей чанков для рендера
        /// </summary>
        private readonly DoubleList<ChunkRender> _renderQueues = new DoubleList<ChunkRender>();

        public WorldRenderer(GameBase game) : base(game)
        {

        }

        #region Поток рендера

        /// <summary>
        /// Поток рендера чанков
        /// </summary>
        private void _RenderLoop()
        {
            while (_flagRenderLoopRunning)
            {
                _RenderQueues(true, _renderQueues);
                //RenderQueues(false, renderAlphaQueues);
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Запуск рендера чанка из очередей в отдельном потоке
        /// </summary>
        /// <param name="isDense">Флаг сплошных блоков</param>
        /// <param name="list">Список очередей</param>
        private void _RenderQueues(bool isDense, DoubleList<ChunkRender> list)
        {
            int count, i;
            ChunkRender chunkRender;
            list.Step();
            count = list.CountBackward;
            for (i = 0; i < count; i++)
            {
                chunkRender = list.GetNext();
                chunkRender.UpBufferChunks();
                chunkRender.Render(isDense);
                chunkRender.ClearBufferChunks();
                if (!_flagRenderLoopRunning) break;
            }
        }

        #endregion

        /// <summary>
        /// Запускается мир, возможно смена миров
        /// </summary>
        public void Starting()
        {
            // Запускаем отдельный поток для рендера
            _flagRenderLoopRunning = true;
            Thread myThread = new Thread(_RenderLoop) { Name = "WorldRender" };
            myThread.Start();
        }

        /// <summary>
        /// Останавливаем мир, возможно смена миров
        /// </summary>
        public void Stoping() 
        {
            _flagRenderLoopRunning = false;
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            _game.Render.TestRun();

            // Биндим чанки вокселей и готовим массив чанков
            _BindChunkVoxel();

            // Рисуем воксели сплошных блоков VBO
            _DrawVoxelDense(timeIndex);

            
        }

        /// <summary>
        /// Связывание чанки вокселей и готовим массив чанков
        /// </summary>
        private void _BindChunkVoxel()
        {
            int count = _game.Player.FrustumCulling.Count;
            ChunkRender chunkRender;
            for (int i = 0; i < count; i++)
            {
                chunkRender = _game.Player.FrustumCulling[i];
                if (chunkRender == null || !chunkRender.IsChunkPresent) continue;
                if (chunkRender.IsModifiedRender && _renderQueues.CountForward < 8)
                {
                    chunkRender.StartRendering();
                    _renderQueues.Add(chunkRender);
                }

                // Занести буфер сплошных блоков псевдо чанка если это требуется
                if (chunkRender.IsMeshDenseBinding) chunkRender.BindBufferDense();
            }
        }

        /// <summary>
        /// Рисуем воксели сплошных блоков
        /// </summary>
        private void _DrawVoxelDense(float timeIndex)
        {
            _game.Render.ShaderBindVoxels(_game.Player.View, _game.Player.OverviewChunk * 16, 1, 1, 1, 15);

            int count = _game.Player.FrustumCulling.Count;
            int px = _game.Player.Position.ChunkPositionX;
            int pz = _game.Player.Position.ChunkPositionZ;
            int bx = px << 4;
            int bz = pz << 4;

            float fx = _game.Player.Position.X - bx;
            float fz = _game.Player.Position.Z - bz;
            ChunkRender chunkRender;
            for (int i = 0; i < count; i++)
            {
                chunkRender = _game.Player.FrustumCulling[i];
                if (chunkRender.NotNullMeshDense)
                {
                    _VoxelsShaderChunk(_game.Render.ShVoxel, chunkRender.CurrentChunkX, chunkRender.CurrentChunkY);
                    chunkRender.DrawDense();
                }
            }
        }

        /// <summary>
        /// Доп параметр к шейдеру чанка
        /// </summary>
        private void _VoxelsShaderChunk(ShaderVoxel shader, int chunkX, int chunkY)
        {
            shader.SetUniform3(_game.GetOpenGL(), "pos",
                (chunkX << 4) - _game.Player.Position.X,
                -_game.Player.Position.Y,
                (chunkY << 4) - _game.Player.Position.Z
            );
        }

        public override void Dispose()
        {
            Vertex.Dispose();
        }
    }
}
