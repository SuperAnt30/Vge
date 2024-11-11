using System.Diagnostics;
using System.Threading;
using Vge.Games;
using Vge.Renderer.Shaders;
using Vge.Util;
using WinGL.OpenGL;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Объект рендера мира
    /// </summary>
    public class WorldRenderer : WarpRenderer
    {
        /// <summary>
        /// Объект OpenGL для элемента управления
        /// </summary>
        private readonly GL gl;

        /// <summary>
        /// Флаг потока рендера чанков
        /// </summary>
        private bool _flagRenderLoopRunning = false;
        /// <summary>
        /// Массив очередей чанков для рендера
        /// </summary>
        private readonly DoubleList<ChunkRender> _renderQueues = new DoubleList<ChunkRender>();
        /// <summary>
        /// Массив очередей чанков для рендера альфа
        /// </summary>
        private readonly DoubleList<ChunkRender> _renderAlphaQueues = new DoubleList<ChunkRender>();
        /// <summary>
        /// Метод чанков для прорисовки
        /// </summary>
        private readonly ArrayFast<ChunkRender> _arrayChunkRender;
        /// <summary>
        /// Объект-событие
        /// </summary>
        private readonly AutoResetEvent _waitHandler = new AutoResetEvent(true);

        /// <summary>
        /// Желаемый размер партии рендера чанков
        /// </summary>
        private byte _desiredBatchSize = Ce.MinDesiredBatchSize;
        /// <summary>
        /// Время мс на рендера чанков
        /// </summary>
        private int _batchChunksTime;
        /// <summary>
        /// Обзор в блоках
        /// </summary>
        private int _overviewBlock = 32;

        public WorldRenderer(GameBase game) : base(game)
        {
            gl = GetOpenGL();
            _arrayChunkRender = new ArrayFast<ChunkRender>(Ce.OverviewCircles.Length);
        }

        /// <summary>
        /// Изменён обзор чанков
        /// </summary>
        public void ModifyOverviewChunk()
        { 
            _arrayChunkRender.Resize(Ce.OverviewCircles.Length);
            _overviewBlock = _game.Player.OverviewChunk * 16;
        }

        #region Поток рендера

        /// <summary>
        /// Поток рендера чанков
        /// </summary>
        private void _RenderLoop()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            long timeBegin;
            int quantity;
            while (_flagRenderLoopRunning)
            {
                timeBegin = stopwatch.ElapsedMilliseconds;
                quantity = _renderQueues.CountForward + _renderAlphaQueues.CountForward;

                if (quantity > 0)
                {
                    quantity = _RenderQueues(true, _renderQueues)
                        + _RenderQueues(false, _renderAlphaQueues);

                    //"WR dbs:" + _desiredBatchSize + "|" + _batchChunksTime + "mc";
                    _batchChunksTime = (int)(stopwatch.ElapsedMilliseconds - timeBegin);
                    _desiredBatchSize = Sundry.RecommendedQuantityBatch(_batchChunksTime, 
                        quantity, _desiredBatchSize, Ce.MaxDesiredBatchSize, Ce.TickTime);
                }
                // Ожидаем сигнала
                _waitHandler.WaitOne();
            }
        }

        /// <summary>
        /// Запуск рендера чанка из очередей в отдельном потоке
        /// </summary>
        /// <param name="isDense">Флаг сплошных блоков</param>
        /// <param name="list">Список очередей</param>
        private int _RenderQueues(bool isDense, DoubleList<ChunkRender> list)
        {
            int count, i;
            ChunkRender chunkRender;
            list.Step();
            count = list.CountBackward;
            for (i = 0; i < count; i++)
            {
                chunkRender = list.GetNext();
                chunkRender.Render(isDense);
                chunkRender.ClearBufferChunks();
                if (!_flagRenderLoopRunning) break;
            }

            return count;
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
            // Сигнализируем, что waitHandler в сигнальном состоянии
            _waitHandler.Set();
        }

        /// <summary>
        /// Такт выполнения
        /// </summary>
        public void Update()
        {
            // Биндим чанки вокселей и готовим массив чанков
            // В игровом такте, для подсчёта чтоб максимально можно было нагрузить CPU
            _BindChunkVoxel();
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            _game.Render.DrawWorldBegin();

            // Обновить кадр основного игрока, камера и прочее
            _game.Player.UpdateFrame(timeIndex);
            
            // Небо
            //DrawSky(timeIndex);

            // Рисуем воксели сплошных и уникальных блоков
            _DrawVoxel();

            // Сущности
            //DrawEntities(timeIndex);

            // Прорисовка вид не с руки, а видим себя

            // Облака
            //DrawClouds(timeIndex);

            // Рисуем воксели альфа
            _DrawVoxelAlpha();

            // Прорисовка руки


        }

        /// <summary>
        /// Связывание чанки вокселей и готовим массив чанков
        /// </summary>
        private void _BindChunkVoxel()
        {
            _arrayChunkRender.Clear();
            int count = _game.Player.FrustumCulling.Count;
            ChunkRender chunkRender;
            int batchCount = 0;
            for (int i = 0; i < count; i++)
            {
                chunkRender = _game.Player.FrustumCulling[i];
                if (chunkRender != null && chunkRender.IsChunkPresent)
                {
                    // Можем ли мы в этом тике пополнять партию
                    if (batchCount < _desiredBatchSize)
                    {
                        // Проверяем надо ли рендер для чанка, и возможно ли по времени
                        if (chunkRender.IsModifiedRender)
                        {
                            // Проверяем занят ли чанк уже рендером
                            if (chunkRender.IsMeshDenseWait)// && chunkRender.IsMeshAlphaWait)
                            {
                                // Обновление рендера псевдочанка
                                Debug.CountUpdateChunck++;
                                batchCount++;
                                chunkRender.StartRendering();
                                chunkRender.UpBufferChunks();
                                _renderQueues.Add(chunkRender);
                            }
                        }
                        // Проверяем надо ли рендер для псевдо чанка, и возможно ли по времени
                        else if (chunkRender.IsModifiedRenderAlpha)
                        {
                            // Проверяем занят ли чанк уже рендером
                            if (chunkRender.IsMeshDenseWait && chunkRender.IsMeshAlphaWait)
                            {
                                // Обновление рендера псевдочанка
                                Debug.CountUpdateChunckAlpha++;
                                batchCount++;
                                chunkRender.StartRenderingAlpha();
                                _renderAlphaQueues.Add(chunkRender);
                            }
                        }
                    }

                    // Занести буфер сплошных блоков псевдо чанка если это требуется
                    if (chunkRender.IsMeshDenseBinding) chunkRender.BindBufferDense();
                    // Занести буфер альфа блоков псевдо чанка если это требуется
                    if (chunkRender.IsMeshAlphaBinding) chunkRender.BindBufferAlpha();

                    _arrayChunkRender.Add(chunkRender);
                }
            }
            if (batchCount > 0)
            {
                // Сигнализируем, что waitHandler в сигнальном состоянии
                _waitHandler.Set();
            }
        }

        /// <summary>
        /// Рисуем воксели сплошных и уникальных блоков
        /// </summary>
        private void _DrawVoxel()
        {
            if (Debug.IsDrawVoxelLine)
            {
                gl.PolygonMode(GL.GL_FRONT_AND_BACK, GL.GL_LINE);
                gl.Disable(GL.GL_CULL_FACE);
            }
            else
            {
                gl.Enable(GL.GL_CULL_FACE);
                gl.PolygonMode(GL.GL_FRONT_AND_BACK, GL.GL_FILL);
            }

            // Биндим шейдор для вокселей
            _game.Render.ShaderBindVoxels(_game.Player.View,
                _overviewBlock, 1, 1, 1, 15);

            int count = _arrayChunkRender.Count;
            ChunkRender chunkRender;
            for (int i = 0; i < count; i++)
            {
                chunkRender = _arrayChunkRender[i];
                /// Прорисовка сплошных блоков чанка
                if (chunkRender.NotNullMeshDenseOrUnique)
                {
                    _VoxelsShaderChunk(_game.Render.ShVoxel, 
                        chunkRender.CurrentChunkX, chunkRender.CurrentChunkY);
                    chunkRender.DrawDenseUnique();
                }
            }

            if (Debug.IsDrawVoxelLine)
            {
                // Дебаг должен прорисовать текстуру по этому сетка тут не уместна
                gl.Enable(GL.GL_CULL_FACE);
                gl.PolygonMode(GL.GL_FRONT_AND_BACK, GL.GL_FILL);
            }
        }

        /// <summary>
        /// Прорисовка вокселей альфа цвета
        /// </summary>
        private void _DrawVoxelAlpha()
        {
            // Биндим шейдор для вокселей
            _game.Render.ShaderBindVoxels(_game.Player.View,
                _overviewBlock, 1, 1, 1, 15);

            int count = _arrayChunkRender.Count - 1;
            ChunkRender chunkRender;

            // Пробегаем по всем чанкам которые видим FrustumCulling
            for (int i = count; i >= 0; i--)
            {
                chunkRender = _arrayChunkRender[i];
                // Прорисовка альфа блоков псевдо чанка
                if (chunkRender.NotNullMeshAlpha)
                {
                    _VoxelsShaderChunk(_game.Render.ShVoxel,
                        chunkRender.CurrentChunkX, chunkRender.CurrentChunkY);
                    chunkRender.DrawAlpha();
                }
            }
        }

        /// <summary>
        /// Доп параметр к шейдеру чанка
        /// </summary>
        private void _VoxelsShaderChunk(ShaderVoxel shader, int chunkX, int chunkY)
        {
            int x = chunkX << 4;
            int z = chunkY << 4;

            shader.SetUniform3(_game.GetOpenGL(), "pos",
                x - _game.Player.PositionFrame.X,
                -_game.Player.PositionFrame.Y,
                z - _game.Player.PositionFrame.Z
            );

            shader.SetUniform3(_game.GetOpenGL(), "camera",
                _game.Player.PositionFrame.X - x,
                _game.Player.PositionFrame.Y,
                _game.Player.PositionFrame.Z - z
            );
        }

        public override void Dispose()
        {

        }

        public override string ToString()
            => "WR dbs:" + _desiredBatchSize + "|" + _batchChunksTime + "mc";
    }
}
