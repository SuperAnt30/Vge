using System;
using System.Diagnostics;
using System.Threading;
using Vge.Games;
using Vge.Renderer.World.Entity;
using Vge.Util;
using Vge.World.Block;
using WinGL.OpenGL;
using WinGL.Util;

namespace Vge.Renderer.World
{
    /// <summary>
    /// Объект рендера мира, блоки, сущности
    /// </summary>
    public class WorldRenderer : WarpRenderer
    {
        /// <summary>
        /// Сколько чанков в обзоре, для карты теней, квадрат 5*5 минус углы.
        /// </summary>
        public const int CountChunkShadowMap = 21;
        /// <summary>
        /// Объект рендера всех сущностей
        /// </summary>
        public readonly EntitiesRenderer Entities;

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
        /// Рендер курсоров
        /// </summary>
        private readonly CursorRender _cursorRender;

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

        /// <summary>
        /// Буфер карты теней
        /// </summary>
        private uint _shadowMapFbo;
        /// <summary>
        /// Текстура карты теней
        /// </summary>
        private uint _shadowMapTexture;
        /// <summary>
        /// Сетка для отладки карты теней
        /// </summary>
        private MeshQuad _meshShadowMap;

        public WorldRenderer(GameBase game) : base(game)
        {
            gl = GetOpenGL();
            _arrayChunkRender = new ArrayFast<ChunkRender>(Ce.OverviewCircles.Length);
            _cursorRender = new CursorRender(game.Player, this);
            Entities = new EntitiesRenderer(game, _arrayChunkRender);
        }

        /// <summary>
        /// Игра запущена, всё загружено из библиотек
        /// </summary>
        public void GameStarting()
        {
            Entities.GameStarting();

            // Активация текстура и буфер карты теней
            uint[] id = new uint[1];
            gl.GenFramebuffers(1, id);
            _shadowMapFbo = id[0];

            gl.GenTextures(1, id);
            _shadowMapTexture = id[0];

            gl.ActiveTexture(GL.GL_TEXTURE0 + (uint)Gi.ActiveTextureShadowMap);
            gl.BindTexture(GL.GL_TEXTURE_2D, _shadowMapTexture);

            gl.TexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_DEPTH_COMPONENT, 1024, 1024, 0, GL.GL_DEPTH_COMPONENT, GL.GL_FLOAT, IntPtr.Zero);
            gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_NEAREST);
            gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_NEAREST);
            gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_REPEAT);
            gl.TexParameter(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_REPEAT);
            gl.BindTexture(GL.GL_TEXTURE_2D, 0);

            gl.BindFramebuffer(GL.GL_FRAMEBUFFER, _shadowMapFbo);
            gl.FramebufferTexture2D(GL.GL_FRAMEBUFFER, GL.GL_DEPTH_ATTACHMENT, GL.GL_TEXTURE_2D, _shadowMapTexture, 0);
            gl.DrawBuffer(GL.GL_NONE);
            gl.ReadBuffer(GL.GL_NONE);
            gl.BindFramebuffer(GL.GL_FRAMEBUFFER, 0);

            // Для блоков теней
            Render.ShVoxelShadowMap.Bind();
            // Активация текстуры атласа с размытостью (с Mipmap)
            gl.Uniform1(Render.ShVoxelShadowMap.GetUniformLocation("atlas_blurry"),
                Gi.ActiveTextureAatlasBlurry);
            // Активация текстуры атласа с резкостью (без Mipmap)
            gl.Uniform1(Render.ShVoxelShadowMap.GetUniformLocation("atlas_sharpness"),
                Gi.ActiveTextureAatlasSharpness);

            // Для сущностей теней
            Render.ShEntityShadowMap.Bind();
            gl.Uniform1(Render.ShEntityShadowMap.GetUniformLocation("sampler_small"),
                Gi.ActiveTextureSamplerSmall);
            gl.Uniform1(Render.ShEntityShadowMap.GetUniformLocation("sampler_big"),
                Gi.ActiveTextureSamplerBig);

            // ---------

            // Для блоков
            Render.ShVoxel.Bind();
            // Активация текстуры атласа с размытостью (с Mipmap)
            gl.Uniform1(Render.ShVoxel.GetUniformLocation("atlas_blurry"),
                Gi.ActiveTextureAatlasBlurry);
            // Активация текстуры атласа с резкостью (без Mipmap)
            gl.Uniform1(Render.ShVoxel.GetUniformLocation("atlas_sharpness"),
                Gi.ActiveTextureAatlasSharpness);
            // Активация текстуры карты света
            gl.Uniform1(Render.ShVoxel.GetUniformLocation("light_map"),
                Gi.ActiveTextureLightMap);
            // Активация текстуры карты теней
            gl.Uniform1(Render.ShVoxel.GetUniformLocation("shadow_map"),
                Gi.ActiveTextureShadowMap);

            // Для сущностей
            Render.ShEntity.Bind();
            gl.Uniform1(Render.ShEntity.GetUniformLocation("sampler_small"),
                Gi.ActiveTextureSamplerSmall);
            gl.Uniform1(Render.ShEntity.GetUniformLocation("sampler_big"),
                Gi.ActiveTextureSamplerBig);
            // Активация текстуры карты света
            gl.Uniform1(Render.ShEntity.GetUniformLocation("light_map"),
                Gi.ActiveTextureLightMap);
            // Активация текстуры карты теней
            gl.Uniform1(Render.ShEntity.GetUniformLocation("shadow_map"),
                Gi.ActiveTextureShadowMap);

            // Для отладки карта теней
            Render.ShShadowMap.Bind();
            gl.BindTexture(GL.GL_TEXTURE_2D, _shadowMapTexture);
            // Активация текстуры карты теней
            gl.Uniform1(Render.ShShadowMap.GetUniformLocation("shadow_map"),
                Gi.ActiveTextureShadowMap);
            _meshShadowMap = new MeshQuad(gl, 0, 0, 1024, 1024);
        }

        /// <summary>
        /// Изменён обзор чанков
        /// </summary>
        public void ModifyOverviewChunk()
        { 
            _arrayChunkRender.Resize(Ce.OverviewCircles.Length);
            _overviewBlock = _game.Player.OverviewChunk * 16;
        }

        /// <summary>
        /// Перезапуск мира
        /// </summary>
        public void RespawnInWorld()
        {
            _cursorRender.SetHeightChunks(_game.World.ChunkPr.Settings.NumberBlocks);
        }

        /// <summary>
        /// Смена видимости курсора чанка
        /// </summary>
        public void ChunkCursorHiddenShow() => _cursorRender.ChunkCursorHiddenShow();

        /// <summary>
        /// Смена видимости хитбокса сущностей
        /// </summary>
        public void HitboxEntitiesHiddenShow() => Entities.IsHitBox = !Entities.IsHitBox;

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
                // Alpha
                if (_renderAlphaQueues.CountForward > 0)
                {
                    _RenderQueues(false, _renderAlphaQueues);
                }

                timeBegin = stopwatch.ElapsedMilliseconds;

                if (_renderQueues.CountForward > 0)
                {
                    quantity = _RenderQueues(true, _renderQueues);

                    //"WR dbs:" + _desiredBatchSize + "|" + _batchChunksTime + "mc";
                    _batchChunksTime = (int)(stopwatch.ElapsedMilliseconds - timeBegin);
                    _desiredBatchSize = Sundry.RecommendedQuantityBatch(_batchChunksTime,
                        quantity, _desiredBatchSize, Ce.MaxDesiredBatchSize, Ce.TickTime * 2);
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
                if (isDense)
                {
                    chunkRender.Render();
                }
                else
                {
                    chunkRender.RenderAlpha();
                }
                chunkRender.ClearBufferChunks();
                if (!_flagRenderLoopRunning) break;
            }
            list.ClearBackward();

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

            Render.LightMap.Update(_game.World.Settings.HasNoSky,
                //Glm.Cos(_game.TickCounter * .01f), .24f );
            0.5f, .1f); // sunLight, MvkStatic.LightMoonPhase[World.GetIndexMoonPhase()]);
            //Render.LightMap.Update(_game.World.Settings.HasNoSky, 1, .12f);
        }

        

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            // Обновить кадр основного игрока, камера и прочее
            _game.Player.UpdateFrame(timeIndex);

            float[] view;
            
            if (gl.CheckFramebufferStatus(GL.GL_FRAMEBUFFER) == GL.GL_FRAMEBUFFER_COMPLETE)
            {
                // Буфер есть работаем дальше
                gl.Viewport(0, 0, 1024, 1024);
                gl.BindFramebuffer(GL.GL_FRAMEBUFFER, _shadowMapFbo);
                gl.Clear(GL.GL_DEPTH_BUFFER_BIT);

                // --- Начало сцены ТЕНЕЙ
                view = _game.Player.ViewDepthMap;
                // Рисуем воксели сплошных и уникальных блоков
                _DrawVoxelDenseShadowMap(timeIndex, view);
                // Сущности
                gl.Disable(GL.GL_CULL_FACE);
                Render.ShaderEntityActionShadowMap();
                Entities.DrawShadowMap(timeIndex, view);
                gl.Enable(GL.GL_CULL_FACE);
                // Облака
                //_DrawClouds(timeIndex);

                // --- Конец сцены ТЕНЕЙ

                // Меняем буфер экрана
                gl.BindFramebuffer(GL.GL_FRAMEBUFFER, 0);
                gl.Viewport(0, 0, Gi.Width, Gi.Height);
            }
            
            gl.Clear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);
            Render.ShaderEntityAction();
            view = _game.Player.View;
            // --- Начало сцены
            // Небо
            //DrawSky(timeIndex);

            // Рисуем воксели сплошных и уникальных блоков
            _DrawVoxelDense(timeIndex, view);
            // Сущности
            gl.Disable(GL.GL_CULL_FACE);
            // Биндим карту освещения
            // Render.LightMap.BindTexture();
            Entities.Draw(timeIndex, view);

            // Рендер и прорисовка курсора если это необходимо
            _cursorRender.RenderDraw();
            // Прорисовка вид не с руки, а видим себя
            Entities.DrawOwner(timeIndex);
            // Облака
            //_DrawClouds(timeIndex);

            // Рисуем воксели альфа
            gl.Enable(GL.GL_CULL_FACE);
            _DrawVoxelAlpha();
            // Прорисовка руки
            // --- Конец сцены

            Render.ShShadowMap.Bind();
            Render.ShShadowMap.SetUniformMatrix4("projview", Render.GetOrtho2D());
            _meshShadowMap.Draw();

            //OpenGLError er = gl.GetError();
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
            int batchCountAlpha = 0;
            for (int i = 0; i < count; i++)
            {
                chunkRender = _game.Player.FrustumCulling[i];
                if (chunkRender != null && chunkRender.IsChunkPresent)
                {
                    // Инициализация GL если это необходимо
                    chunkRender.InitGL();
                    // Можем ли мы в этом тике пополнять партию
                    if (batchCount < _desiredBatchSize)
                    {
                        // Проверяем надо ли рендер для чанка, и проверка загрузки рендера
                        if (chunkRender.IsModifiedRender && _renderQueues.CountBackward < 3)
                        {
                            // Проверяем занят ли чанк уже рендером
                            if (chunkRender.IsMeshDenseWait && chunkRender.IsMeshAlphaWait)
                            {
                                // Обновление рендера псевдочанка
                                Debug.CountUpdateChunck++;
                                batchCount++;
                                chunkRender.StartRendering();
                                chunkRender.UpBufferChunks();
                                _renderQueues.Add(chunkRender);
                            }
                        }
                        // Проверяем надо ли рендер для псевдо чанка, и проверка загрузки рендера
                        else if (chunkRender.IsModifiedRenderAlpha && _renderAlphaQueues.CountBackward < 4)
                        {
                            // Проверяем занят ли чанк уже рендером
                            if (chunkRender.IsMeshDenseWait && chunkRender.IsMeshAlphaWait)
                            {
                                // Обновление рендера псевдочанка
                                Debug.CountUpdateChunckAlpha++;
                                batchCountAlpha++;
                                chunkRender.StartRenderingAlpha();
                                chunkRender.UpBufferChunks();
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
            if (batchCount > 0 || batchCountAlpha > 0)
            {
                // Сигнализируем, что waitHandler в сигнальном состоянии
                _waitHandler.Set();
            }
        }

        /// <summary>
        /// Рисуем воксели сплошных и уникальных блоков для карты теней
        /// </summary>
        private void _DrawVoxelDenseShadowMap(float timeIndex, float[] view)
        {
            // Биндим шейдор для вокселей
            Render.ShaderBindVoxelsShadowMap(view, timeIndex);
            
            int count = _arrayChunkRender.Count;
            if (count > CountChunkShadowMap) count = CountChunkShadowMap;
            ChunkRender chunkRender;
            for (int i = 0; i < count; i++)
            {
                chunkRender = _arrayChunkRender[i];
                // Прорисовка сплошных блоков чанка
                if (chunkRender.NotNullMeshDense)
                {
                    Render.ShVoxelShadowMap.SetUniform2("chunk", chunkRender.CurrentChunkX << 4, chunkRender.CurrentChunkY << 4);
                    chunkRender.DrawDense();
                }
            }
        }

        /// <summary>
        /// Рисуем воксели сплошных и уникальных блоков
        /// </summary>
        private void _DrawVoxelDense(float timeIndex, float[] view)
        {
            // Биндим шейдор для вокселей
            Render.ShaderBindVoxels(view, timeIndex,
                _overviewBlock, .4f, .4f, .7f, 5);

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

            int count = _arrayChunkRender.Count;
            ChunkRender chunkRender;
            for (int i = 0; i < count; i++)
            {
                chunkRender = _arrayChunkRender[i];
                // Прорисовка сплошных блоков чанка
                if (chunkRender.NotNullMeshDense)
                {
                    Render.ShVoxel.SetUniform2("chunk", chunkRender.CurrentChunkX << 4, chunkRender.CurrentChunkY << 4);
                    chunkRender.DrawDense();
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
            //gl.DepthMask(0); // Нельзя, так-как пробивает не прозрачный блок
            gl.Enable(GL.GL_POLYGON_OFFSET_FILL);
            gl.PolygonOffset(0.5f, 10f);// -3f, -3f);

            Render.ShVoxel.Bind();

            int count = _arrayChunkRender.Count - 1;
            ChunkRender chunkRender;

            // Пробегаем по всем чанкам которые видим FrustumCulling
            for (int i = count; i >= 0; i--)
            {
                chunkRender = _arrayChunkRender[i];
                // Прорисовка альфа блоков псевдо чанка
                if (chunkRender.NotNullMeshAlpha)
                {
                    Render.ShVoxel.SetUniform2("chunk", chunkRender.CurrentChunkX << 4, chunkRender.CurrentChunkY << 4);
                    chunkRender.DrawAlpha();
                }
            }

            gl.Enable(GL.GL_POLYGON_OFFSET_FILL);
            gl.PolygonOffset(0, 0);
        }

        public override void Dispose()
        {
            BlocksReg.BlockAtlas.DeleteTexture(Render.Texture);
            Entities.Dispose();
            _cursorRender.Dispose();
            if (_shadowMapFbo != 0)
            {
                gl.DeleteFramebuffers(1, new uint[] { _shadowMapFbo });
                _shadowMapFbo = 0;
            }
            if (_shadowMapTexture != 0)
            {
                gl.DeleteTextures(1, new uint[] { _shadowMapTexture });
                _shadowMapTexture = 0;
            }
            _meshShadowMap.Dispose();
        }

        public override string ToString()
            => "WR E:" + Entities.CountEntitiesFC 
            + "  dbs:" + _desiredBatchSize + "|" + _batchChunksTime + "mc";
    }
}
