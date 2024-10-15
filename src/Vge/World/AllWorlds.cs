using System;
using System.Threading;
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

        public AllWorlds()
        {
            _worldServers = new WorldServer[_count];
        }

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
            if (Ce.OneWorldRunInFlow)
            {
                _worldServers[0].UpdateRunInFlow();
            }
            else
            {
                _worldServers[0].Update();
            }
            for (byte i = 1; i < _count; i++)
            {
                _worldServers[i].UpdateRunInFlow();
            }

            if (_count > 1 || Ce.OneWorldRunInFlow)
            {
                bool flag;
                // Если мир отрабатываем в потоке
                while (Server.IsServerRunning)
                {
                    // Ждём когда отработает первый мир
                    if (!Ce.OneWorldRunInFlow || _worldServers[0].FlagExecutionTackt)
                    {
                        flag = true;
                        // Далее ждём когда отработают все остальные миры
                        for (byte i = 1; i < _count; i++)
                        {
                            if (!_worldServers[i].FlagExecutionTackt)
                            {
                                flag = false;
                                Thread.Sleep(1);
                                break;
                            }
                        }
                        if (flag)
                        {
                            break;
                        }
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
            }

            if (Ce.IsDebugDraw && Ce.IsDebugDrawChunks)
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
