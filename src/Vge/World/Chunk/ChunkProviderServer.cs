using System;
using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World.Gen;
using WinGL.Util;

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
        /// Объект для генерации чанков
        /// </summary>
        public readonly IChunkProviderGenerate ChunkGenerate;
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
        private readonly ListMessy<ChunkServer> _loadingChunks = new ListMessy<ChunkServer>();
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
            ChunkGenerate = world.Settings.ChunkGenerate;
            Wait = new WaitHandler("Chunk" + world.IdWorld);
            Wait.DoInFlow += (sender, e) => _LoadQueuedChunks();
            Wait.Run();
        }

        /// <summary>
        /// Стартовая загрузка чанка в лоадинге
        /// </summary>
        public void InitialLoadingChunk(int x, int y)
        {
            ChunkServer chunk = new ChunkServer(_worldServer, Settings, x, y);
            _chunkMapping.Add(chunk);
            _LoadOrGen(chunk);
        }

        /// <summary>
        /// Нужен чанк, вернёт true если чанк был
        /// </summary>
        public bool NeededChunk(int x, int y)
        {
            _droppedChunks.Remove(Conv.ChunkXyToIndex(x, y));
            if (IsChunkLoaded(x, y)) return true;
            ChunkServer chunk = new ChunkServer(_worldServer, Settings, x, y);
            _loadingChunks.Add(chunk);
            _loadingChunkMapping.Add(chunk);
            return false;
        }

        /// <summary>
        /// Получить чанк по координатам чанка плюс проверка в загрузке
        /// </summary>
        public ChunkServer GetChunkPlus(int x, int y)
        {
            if (!(_chunkMapping.Get(x, y) is ChunkServer chunk))
            {
                return _loadingChunkMapping.Get(x, y) as ChunkServer;
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
                ChunkServer chunk;
                int i;

                long timeBegin = _worldServer.Server.Time();
                for (i = 0; i < count; i++)
                {
                    chunk = _loadingChunks[i];
                    _LoadOrGen(chunk);
                }

                _counterLBS += count;
                //" Lbs:" + LoadingBatchSize + "|" + _counterLBSprev
                //int time = (int)(_worldServer.Server.Time() - timeBegin);
                LoadingBatchSize = Sundry.RecommendedQuantityBatch(
                    (int)(_worldServer.Server.Time() - timeBegin),
                    count, LoadingBatchSize, Ce.MaxDesiredBatchSize, Ce.MaxBatchChunksTime);

                //_worldServer.Server.Log.Log("Lbs: " + LoadingBatchSize + " t:" + time);
            }
        }

        /// <summary>
        /// Загружаем, если нет чанка то генерируем
        /// </summary>
        /// <param name="chunk">Объект чанка не null</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _LoadOrGen(ChunkServer chunk)
        {
            if (!chunk.IsChunkPresent)
            {
                // Пробуем загрузить с файла
                try
                {
                    //Stopwatch stopwatch = new Stopwatch();
                    //stopwatch.Start();
                    //chunk.LoadFileChunk(worldServer);
                    // if (chunk.IsChunkLoaded) world.Log.Log("ChunkLoad[{1}]: {0:0.00} ms", stopwatch.ElapsedTicks / (float)MvkStatic.TimerFrequency, chunk.Position);
                }
                catch (Exception ex)
                {
                    _worldServer.SetLog(Srl.CouldNotReadChunkInRegion,
                        ex.Message, ex.StackTrace, chunk.ToPosition(), chunk.ToRegion());
                }
                if (!chunk.IsChunkPresent)
                {
                    // Начинаем генерацию рельефа
                    ChunkGenerate.Relief(chunk);
                }
                // Готова начальная генерация или загрузка
                chunk.ChunkPresent();
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
