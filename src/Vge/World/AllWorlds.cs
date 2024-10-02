using Vge.Games;

namespace Vge.World
{
    /// <summary>
    /// Объект миров этот объект отвечает за взаимосвязь всех миров и сервера
    /// </summary>
    public class AllWorlds
    {
        /// <summary>
        /// Миры игры
        /// </summary>
        public readonly WorldServer[] worldServers;

        /// <summary>
        /// Сервер
        /// </summary>
        protected Server server;

        private const int count = 2;

        public AllWorlds() => worldServers = new WorldServer[count];

        public void Init(Server server)
        {
            this.server = server;
            worldServers[0] = new WorldServer(server, 0, new WorldSettings());
            worldServers[1] = new WorldServer(server, 1, new WorldSettings());
        }

        /// <summary>
        /// Такт сервера
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < count; i++)
            {
                server.Filer.StartSection("World-" + i);
                worldServers[i].Update();
                server.Filer.EndSection();
            }
        }
    }
}
