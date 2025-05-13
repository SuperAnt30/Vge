using Vge.Entity;
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
        /// Основной сервер
        /// </summary>
        public readonly GameServer Server;
        /// <summary>
        /// Объект управляет всеми чанками которые надо загрузить или выгрузить
        /// </summary>
        public readonly FragmentManager Fragment;
        /// <summary>
        /// Трекер сущностей
        /// </summary>
        public readonly EntityTrackerManager Tracker;
        /// <summary>
        /// Посредник серверного чанка
        /// </summary>
        public readonly ChunkProviderServer ChunkPrServ;
        /// <summary>
        /// Ждать обработчик
        /// </summary>
        public readonly WaitHandler Wait;
        /// <summary>
        /// Запущен ли мир
        /// </summary>
        public bool IsRuning { get; private set; } = true;

        /// <summary>
        /// Время затраченое за такт
        /// </summary>
        private short _timeTick = 0;

        //private readonly TestAnchor _testAnchor;

        public WorldServer(GameServer server, byte idWorld, WorldSettings worldSettings) 
            : base(worldSettings.NumberChunkSections * 16)
        {
            Server = server;
            IdWorld = idWorld;
            PathWorld = server.Settings.GetPathWorld(IdWorld);
            Settings = worldSettings;
            Rnd = new Rand(server.Settings.Seed);
            ChunkPr = ChunkPrServ = new ChunkProviderServer(this);
            Collision.Init();
            Filer = new Profiler(server.Log, "[Server] ");
            Fragment = new FragmentManager(this);
            Tracker = new EntityTrackerManager(this);

            if (idWorld == 0)
            {
                //_testAnchor = new TestAnchor(this);
                //Fragment.AddAnchor(_testAnchor);
            }

            if (idWorld != 0)
            {
                // Все кроме основного мира, использую дополнительный поток
                Wait = new WaitHandler("World" + IdWorld);
                Wait.DoInFlow += (sender, e) => Update();
                Wait.Run();
            }
        }

        /// <summary>
        /// Внести лог
        /// </summary>
        public override void SetLog(string logMessage, params object[] args) 
            => Filer.Log.Server(logMessage, args);

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
              //  _testAnchor.Update();
            }

            // Обработка фрагментов в конце такта
            _FragmentEnd();

            Filer.StartSection("Entities");
            _UpdateEntities();
            Filer.EndStartSection("Tracker");
            Tracker.Update();
            Filer.EndSection();

            _timeTick = (short)((_timeTick * 3 + (Server.Time() - timeBegin)) / 4);
        }

        /// <summary>
        /// Останавливаем мир
        /// </summary>
        public void Stoping()
        {
            IsRuning = false;
            ChunkPrServ.Wait.Stoping();
            if (Wait != null)
            {
                Wait.Stoping();
            }
            _WriteToFile();
        }

        #region Fragments (Chunks)

        /// <summary>
        /// Отметить блок для обновления 
        /// </summary>
        public override void MarkBlockForUpdate(int x, int y, int z)
            => Fragment.FlagBlockForUpdate(x, y, z);

        /// <summary>
        /// Отметить блоки для изминения
        /// </summary>
        public override void MarkBlockRangeForModified(int x0, int z0, int x1, int z1)
        {
            int c0x = (x0) >> 4;
            int c0z = (z0) >> 4;
            int c1x = (x1) >> 4;
            int c1z = (z1) >> 4;
            ChunkBase chunk;
            int x, z;
            for (x = c0x; x <= c1x; x++)
            {
                for (z = c0z; z <= c1z; z++)
                {
                    chunk = ChunkPr.GetChunk(x, z);
                    if (chunk != null) chunk.Modified();
                }
            }
        }

        /// <summary>
        /// Отметить псевдочанки на обновления, сервер. Координаты чанков
        /// </summary>
        public void ServerMarkChunkRangeForRenderUpdate(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            int x, y, z;
            for (x = x0; x <= x1; x++)
            {
                for (z = z0; z <= z1; z++)
                {
                    for (y = y0; y <= y1; y++)
                    {
                        Fragment.FlagChunkForUpdate(x, y, z);
                    }
                }
            }
        }

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
            ChunkPrServ.Wait.RunInFlow();
        }

        /// <summary>
        /// Обработка фрагментов в конце такта
        /// </summary>
        private void _FragmentEnd()
        {
            // Дожидаемся загрузки чанков
            ChunkPrServ.Wait.Waiting();

            // Выгрузка требуемых чанков из очереди которые отработали в отдельном потоке
            Filer.EndStartSection("UnloadingRequiredChunksFromQueue");
            ChunkPrServ.UnloadingRequiredChunksFromQueue();
            Filer.EndSection();
        }

        #endregion

        #region Entity

        protected override void _OnEntityAdded(EntityBase entity)
        {
            base._OnEntityAdded(entity);
            Tracker.EntityAdd(entity);
        }

        protected override void _OnEntityRemoved(EntityBase entity)
        {
            base._OnEntityRemoved(entity);
            Tracker.UntrackEntity(entity);
        }

        /// <summary>
        /// Удаление игрока в текущем мире для перехода в другой мир
        /// </summary>
        public void RemovePlayerInWorldForNextWorld(PlayerServer player)
        {
            Fragment.RemoveAnchor(player);
            player.SetDead();
            _OnEntityRemoved(player);
            LoadedEntityList.Remove(player.Id, player);
        }

        /// <summary>
        /// Игрок переходит в другой мир
        /// </summary>
        public void PlayerForNextWorld(PlayerServer player)
        {
            Fragment.AddAnchor(player);
            SpawnEntityInWorld(player);
        }

        /// <summary>
        /// Флаг для отладки сервера
        /// </summary>
        protected override void _FlagDebugChunkProviderServer() 
            => Fragment.flagDebugChunkProviderServer = true;

        /// <summary>
        /// Изменение в каждом такте сущности
        /// </summary>
        protected override void _UpdateEntity(EntityBase entity)
        {
            if (entity.AddedToChunk)
            {
                entity.Update();
            }
            base._UpdateEntity(entity);
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
            + " " + _timeTick + "ms " + Fragment.ToString() + " " + ChunkPrServ.ToString()
            + "\r\n" + Tracker;
    }
}
