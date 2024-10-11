using Vge.Games;
using Vge.Management;
using Vge.Util;
using Vge.World.Chunk;

namespace Vge.World
{
    /// <summary>
    /// Серверный объект мира
    /// </summary>
    public class WorldServer : WorldBase
    {
        /// <summary>
        /// ID мира
        /// </summary>
        public readonly byte IdWorld;
        /// <summary>
        /// Имя пути к папке
        /// </summary>
        public readonly string PathWorld;
        /// <summary>
        /// Настройки мира
        /// </summary>
        public readonly WorldSettings Settings;
        /// <summary>
        /// Основной сервер
        /// </summary>
        public readonly GameServer Server;
        /// <summary>
        /// Объект управляет всеми чанками которые надо загрузить или выгрузить
        /// </summary>
        public readonly FragmentManager Fragment;
        /// <summary>
        /// Посредник серверного чанка
        /// </summary>
        public readonly ChunkProviderServer ChunkPrServ;

        private readonly TestAnchor _testAnchor;

        public WorldServer(GameServer server, byte idWorld, WorldSettings worldSettings)
        {
            Server = server;
            IdWorld = idWorld;
            PathWorld = server.Settings.GetPathWorld(IdWorld);
            Settings = worldSettings;
            Rnd = new Rand(server.Settings.Seed);
            ChunkPr = ChunkPrServ = new ChunkProviderServer(this);
            Filer = new Profiler(server.Log, "[Server] ");
            Fragment = new FragmentManager(this);
            _testAnchor = new TestAnchor(this);
            if (idWorld == 0)
            {
                Fragment.AddAnchor(_testAnchor);
            }
        }

        public override void Update()
        {
            _testAnchor.Update();
            Filer.StartSection("Fragment");
            Fragment.Update();
            Filer.EndStartSection("UnloadQueuedChunks");
            ChunkPrServ.UnloadQueuedChunks();
            Filer.EndSection();
            //System.Threading.Thread.Sleep(50);
        }

        #region WriteRead

        /// <summary>
        /// Записать данные мира
        /// </summary>
        public void WriteToFile()
        {
            GameFile.CheckPath(PathWorld);
        }

        #endregion

        public override string ToString() => Fragment.ToString() + " " + ChunkPrServ.ToString();
    }
}
