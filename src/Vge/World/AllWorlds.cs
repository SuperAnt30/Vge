using Vge.Games;

namespace Vge.World
{
    /// <summary>
    /// Объект миров этот объект отвечает за взаимосвязь всех миров и сервера
    /// </summary>
    public class AllWorlds
    {
#if DEBUG
        private const int _stepTime = 25;
#else
        private const int _stepTime = 60;
#endif
        /// <summary>
        /// Сервер
        /// </summary>
        public GameServer Server { get; protected set; }

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
                Server.Filer.EndStartSection("World-" + i, _stepTime);
                _worldServers[i].Update();
            }
            Server.Filer.EndSection(_stepTime);
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

        public override string ToString() => _worldServers[0].ToString();
    }
}
