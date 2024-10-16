using System.Threading;
using Vge.Util;

namespace Vge.World.Chunk
{
    /// <summary>
    /// Объект сервер который хранит и отвечает за кэш чанков
    /// </summary>
    public class ChunkProviderServer : ChunkProvider
    {
        /// <summary>
        /// Размер партии закачки чанков
        /// </summary>
        public byte LoadingBatchSize { get; private set; } = Ce.MinDesiredBatchSize;
        /// <summary>
        /// Флаг выполненого такта
        /// </summary>
        public bool FlagExecutionTackt { get; private set; }

        /// <summary>
        /// Посредник серверного чанка
        /// </summary>
        private readonly WorldServer _worldServer;

        /// <summary>
        /// Список чанков которые надо выгрузить
        /// </summary>
        private readonly ListMessy<ulong> _droppedChunks = new ListMessy<ulong>();
        /// <summary>
        /// Список чанков которые надо загрузить
        /// </summary>
        private readonly ListMessy<ChunkBase> _loadingChunks = new ListMessy<ChunkBase>();

        /// <summary>
        /// Флаг разрешающий запустить такт
        /// </summary>
        private bool _flagRunUpdate;

        public ChunkProviderServer(WorldServer world) : base(world)
        {
            _worldServer = world;
            // Запускаем отдельный поток для загрузки и генерации чанков
            Thread myThread = new Thread(_ThreadUpdate) { Name = "Chunk" + world.IdWorld };
            myThread.Start();
        }

        /// <summary>
        /// Стартовая загрузка чанка в лоадинге
        /// </summary>
        public void InitialLoadingChunk(int x, int y)
        {
            ChunkBase chunk = new ChunkBase(_worldServer, x, y);
            _chunkMapping.Add(chunk);
            _LoadOrGen(chunk);
            chunk.OnChunkLoad();
        }

        /// <summary>
        /// Нужен чанк, вернёт true если чанк был
        /// </summary>
        public bool NeededChunk(int x, int y)
        {
            if (IsChunkLoaded(x, y)) return true;
            ChunkBase chunk = new ChunkBase(_worldServer, x, y);
            _loadingChunks.Add(chunk);
            return false;
        }

        /// <summary>
        /// Добавить чанк на удаление
        /// </summary>
        public void DropChunk(int x, int y) => _droppedChunks.Add(Conv.ChunkXyToIndex(x, y));

        /// <summary>
        /// Загрузка поставленных в очередь чанков
        /// </summary>
        private void _LoadQueuedChunks()
        {
            int count = _loadingChunks.Count;
            if (count > 0)
            {
                ChunkBase chunk;
                int i;

                long timeBegin = _worldServer.Server.Time();
                for (i = 0; i < count; i++)
                {
                    chunk = _loadingChunks[i];
                    _LoadOrGen(chunk);
                    chunk.OnChunkLoad();
                }
                LoadingBatchSize = Sundry.RecommendedQuantityBatch(
                    (int)(_worldServer.Server.Time() - timeBegin),
                    count, LoadingBatchSize);
            }
        }

        /// <summary>
        /// Выгрузка требуемых чанков из очереди
        /// </summary>
        public void UnloadingRequiredChunksFromQueue()
        {
            
            int i;
            int count = _loadingChunks.Count;
            ChunkBase chunk;
            count--;
            for (i = count; i >= 0; i--)
            {
                chunk = _loadingChunks[i];
                _loadingChunks.RemoveLast();
                _chunkMapping.Add(chunk);
            }
        }

        /// <summary>
        /// Выгрузка ненужных чанков из очереди
        /// </summary>
        public void UnloadingUnnecessaryChunksFromQueue()
        {
            int i, x, y;
            int count = _droppedChunks.Count;
            int first = count - Ce.MaxCountDroppedChunks;
            if (first < 0) first = 0;
            ulong index;
            ChunkBase chunk;
            count--;
            for (i = count; i >= first; i--)
            {
                index = _droppedChunks[i];
                x = Conv.IndexToChunkX(index);
                y = Conv.IndexToChunkY(index);
                _droppedChunks.RemoveLast();
                chunk = _chunkMapping.Get(x, y) as ChunkBase;
                if (chunk != null)
                {
                    chunk.OnChunkUnload();
                    //Тут сохраняем чанк
                    //SaveChunkData(chunk);
                    _chunkMapping.Remove(x, y);
                    // Для дебага
                    _worldServer.Fragment.flagDebugChunkProviderServer = true;
                }
            }
        }

        /// <summary>
        /// Загружаем, если нет чанка то генерируем
        /// </summary>
        /// <param name="chunk">Объект чанка не null</param>
        private void _LoadOrGen(ChunkBase chunk)
        {
            if (!chunk.IsChunkPresent)
            {
                // Пробуем загрузить с файла
                float f, d;
                f = d = .5f;

                //  _worldServer.Filer.StartSection("Reg");
                // 1.2-2.1 мс
                for (int i = 0; i < 500000; i++)
                {
                    f *= d + i;
                }
            }
        }

        #region В потоке

        /// <summary>
        /// Отдельный поток для дополнительного мира
        /// </summary>
        private void _ThreadUpdate()
        {
            while (_worldServer.IsRuning)
            {
                if (_flagRunUpdate)
                {
                    _flagRunUpdate = false;
                    _LoadQueuedChunks();
                    FlagExecutionTackt = true;
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        /// <summary>
        /// Запуск в потоке
        /// </summary>
        public void UpdateRunInFlow()
        {
            _flagRunUpdate = true;
            FlagExecutionTackt = false;
        }

        #endregion

        public override string ToString() => "Ch:" + _chunkMapping.ToString()
            + " LBS:" + LoadingBatchSize
            + " Dr:" + _droppedChunks.Count;
    }
}
