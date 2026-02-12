using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Список чанков для сохранения
        /// </summary>
        private ListMessy<ChunkServer> _savingChunks = new ListMessy<ChunkServer>();

        /// <summary>
        /// Счётчик для партии закачки чанков прошлой секунды
        /// </summary>
        private int _counterLBSprev;
        /// <summary>
        /// Счётчик для партии закачки чанков в секунду
        /// </summary>
        private int _counterLBS;
        /// <summary>
        /// Флаг для сохранения региона
        /// </summary>
        private FlagSave _flagSaveRegion = FlagSave.None;

        private enum FlagSave
        {
            /// <summary>
            /// Нет действия
            /// </summary>
            None,
            /// <summary>
            /// Был запуск сохранения но чанки ещё не готовы
            /// </summary>
            SavingChunk,
            /// <summary>
            /// Надо сохранять регион
            /// </summary>
            SavingRegin
        }


        public ChunkProviderServer(WorldServer world) : base(world)
        {
            _worldServer = world;
            Settings.SetHeightChunks(world.Settings.NumberChunkSections);
            ChunkGenerate = world.Settings.ChunkGenerate;
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
        public void LoadQueuedChunks()
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
            ChunkServer chunk;
            count--;
            for (i = count; i >= first; i--)
            {
                index = _droppedChunks[i];
                x = Conv.IndexToChunkX(index);
                y = Conv.IndexToChunkY(index);
                _droppedChunks.RemoveLast();
                chunk = _chunkMapping.Get(x, y) as ChunkServer;
                if (chunk != null)
                {
                    chunk.OnChunkUnload();
                    //Тут сохраняем чанк
                    _SaveChunkData(chunk);
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

        #region Save

        /// <summary>
        /// Начало сохранение чанков
        /// </summary>
        public void BeginSaving()
        {
            if (_savingChunks.Count == 0)
            {
                // готовим список чанков для сохранения
                List<IChunkPosition> chunks = _chunkMapping.GetList();
                foreach (ChunkServer chunk in chunks.Cast<ChunkServer>())
                {
                    _savingChunks.Add(chunk);
                }
                _flagSaveRegion = FlagSave.SavingChunk;
            }
        }

        /// <summary>
        /// Сохранение чанков в одном тике
        /// </summary>
        public void TickSaving()
        {
            if (_flagSaveRegion == FlagSave.SavingChunk)
            {
                if (_savingChunks.Count > 0)
                {
                    // Если были не сохранённые чанки сохраняем пакет из 50 чанков за тик
                    int i = 0;
                    bool b = true;
                    while (i < 50 && b)
                    {
                        if (_savingChunks.Count == 0) b = false;
                        else if (_SaveChunkData(_savingChunks[0])) i++;
                    }
                }
                else
                {
                    _flagSaveRegion = FlagSave.SavingRegin;
                }
            }
        }

        /// <summary>
        /// Сохранение регионов
        /// </summary>
        public void SavingRegions()
        {
            if (_flagSaveRegion == FlagSave.SavingRegin)
            {
                _worldServer.Regions.WriteToFile(false);
                _flagSaveRegion = FlagSave.None;
            }
        }

        /// <summary>
        /// Сохранить все чанки при закрытии мира
        /// </summary>
        public void SaveChunks()
        {
            List<IChunkPosition> chunks = _chunkMapping.GetList();
            int all = 0;
            int save = 0;
            foreach (ChunkServer chunk in chunks.Cast<ChunkServer>())
            {
                if (_SaveChunkData(chunk)) save++;
                all++;
            }
            _worldServer.Server.Log.Server(Srl.ServerSavingChunk, all, save);
        }

        /// <summary>
        /// Сохранить основные данные чанка
        /// </summary>
        private bool _SaveChunkData(ChunkServer chunk)
        {
            if (chunk != null)
            {
                try
                {
                    _savingChunks.Remove(chunk);
                    return chunk.SaveFileChunk();
                }
                catch (Exception ex)
                {
                    _worldServer.Server.Log.Error(Srl.ServerSavingChunkError,
                        ex.Message, ex.StackTrace,
                        chunk.X, chunk.Y, chunk.X >> 5, chunk.Y >> 5);
                }
            }
            return false;
        }

        /// <summary>
        /// Cгенерировать список регионов
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<ulong> GetRegionList() => _chunkMapping.GetRegionList();

        #endregion

        public override string ToString() => "Ch:" + _chunkMapping.ToString()
            + " Lbs:" + LoadingBatchSize + "|" + _counterLBSprev
            + " Dr:" + _droppedChunks.Count;
    }
}
