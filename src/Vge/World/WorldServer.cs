using Vge.Games;
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
        private readonly Server server;

        public WorldServer(Server server, byte idWorld, WorldSettings worldSettings)
        {
            this.server = server;
            IdWorld = idWorld;
            PathWorld = server.Settings.GetPathWorld(IdWorld);
            Settings = worldSettings;
            Filer = new Profiler(server.Log, "[Server] ");
        }

        public override void Update()
        {
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
