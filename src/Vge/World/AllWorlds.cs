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
        public GameServer @Server { get; protected set; }

        /// <summary>
        /// Миры игры
        /// </summary>
        protected WorldServer[] _worldServers;
        /// <summary>
        /// Количество миров
        /// </summary>
        protected int _count = 1;

        public AllWorlds() => _worldServers = new WorldServer[_count];

        /// <summary>
        /// Инициализация миров после создания сервера
        /// </summary>
        public virtual void Init(GameServer server)
        {
            Server = server;
            for (byte i = 0; i < _count; i++)
            {
                _worldServers[i] = new WorldServer(server, i, new WorldSettings());
            }
        }

        /// <summary>
        /// Такт сервера
        /// </summary>
        public void Update()
        {
            Server.Filer.StartSection("World");
            _worldServers[0].Update();
            for (byte i = 1; i < _count; i++)
            {
                Server.Filer.EndStartSection("World-" + i);
                _worldServers[i].Update();
            }
            Server.Filer.EndSection();
        }

        /// <summary>
        /// Останавливается сервер, остановить миры
        /// </summary>
        public virtual void Stoping()
        {
            // Сохраняем все миры
            for (byte i = 0; i < _count; i++)
            {
                _worldServers[i].WriteToFile();
            }
        }

        public WorldServer GetWorld(int index) => _worldServers[index];
    }
}
