using System.Threading;
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
        /// <summary>
        /// Флаг выполненого такта
        /// </summary>
        public bool FlagExecutionTackt { get; private set; }

        /// <summary>
        /// Флаг разрешающий запустить такт
        /// </summary>
        private bool _flagRunUpdate;
        /// <summary>
        /// Запущен ли мир
        /// </summary>
        private bool _isRuning = true;
        

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

            if (idWorld != 0 || Ce.OneWorldRunInFlow)
            {
                // Запускаем отдельный поток для всех дополнительных миров
                Thread myThread = new Thread(_ThreadUpdate) { Name = "World" + idWorld };
                myThread.Start();
            }
        }

        #region В потоке

        /// <summary>
        /// Отдельный поток для дополнительного мира
        /// </summary>
        private void _ThreadUpdate()
        {
            while (_isRuning && Server.IsServerRunning)
            {
                if (_flagRunUpdate)
                {
                    _flagRunUpdate = false;
                    Update();
                    FlagExecutionTackt = true;
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        /// <summary>
        /// Запуск в потоке
        /// </summary>
        public void UpdateRunInFlow()
        {
            _flagRunUpdate = true;
            FlagExecutionTackt = false;
        }

        #endregion

        /// <summary>
        /// Такт выполнения
        /// </summary>
        public void Update()
        {
            _testAnchor.Update();
            Filer.StartSection("Fragment");
            Fragment.Update();
            Filer.EndStartSection("UnloadQueuedChunks", Profiler.StepTime);
            ChunkPrServ.UnloadQueuedChunks();
            Filer.EndSection();
        }

        /// <summary>
        /// Останавливаем мир
        /// </summary>
        public void Stoping()
        {
            _isRuning = false;
            _WriteToFile();
        }

        #region WriteRead

        /// <summary>
        /// Записать данные мира
        /// </summary>
        private void _WriteToFile()
        {
            GameFile.CheckPath(PathWorld);
        }

        #endregion

        public override string ToString() => Fragment.ToString() + " " + ChunkPrServ.ToString();
    }
}
