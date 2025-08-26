using System.Runtime.CompilerServices;
using Vge.Games;

namespace Vge.World
{
    /// <summary>
    /// Объект миров этот объект отвечает за взаимосвязь всех миров на сервере.
    /// Есть возможность мода
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

        public AllWorlds()
        {
            _worldServers = new WorldServer[_count];
        }

        /// <summary>
        /// Инициализация миров после создания сервера
        /// </summary>
        public virtual void Init(GameServer server) => Server = server;

        /// <summary>
        /// Получить индекс кидаемого блока
        /// </summary>
        public virtual int GetDebugIndex(int b) => -1;

        /// <summary>
        /// Такт сервера
        /// </summary>
        public void Update()
        {
            byte b;
            // Запускаем такты дополнительный миров в отдельных потоках
            for (b = 1; b < _count; b++)
            {
                _worldServers[b].Wait.RunInFlow();
            }

            // Отрабатываем такт основного мира
            _worldServers[0].Update();

            // Ждём отработку в остальных потоках
            for (b = 1; b < _count; b++)
            {
                _worldServers[b].Wait.Waiting();
            }

            // Если есть отладка отправляем на отладку
            if (Ce.IsDebugDrawChunks)
            {
                Server.Filer.StartSection("FragmentManagerDebug");
                _worldServers[0].Fragment.UpdateDebug();
                Server.Filer.EndSection();
            }
        }

        /// <summary>
        /// Останавливается сервер, остановить миры
        /// </summary>
        public virtual void Stoping()
        {
            // Сохраняем все миры
            for (byte i = 0; i < _count; i++)
            {
                _worldServers[i].Stoping();
            }
        }

        /// <summary>
        /// Получить мир по id
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WorldServer GetWorld(int index) => _worldServers[index];

        public override string ToString()
        {
            string s = "";
            for (byte i = 0; i < _count; i++)
            {
                s += _worldServers[i].ToString() + "\r\n";
            }
            return s;
        }
    }
}
