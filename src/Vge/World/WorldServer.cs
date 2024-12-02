﻿using Vge.Games;
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
        {
            Server = server;
            IdWorld = idWorld;
            PathWorld = server.Settings.GetPathWorld(IdWorld);
            Settings = worldSettings;
            Rnd = new Rand(server.Settings.Seed);
            ChunkPr = ChunkPrServ = new ChunkProviderServer(this);
            Filer = new Profiler(server.Log, "[Server] ");
            Fragment = new FragmentManager(this);


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
