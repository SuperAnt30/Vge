using Vge.Event;
using Vge.Games;
using Vge.Util;
using Vge.World.Chunk;

namespace Vge.World
{
    /// <summary>
    /// Клиентский объект мира
    /// </summary>
    public class WorldClient : WorldBase
    {
        /// <summary>
        /// Посредник клиентоского чанка
        /// </summary>
        public readonly ChunkProviderClient ChunkPrClient;

        /// <summary>
        /// Флаг на изминение количество чанков
        /// </summary>
        private bool _flagDebugChunkMappingChanged;

        public WorldClient(GameBase game)
        {
            IsRemote = true;
            Rnd = new Rand();
            ChunkPr = ChunkPrClient = new ChunkProviderClient(this);
            Filer = new Profiler(game.Log, "[Client] ");
            ChunkPrClient.ChunkMappingChanged += _ChunkPrClient_ChunkMappingChanged;
        }

        private void _ChunkPrClient_ChunkMappingChanged(object sender, System.EventArgs e)
            => _flagDebugChunkMappingChanged = true;

        /// <summary>
        /// Такт выполнения
        /// </summary>
        public void Update()
        {
            if (Ce.IsDebugDrawChunks && _flagDebugChunkMappingChanged)
            {
                _flagDebugChunkMappingChanged = false;
                OnTagDebug(Debug.Key.ChunkClient.ToString(), ChunkPr.GetListDebug());
            }
        }

        public override string ToString() => ChunkPrClient.ToString();

        /// <summary>
        /// Событие любого объекта с сервера для отладки
        /// </summary>
        public event StringEventHandler TagDebug;
        public void OnTagDebug(string title, object tag)
            => TagDebug?.Invoke(this, new StringEventArgs(title, tag));
    }
}
