using System.Runtime.CompilerServices;
using WinGL.Util;

namespace Vge.World.Chunk
{
    /// <summary>
    /// Объект который хранит и отвечает за кэш чанков конкретного мира
    /// </summary>
    public abstract class ChunkProvider
    {
        /// <summary>
        /// Опции высот чанка
        /// </summary>
        public readonly ChunkSettings Settings = new ChunkSettings();
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ChunkBase GetChunk(int x, int y) 
            => _chunkMapping.Get(x, y) as ChunkBase;

        /// <summary>
        /// Получить чанк по координатам чанка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChunkBase GetChunk(Vector2i chunkPos)
            => _chunkMapping.Get(chunkPos.X, chunkPos.Y) as ChunkBase;

        /// <summary>
        /// Количество чанков в кэше
        /// </summary>
        public int Count => _chunkMapping.Count;

        /// <summary>
        /// Список чанков только для отладки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IChunkPosition[] GetListDebug() => _chunkMapping.ToArrayDebug();

        #endregion
    }
}
