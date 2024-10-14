using System;
using System.Diagnostics;
using System.Threading;
using Vge.Event;
using Vge.Gui.Huds;
using Vge.Management;
using Vge.Network;
using Vge.Network.Packets;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.Util;
using Vge.World;
using WinGL.Actions;
using WinGL.Util;

namespace Vge.Games
{
    /// <summary>
    /// Абстрактный класс игры
    /// </summary>
    public abstract class GameBase : Warp
    {
        /// <summary>
        /// Объект лога
        /// </summary>
        public readonly Logger Log;
        /// <summary>
        /// Объект сыщик
        /// </summary>
        public readonly Profiler Filer;

        /// <summary>
        /// Локальная игра
        /// </summary>
        public bool IsLoacl { get; protected set; } = true;
        /// <summary>
        /// Пауза в игре
        /// </summary>
        public bool IsGamePaused { get; protected set; } = false;
        /// <summary>
        /// Объект индикация
        /// </summary>
        public HudBase Hud { get; protected set; }
        /// <summary>
        /// Объект игрока для клиента
        /// </summary>
        public PlayerClient Player { get; private set; }
        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient World { get; private set; }

        /// <summary>
        /// Увеличивается каждый тик 
        /// </summary>
        public uint TickCounter { get; private set; } = 0;

        /// <summary>
        /// Счётчик тиков без синхронизации с сервером, отсчёт от запуска программы
        /// </summary>
        protected uint _tickCounterClient = 0;
        /// <summary>
        /// Оповещение остановки, если "" её ещё не было
        /// </summary>
        protected string _stopNotification = "";
        /// <summary>
        /// Надо ли выводить окно оповещении 
        /// </summary>
        protected bool _stopIsWarning = false;
        /// <summary>
        /// Остановка из потока
        /// </summary>
        protected bool _isStop = false;
        /// <summary>
        /// Флаг, можно ли уже использовать тик
        /// </summary>
        protected bool _flagTick = false;

        /// <summary>
        /// Объект времени с момента запуска проекта
        /// </summary>
        private readonly Stopwatch _stopwatch = new Stopwatch();
       
        /// <summary>
        /// Флаг было ли уже остановление
        /// </summary>
        private bool _flagStoped = false;
        /// <summary>
        /// Объект работы с пакетами
        /// </summary>
        protected readonly ProcessClientPackets _packets;

        public GameBase(WindowMain window) : base(window)
        {
            Ce.InitClient();
            Log = new Logger("Logs");
            Filer = new Profiler(Log, "[Client] ");
            _packets = new ProcessClientPackets(this);
            Player = new PlayerClient(this);
        }

        /// <summary>
        /// Псевдоним игрока
        /// </summary>
        public virtual string ToLoginPlayer() => Options.Nickname;
        /// <summary>
        /// токен игрока
        /// </summary>
        public virtual string ToTokenPlayer() => Options.Token;

        #region StartStopPause

        /// <summary>
        /// Запуск игры
        /// </summary>
        public virtual void GameStarting()
        {
            World = new WorldClient(this);
            World.TagDebug += World_TagDebug;
            _stopwatch.Start();
        }

        private void World_TagDebug(object sender, StringEventArgs e) => _OnTagDebug(e);

        /// <summary>
        /// Остановка игры
        /// </summary>
        public virtual void GameStoping(string notification, bool isWarning) { }

        /// <summary>
        /// Задать паузу для одиночной игры
        /// </summary>
        public virtual void SetGamePauseSingle(bool value) { }

        /// <summary>
        /// Игрок на сервере
        /// </summary>
        public void PlayerOnTheServer(int id, string uuid)
        {
            Player.PlayerOnTheServer(id, uuid);

            // Отправим обзор 
            Player.SetOverviewChunk(Options.OverviewChunk, false);
            // Закрываем скрин загрузки
            window.LScreen.Close();
            _flagTick = true;
        }

        /// <summary>
        /// Получили пакет загрузки от сервера
        /// </summary>
        public virtual void PacketLoadingGame(PacketS02LoadingGame packet) { }

        #endregion

        #region OnKey

        /// <summary>
        /// Клавиша нажата
        /// </summary>
        public override void OnKeyDown(Keys keys)
        {
            // Скрина быть не должно коль клавиши тут работают
            if (keys == Keys.Escape)
            {
                SetGamePauseSingle(true);
                window.LScreen.InGameMenu();
            }
        }

        /// <summary>
        /// Клавиша отпущена
        /// </summary>
        public override void OnKeyUp(Keys keys) { }

        /// <summary>
        /// Нажата клавиша в char формате
        /// </summary>
        public override void OnKeyPress(char key) { }

        #endregion

        #region TickDraw

        /// <summary>
        /// Игровой такт
        /// </summary>
        public override void OnTick(float deltaTime)
        {
            // почерёдно получаем пакеты с сервера
            _packets.Update();

            if (_flagTick && !IsGamePaused)
            {
                _tickCounterClient++;
                TickCounter++;

                World.Update();

                // Прошла минута, или 1800 тактов
                if (_tickCounterClient % 1800 == 0)
                {
                    Log.Save();
                }

                if (_tickCounterClient % 150 == 0)
                {
                    // Раз в 5 секунды перепинговка
                    TrancivePacket(new Packet00PingPong(Time()));
                }
            }
            // Тест
         //   TrancivePacket(new PacketC04PlayerPosition(new System.Numerics.Vector3(1, 2.5f, 3.5f), true, false, true));
        }

        /// <summary>
        /// Получить время в милисекундах с момента запуска проекта
        /// </summary>
        public long Time() => _stopwatch.ElapsedMilliseconds;

        /// <summary>
        /// Задать время с сервера
        /// </summary>
        public void SetTickCounter(uint time) => TickCounter = time;

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            // Тут прорисовка мира
            gl.ClearColor(.4f, .4f, .7f, 1f);

            // мир
            // Тут индикация если имеется
            if (Hud != null)
            {
                Hud.Draw();
            }

            if (Ce.FlagDebugDrawChunks)
            {
                Debug.DrawChunks(window);
            }
        }

        #endregion

        #region Packets

        /// <summary>
        /// Отправить пакет на сервер
        /// </summary>
        public virtual void TrancivePacket(IPacket packet) { }

        #endregion

        public override string ToString()
        {
            return string.Format("{0} ping: {1} ms Traffic: {3}{2} Tick {4}\r\n{5}\r\n{6}",
                IsLoacl ? "Local" : "Net", // 0
                Player.Ping, IsGamePaused ? " Pause" : "", // 1-2
                _ToTraffic(),TickCounter, // 3-4
                Player, // 5
                World.ToString() // 6
                );
        }

        private string _ToTraffic()
        {
            long traffic = _packets.Traffic;
            if (traffic < 1048576)
            {
                return (traffic / 1024f).ToString("0.0") + " kb";
            }
            else
            {
                return (traffic / 1048576f).ToString("0.0") + " mb";
            }
        }

        #region Event

        /// <summary>
        /// Событие остановлена игра
        /// </summary>
        public event GameStopEventHandler Stoped;
        protected void _OnStoped(string notification, bool isWarning)
        {
            if (!_flagStoped)
            {
                // флаг нужен, так-как можно попасть сюда много раз, из-за разрыва сети.
                _flagStoped = true;
                Log.Client(Srl.StoppedClient, notification == "" ? "" : " [" + notification + "]");
                Log.Save();
                Stoped?.Invoke(this, new GameStopEventArgs(notification, isWarning));
            }
        }

        /// <summary>
        /// Событие ошибки на сервере
        /// </summary>
        public event ThreadExceptionEventHandler Error;
        protected void _OnError(Exception ex)
            => Error?.Invoke(this, new ThreadExceptionEventArgs(ex));

        /// <summary>
        /// Событие текст отладки сервера
        /// </summary>
        public event StringEventHandler ServerTextDebug;
        protected virtual void _OnServerTextDebug(string text) 
            => ServerTextDebug?.Invoke(this, new StringEventArgs(text));

        /// <summary>
        /// Событие tag отладки сервера
        /// </summary>
        public event StringEventHandler TagDebug;
        protected virtual void _OnTagDebug(StringEventArgs e)
            => TagDebug?.Invoke(this, e);

        #endregion
    }
}
