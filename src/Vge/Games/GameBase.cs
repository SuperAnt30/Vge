using System;
using System.Diagnostics;
using System.Threading;
using Vge.Event;
using Vge.Network;
using Vge.Network.Packets.Client;

namespace Vge.Games
{
    /// <summary>
    /// Абстрактный класс игры
    /// </summary>
    public abstract class GameBase
    {
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
        /// Счётчик тиков без синхронизации с сервером, отсчёт от запуска программы
        /// </summary>
        protected uint tickCounterClient = 0;

        /// <summary>
        /// Объект времени с момента запуска проекта
        /// </summary>
        private Stopwatch stopwatch = new Stopwatch();
        /// <summary>
        /// Последнее время пинга в милисекундах
        /// </summary>
        private long lastTimeServer;
        /// <summary>
        /// Объект работы с пакетами
        /// </summary>
        protected ProcessClientPackets packets;

        /// <summary>
        /// Запуск игры
        /// </summary>
        public virtual void GameStarting()
        {
            stopwatch.Start();
            packets = new ProcessClientPackets(this);
        }

        /// <summary>
        /// Остановка игры
        /// </summary>
        public virtual void GameStoping(string notification = "") { }

        /// <summary>
        /// Задать паузу для одиночной игры
        /// </summary>
        public virtual void SetGamePauseSingle(bool value) { }

        /// <summary>
        /// Отправить пакет на сервер
        /// </summary>
        public virtual void TrancivePacket(IPacket packet) { }

        /// <summary>
        /// Получить от сервера пакет
        /// </summary>
        protected void RecievePacket(ServerPacketEventArgs e)
        {
            packets.ReceiveBuffer(e.Packet.bytes);
        }

        /// <summary>
        /// Игровой такт
        /// </summary>
        public virtual void OnTick()
        {
            tickCounterClient++;

            // почерёдно получаем пакеты с сервера
            packets.Update();

            if (tickCounterClient % 40 == 0)
            {
                // Раз в 2 секунды перепинговка
                TrancivePacket(new PacketC00Ping(Time()));
            }
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
        public bool TimeOut() => (Time() - lastTimeServer) > 30000;

        public override string ToString()
        {
            return string.Format("{0} ping: {1} ms{2}",
                IsLoacl ? "Local" : "Net", Ping, IsGamePaused ? " Pause" : "");
        }

        #region Event

        /// <summary>
        /// Событие остановлена игра
        /// </summary>
        public event StringEventHandler Stoped;
        protected void OnStoped(string notification = "") 
            => Stoped?.Invoke(this, new StringEventArgs(notification));

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

        /// <summary>
        /// Событие одного игрового такта
        /// </summary>
        public event EventHandler Tick;
        public void OnTick2() => Tick?.Invoke(this, new EventArgs());

        #endregion
    }
}
