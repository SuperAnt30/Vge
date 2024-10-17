namespace Vge.World.Chunk
{
    /// <summary>
    /// Объект который хранит и отвечает за кэш чанков конкретного мира
    /// </summary>
    public abstract class ChunkProvider
    {
        /// <summary>
        /// Количество секций в чанке
        /// </summary>
        public static byte NumberSections { get; private set; }
        /// <summary>
        /// Количество секций в чанке меньше. NumberChunkSections - 1
        /// </summary>
        public static byte NumberSectionsLess { get; private set; }
        /// <summary>
        /// Количество блоков в чанке. NumberChunkSections * 16 - 1
        /// </summary>
        public static ushort NumberBlocks { get; private set; }
        /// <summary>
        /// Верхний блок который можно установить. NumberBlocks - 1
        /// </summary>
        public static ushort NumberMaxBlock { get; private set; }

        /// <summary>
        /// Сылка на объект мира
        /// </summary>
        private readonly WorldBase _world;
        /// <summary>
        /// Карта чанков
        /// </summary>
        protected MapChunk _chunkMapping = new MapChunk();

        public ChunkProvider(WorldBase world)
        {
            _world = world;
        }

        #region chunkMapping

        /// <summary>
        /// Проверить наличие чанка в массиве
        /// </summary>
        public bool IsChunkLoaded(int x, int y)
        {
            if (_chunkMapping.Contains(x, y))
            {
                ChunkBase chunk = GetChunk(x, y);
                return chunk != null && chunk.IsChunkPresent;
            }
            return false;
        }

        /// <summary>
        /// Получить чанк по координатам чанка
        /// </summary>
        public virtual ChunkBase GetChunk(int x, int y) 
            => _chunkMapping.Get(x, y) as ChunkBase;

        /// <summary>
        /// Количество чанков в кэше
        /// </summary>
        public int Count => _chunkMapping.Count;

        /// <summary>
        /// Список чанков только для отладки
        /// </summary>
        public IChunkPosition[] GetListDebug() => _chunkMapping.ToArrayDebug();

        #endregion

        /// <summary>
        /// Задать высоту чанков
        /// </summary>
        public void SetHeightChunks(byte height)
        {
            NumberSections = height;
            NumberSectionsLess = (byte)(height - 1);
            NumberBlocks = (ushort)(height * 16 - 1);
            NumberMaxBlock = (ushort)(height * 16 - 2);
        }
    }
}
