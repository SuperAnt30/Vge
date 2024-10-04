using Vge.Games;
using Vge.Management;
using Vge.Util;

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

        public WorldServer(GameServer server, byte idWorld, WorldSettings worldSettings)
        {
            Server = server;
            IdWorld = idWorld;
            PathWorld = server.Settings.GetPathWorld(IdWorld);
            Settings = worldSettings;
            Rnd = new Rand(server.Settings.Seed);
            Filer = new Profiler(server.Log, "[Server] ");
            Fragment = new FragmentManager(this);
        }

        public override void Update()
        {
            Filer.StartSection("Fragment");
            Fragment.Update();
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
    }
}
