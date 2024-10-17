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
        /// Запущен ли мир
        /// </summary>
        public bool IsRuning { get; private set; } = true;

        /// <summary>
        /// Флаг разрешающий запустить такт
        /// </summary>
        private bool _flagRunUpdate;
        /// <summary>
        /// Время затраченое за такт
        /// </summary>
        private short _timeTick = 0;

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
            while (IsRuning && Server.IsServerRunning)
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
            long timeBegin = Server.Time();

            // Обработка фрагментов в начале такта
            _FragmentBegin();

            // Тут начинается все действия с блоками АИ мобов и всё такое....
            if (IdWorld == 0)
            {
                _testAnchor.Update();
            }

            // Обработка фрагментов в конце такта
            _FragmentEnd();

            _timeTick = (short)((_timeTick * 3 + (Server.Time() - timeBegin)) / 4);
        }

        /// <summary>
        /// Останавливаем мир
        /// </summary>
        public void Stoping()
        {
            IsRuning = false;
            _WriteToFile();
        }

        #region Fragments (Chunks)

        /// <summary>
        /// Обработка фрагментов в начале такта
        /// </summary>
        private void _FragmentBegin()
        {
            if (Server.TickCounter % Ce.Tps == 0)
            {
                // Прошла секунда
                ChunkPrServ.ClearCounter();
            }
            // Запускаем фрагмент, тут определение какие чанки выгрузить, какие загрузить, 
            // определение активных чанков.
            Filer.StartSection("Fragment");
            Fragment.Update();
            // Выгрузка ненужных чанков из очереди
            Filer.StartSection("UnloadingUnnecessaryChunksFromQueue");
            ChunkPrServ.UnloadingUnnecessaryChunksFromQueue();
            Filer.EndSection();

            // Этап запуска чанков в отдельном потоке
            ChunkPrServ.UpdateRunInFlow();
        }

        /// <summary>
        /// Обработка фрагментов в конце такта
        /// </summary>
        private void _FragmentEnd()
        {
            // Дожидаемся Загрузки чанков
            while(IsRuning)
            {
                // Ждём когда отработает поток чанков
                if (!ChunkPrServ.FlagExecutionTackt)
                {
                    Thread.Sleep(1);
                }
                else
                {
                    break;
                }
            }

            // Выгрузка требуемых чанков из очереди которые отработали в отдельном потоке
            Filer.EndStartSection("UnloadingRequiredChunksFromQueue");
            ChunkPrServ.UnloadingRequiredChunksFromQueue();
            Filer.EndSection();

            
        }

        #endregion

        #region WriteRead

        /// <summary>
        /// Записать данные мира
        /// </summary>
        private void _WriteToFile()
        {
            GameFile.CheckPath(PathWorld);
        }

        #endregion

        public override string ToString() => "World-" + IdWorld 
            + " " + _timeTick + "ms " + Fragment.ToString() + " " + ChunkPrServ.ToString();
    }
}
