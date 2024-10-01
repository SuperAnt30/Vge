using System;
using System.Diagnostics;
using System.Threading;
using Vge.Event;
using Vge.Gui.Huds;
using Vge.Network;
using Vge.Network.Packets;
using Vge.Network.Packets.Server;
using Vge.Util;
using WinGL.Actions;

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
        /// Пинг к серверу
        /// </summary>
        public int Ping { get; private set; } = -1;
        /// <summary>
        /// Объект индикация
        /// </summary>
        public HudBase Hud { get; protected set; }

        /// <summary>
        /// Увеличивается каждый тик 
        /// </summary>
        public uint TickCounter { get; private set; } = 0;

        /// <summary>
        /// Счётчик тиков без синхронизации с сервером, отсчёт от запуска программы
        /// </summary>
        protected uint tickCounterClient = 0;
        /// <summary>
        /// Оповещение остановки, если "" её ещё не было
        /// </summary>
        protected string stopNotification = "";
        /// <summary>
        /// Надо ли выводить окно оповещении 
        /// </summary>
        protected bool stopIsWarning = false;
        /// <summary>
        /// Остановка из потока
        /// </summary>
        protected bool isStop = false;
        /// <summary>
        /// Флаг, можно ли уже использовать тик
        /// </summary>
        protected bool flagTick = false;

        /// <summary>
        /// Объект времени с момента запуска проекта
        /// </summary>
        private readonly Stopwatch stopwatch = new Stopwatch();
        /// <summary>
        /// Последнее время пинга в милисекундах
        /// </summary>
        private long lastTimeServer;
        /// <summary>
        /// Флаг было ли уже остановление
        /// </summary>
        private bool flagStoped = false;
        /// <summary>
        /// Объект работы с пакетами
        /// </summary>
        protected readonly ProcessClientPackets packets;

        public GameBase(WindowMain window) : base(window)
        {
            Log = new Logger("Logs");
            Filer = new Profiler(Log, "[Client] ");
            packets = new ProcessClientPackets(this);
        }

        /// <summary>
        /// Псевдоним игрока
        /// </summary>
        public virtual string ToLoginPlayer() => "";
        /// <summary>
        /// токен игрока
        /// </summary>
        public virtual string ToTokenPlayer() => "";

        #region StartStopPause

        /// <summary>
        /// Запуск игры
        /// </summary>
        public virtual void GameStarting() => stopwatch.Start();

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
            window.LScreen.Close();
            flagTick = true;
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
            packets.Update();

            if (flagTick && !IsGamePaused)
            {
                tickCounterClient++;
                TickCounter++;

                // Тут надо указать 60 секунд
                // Прошла минута, или 1200 тактов
                if (tickCounterClient % 1200 == 0)
                {
                    Log.Save();
                }

                // Тут надо указать 10 секунд
               // if (tickCounterClient % 20 == 0)
                {
                    // Раз в 10 секунды перепинговка
                    TrancivePacket(new Packet00PingPong(Time()));
                }
            }
            // Тест
         //   TrancivePacket(new PacketC04PlayerPosition(new System.Numerics.Vector3(1, 2.5f, 3.5f), true, false, true));
        }

        /// <summary>
        /// Получить время в милисекундах с момента запуска проекта
        /// </summary>
        public long Time() => stopwatch.ElapsedMilliseconds;

        /// <summary>
        /// Задать время пинга
        /// </summary>
        public void SetPing(long time)
        {
            lastTimeServer = Time();
            Ping = (Ping * 3 + (int)(lastTimeServer - time)) / 4;
        }

        /// <summary>
        /// Проверка времени игрока без пинга, если игрок не отвечал больше 30 секунд
        /// </summary>
        public bool TimeOut() => (Time() - lastTimeServer) > 3000;//30000;

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
            return string.Format("{0} ping: {1} ms Traffic: {3}{2} Tick {4}",
                IsLoacl ? "Local" : "Net",
                Ping,
                IsGamePaused ? " Pause" : "",
                ToTraffic(),
                TickCounter);
        }

        private string ToTraffic()
        {
            long traffic = packets.Traffic;
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
        protected void OnStoped(string notification, bool isWarning)
        {
            if (!flagStoped)
            {
                // флаг нужен, так-как можно попасть сюда много раз, из-за разрыва сети.
                flagStoped = true;
                Log.Client(Srl.StoppedClient, notification == "" ? "" : " [" + notification + "]");
                Log.Save();
                Stoped?.Invoke(this, new GameStopEventArgs(notification, isWarning));
            }
        }

        /// <summary>
        /// Событие ошибки на сервере
        /// </summary>
        public event ThreadExceptionEventHandler Error;
        protected void OnError(Exception ex)
            => Error?.Invoke(this, new ThreadExceptionEventArgs(ex));

        /// <summary>
        /// Событие текст отладки сервера
        /// </summary>
        public event StringEventHandler ServerTextDebug;
        protected virtual void OnServerTextDebug(string text) 
            => ServerTextDebug?.Invoke(this, new StringEventArgs(text));

        #endregion
    }
}
