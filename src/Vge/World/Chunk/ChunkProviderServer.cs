using Vge.Util;

namespace Vge.World.Chunk
{
    /// <summary>
    /// Объект сервер который хранит и отвечает за кэш чанков
    /// </summary>
    public class ChunkProviderServer : ChunkProvider
    {
        /// <summary>
        /// Посредник серверного чанка
        /// </summary>
        private readonly WorldServer _worldServer;

        /// <summary>
        /// Список чанков которые надо выгрузить
        /// </summary>
        private readonly ListMessy<ulong> _droppedChunks = new ListMessy<ulong>();

        public ChunkProviderServer(WorldServer world) : base(world)
        {
            _worldServer = world;
        }

        /// <summary>
        /// Нужен чанк, вернёт true если чанк был
        /// </summary>
        /// <param name="imStrom">В отдельном потоке генерируем или загружаем чанк</param>
        public bool NeededChunk(int x, int y, bool imStrom = true)
        {
            if (IsChunkLoaded(x, y)) return true;
            _LoadGenAdd(x, y, imStrom);
            return false;
        }

        /// <summary>
        /// Загрузка или генерация чанка, с пополнением его в карту чанков
        /// </summary>
        /// <param name="imStrom">В отдельном потоке генерируем или загружаем чанк</param>
        private void _LoadGenAdd(int x, int y, bool imStrom)
        {
            ChunkBase chunk = new ChunkBase(_worldServer, x, y);
            _chunkMapping.Add(chunk);
            _LoadOrGen(chunk, imStrom);
            //System.Threading.Thread.Sleep(1);
            chunk.OnChunkLoad();
        }

        /// <summary>
        /// Добавить чанк на удаление
        /// </summary>
        public void DropChunk(int x, int y) => _droppedChunks.Add(Conv.ChunkXyToIndex(x, y));

        /// <summary>
        /// Выгрузка ненужных чанков Для сервера
        /// </summary>
        public void UnloadQueuedChunks()
        {
            int i, x, y;
            int count = _droppedChunks.Count;
            int first = count - 100;
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
        /// <param name="imStrom">В отдельном потоке генерируем или загружаем чанк</param>
        private void _LoadOrGen(ChunkBase chunk, bool imStrom)
        {
            if (!chunk.IsChunkPresent)
            {
                // Пробуем загрузить с файла
                if (imStrom)
                {
                    //System.Threading.Tasks.Task.Factory.StartNew(_Thread);
                    _Thread();
                }
                else
                {
                    //  _Thread();
                }
                
            }
        }

        private void _Thread()
        {
            float f, d;
            f = d = .5f;

          //  _worldServer.Filer.StartSection("Reg");
            // 1.2-2.1 мс
            for (int i = 0; i < 500000; i++)
            {
                f *= d + i;
            }
            //_worldServer.Filer.EndSectionLog();
            
        }


        public override string ToString() => "Ch:" + _chunkMapping.ToString()
            + " Dr:" + _droppedChunks.Count;
    }
}
