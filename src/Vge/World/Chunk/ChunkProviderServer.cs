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
        public bool NeededChunk(int x, int y)
        {
            if (IsChunkLoaded(x, y)) return true;
            _LoadGenAdd(x, y);
            return false;
        }

        /// <summary>
        /// Загрузка или генерация чанка, с пополнением его в карту чанков
        /// </summary>
        private void _LoadGenAdd(int x, int y)
        {
            ChunkBase chunk = new ChunkBase(_worldServer, x, y);
            _chunkMapping.Add(chunk);
            //LoadOrGen(chunk);
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
                    _worldServer.Fragment.flagDebugChunkProvider = true;
                }
            }
        }

        public override string ToString() => string.Format("Ch:{0} Dr:{1}",
               _chunkMapping.Count, _droppedChunks.Count);
    }
}
