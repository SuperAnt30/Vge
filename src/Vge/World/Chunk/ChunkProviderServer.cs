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
        public byte LoadingBatchSize { get; private set; } = Ce.StartDesiredBatchSize;
        /// <summary>
        /// Ждать обработчик
        /// </summary>
        public readonly WaitHandler Wait;

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
        /// Карта чанков которые надо загрузить
        /// </summary>
        private readonly MapChunk _loadingChunkMapping = new MapChunk();
        
        /// <summary>
        /// Счётчик для партии закачки чанков прошлой секунды
        /// </summary>
        private int _counterLBSprev;
        /// <summary>
        /// Счётчик для партии закачки чанков в секунду
        /// </summary>
        private int _counterLBS;

        public ChunkProviderServer(WorldServer world) : base(world)
        {
            _worldServer = world;
            Settings.SetHeightChunks(world.Settings.NumberChunkSections);
            Wait = new WaitHandler("Chunk" + world.IdWorld);
            Wait.DoInFlow += (sender, e) => _LoadQueuedChunks();
            Wait.Run();
        }

        /// <summary>
        /// Стартовая загрузка чанка в лоадинге
        /// </summary>
        public void InitialLoadingChunk(int x, int y)
        {
            ChunkBase chunk = new ChunkBase(_worldServer, Settings, x, y);
            _chunkMapping.Add(chunk);
            chunk.LoadingOrGen();
        }

        /// <summary>
        /// Нужен чанк, вернёт true если чанк был
        /// </summary>
        public bool NeededChunk(int x, int y)
        {
            _droppedChunks.Remove(Conv.ChunkXyToIndex(x, y));
            if (IsChunkLoaded(x, y)) return true;
            ChunkBase chunk = new ChunkBase(_worldServer, Settings, x, y);
            _loadingChunks.Add(chunk);
            _loadingChunkMapping.Add(chunk);
            return false;
        }

        /// <summary>
        /// Получить чанк по координатам чанка плюс проверка в загрузке
        /// </summary>
        public ChunkBase GetChunkPlus(int x, int y)
        {
            if (!(_chunkMapping.Get(x, y) is ChunkBase chunk))
            {
                return _loadingChunkMapping.Get(x, y) as ChunkBase;
            }
            return chunk;
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
                    chunk.LoadingOrGen();
                }

                _counterLBS += count;
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
            int count = _loadingChunks.Count;
            if (count > 0)
            {
                int i;
                ChunkBase chunk;
                for (i = 0; i < count; i++)
                {
                    chunk = _loadingChunks[i];
                    _chunkMapping.Add(chunk);
                }
                _loadingChunkMapping.Clear();
                _loadingChunks.Clear();
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
        /// Обнулить счётчик
        /// </summary>
        public void ClearCounter()
        {
            _counterLBSprev = _counterLBS;
            _counterLBS = 0;
        }

        public override string ToString() => "Ch:" + _chunkMapping.ToString()
            + " Lbs:" + LoadingBatchSize + "|" + _counterLBSprev
            + " Dr:" + _droppedChunks.Count;
    }
}
