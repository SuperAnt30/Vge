using Vge.Event;
using Vge.Games;
using Vge.Renderer.World;
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
        /// Мир для рендера
        /// </summary>
        public readonly WorldRenderer WorldRender;
        /// <summary>
        /// Объект игры
        /// </summary>
        public readonly GameBase Game;

        /// <summary>
        /// Флаг на изминение количество чанков
        /// </summary>
        private bool _flagDebugChunkMappingChanged;

        public WorldClient(GameBase game) : base(512)
        {
            Game = game;
            IsRemote = true;
            Rnd = new Rand();
            WorldRender = game.WorldRender;
            ChunkPr = ChunkPrClient = new ChunkProviderClient(this);
            Filer = new Profiler(game.Log, "[Client] ");
            ChunkPrClient.ChunkMappingChanged += _ChunkPrClient_ChunkMappingChanged;

            WorldRender.Starting();
        }

        /// <summary>
        /// Получить время в тактах объекта Stopwatch с момента запуска проекта
        /// </summary>
        public override long ElapsedTicks() => Game.ElapsedTicks();

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

        /// <summary>
        /// Останавливаем мир
        /// </summary>
        public void Stoping()
        {
            WorldRender.Stoping();
            ChunkPrClient.Stoping();
        }

        #region Mark

        /// <summary>
        /// Сделать запрос перерендера выбранной облости псевдочанков
        /// </summary>
        public void AreaModifiedToRender(int c0x, int c0y, int c0z, int c1x, int c1y, int c1z)
        {
            int x, y, z;
            if (c0y < 0) c0y = 0;
            if (c1y > ChunkPr.Settings.NumberSectionsLess) c1y = ChunkPr.Settings.NumberSectionsLess;
            for (x = c0x; x <= c1x; x++)
            {
                for (z = c0z; z <= c1z; z++)
                {
                    ChunkRender chunk = ChunkPrClient.GetChunkRender(x, z);
                    if (chunk != null)
                    {
                        for (y = c0y; y <= c1y; y++)
                        {
                            chunk.ModifiedToRender(y);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Отметить блок для обновления
        /// </summary>
        public override void MarkBlockForUpdate(int x, int y, int z)
            => AreaModifiedToRender((x - 1) >> 4, (y - 1) >> 4, (z - 1) >> 4,
                (x + 1) >> 4, (y + 1) >> 4, (z + 1) >> 4);

        /// <summary>
        /// Отметить блоки для обновления
        /// </summary>
        public override void MarkBlockRangeForUpdate(int x0, int y0, int z0, int x1, int y1, int z1)
            => AreaModifiedToRender((x0 - 1) >> 4, (y0 - 1) >> 4, (z0 - 1) >> 4,
                (x1 + 1) >> 4, (y1 + 1) >> 4, (z1 + 1) >> 4);

        #endregion

        public override void DebugString(string logMessage, params object[] args) 
            => Debug.DebugString = string.Format(logMessage, args);

        public override string ToString() => ChunkPrClient.ToString();

        /// <summary>
        /// Событие любого объекта с сервера для отладки
        /// </summary>
        public event StringEventHandler TagDebug;
        public void OnTagDebug(string title, object tag)
            => TagDebug?.Invoke(this, new StringEventArgs(title, tag));
    }
}
