using Vge.Games;

namespace Vge.World
{
    /// <summary>
    /// Объект миров этот объект отвечает за взаимосвязь всех миров и сервера
    /// </summary>
    public class AllWorlds
    {
        /// <summary>
        /// Сервер
        /// </summary>
        protected Server server;
        /// <summary>
        /// Миры игры
        /// </summary>
        protected WorldServer[] worldServers;
        /// <summary>
        /// Количество миров
        /// </summary>
        protected int count = 1;

        public AllWorlds() => worldServers = new WorldServer[count];

        /// <summary>
        /// Инициализация миров после создания сервера
        /// </summary>
        public virtual void Init(Server server)
        {
            this.server = server;
            for (byte i = 0; i < count; i++)
            {
                worldServers[i] = new WorldServer(server, i, new WorldSettings());
            }
        }

        /// <summary>
        /// Такт сервера
        /// </summary>
        public void Update()
        {
            server.Filer.StartSection("World");
            worldServers[0].Update();
            for (byte i = 1; i < count; i++)
            {
                server.Filer.EndStartSection("World-" + i);
                worldServers[i].Update();
            }
            server.Filer.EndSection();
        }

        /// <summary>
        /// Останавливается сервер, остановить миры
        /// </summary>
        public virtual void Stoping()
        {
            // Сохраняем все миры
            for (byte i = 0; i < count; i++)
            {
                worldServers[i].WriteToFile();
            }
        }
    }
}
