using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Vge.Event;
using Vge.Management;
using Vge.Network;
using Vge.Network.Packets.Server;
using Vge.Util;
using Vge.World;
using WinGL.Util;

namespace Vge.Games
{
    public class Server
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
        /// Объект управления пользователями на сервере
        /// </summary>
        public readonly PlayerManager Players;

        /// <summary>
        /// Указывает, запущен сервер или нет. Установите значение false, 
        /// чтобы инициировать завершение работы. 
        /// </summary>
        public bool IsServerRunning { get; private set; } = true;
        /// <summary>
        /// Увеличивается каждый такт 
        /// </summary>
        public uint TickCounter { get; private set; }
        /// <summary>
        /// Увеличивается время каждый такт, время в мс
        /// </summary>
        public long TimeCounter { get; private set; }
        /// <summary>
        /// Дельта последнего тика в mc
        /// </summary>
        public double DeltaTime { get; private set; }
        /// <summary>
        /// Настройки игры
        /// </summary>
        public readonly GameSettings Settings;
        /// <summary>
        /// Миры игры
        /// </summary>
        public readonly AllWorlds Worlds;

        /// <summary>
        /// Часы для Tps
        /// </summary>
        private readonly Stopwatch stopwatchTps = new Stopwatch();
        /// <summary>
        /// Для перевода тактов в мили секунды Stopwatch.Frequency / 1000;
        /// </summary>
        private readonly long frequencyMs;
        /// <summary>
        /// Устанавливается при появлении предупреждения «Не могу угнаться», 
        /// которое срабатывает снова через 15 секунд. 
        /// </summary>
        private long timeOfLastWarning;
        /// <summary>
        /// Хранение тактов за 1/5 секунды игры, для статистики определения среднего времени такта
        /// </summary>
        private readonly long[] tickTimeArray = new long[Ce.Tps];
        /// <summary>
        /// Пауза в игре, только для одиночной версии
        /// </summary>
        private bool isGamePaused = false;
        /// <summary>
        /// Флаг уже в loopе сервера, false - елё ещё не дошли до loop
        /// </summary>
        private bool flagInLoop = false;
        /// <summary>
        /// Порт для режима сервера без владельца
        /// </summary>
        private int portToServer = 32021;

        /// <summary>
        /// Принято пакетов за секунду
        /// </summary>
        private int rx = 0;
        /// <summary>
        /// Передано пакетов за секунду
        /// </summary>
        private int tx = 0;
        /// <summary>
        /// Принято пакетов за предыдущую секунду
        /// </summary>
        private int rxPrev = 0;
        /// <summary>
        /// Передано пакетов за предыдущую секунду
        /// </summary>
        private int txPrev = 0;
        /// <summary>
        /// статус запуска сервера
        /// </summary>
        private string strNet = "";
        /// <summary>
        /// Сокет для сети
        /// </summary>
        private SocketServer socketServer;
        /// <summary>
        /// Объект работы с пакетами
        /// </summary>
        private ProcessServerPackets packets;

        /// <summary>
        /// Счётчик зарегистрированных сущностей с начала запуска игры
        /// </summary>
        private int lastEntityId = 0;

        public Server(Logger log, GameSettings gameSettings, AllWorlds worlds)
        {
            Settings = gameSettings;
            Log = log;
            Filer = new Profiler(Log, "[Server] ");
            packets = new ProcessServerPackets(this);
            Players = new PlayerManager(this);
            Worlds = worlds;
            Worlds.Init(this);
            frequencyMs = Stopwatch.Frequency / 1000;
            stopwatchTps.Start();
        }

        #region StartStopPause

        /// <summary>
        /// Запустить сервер в отдельном потоке для сервера без владельца игрока
        /// </summary>
        public void Starting(int port)
        {
            portToServer = port;
            Log.Server(Srl.Starting, Settings.Seed, Ce.IndexVersion);
            Thread myThread = new Thread(Loop) { Name = "ServerLoop" };
            myThread.Start();
        }

        /// <summary>
        /// Запустить сервер в отдельном потоке с игроком владельцем
        /// </summary>
        public void Starting(string login, string token)
        {
            Players.PlayerOwnerAdd(login, token);
            Log.Server(Srl.Starting, Settings.Seed, Ce.IndexVersion);
            Thread myThread = new Thread(Loop) { Name = "ServerLoop" };
            myThread.Start();
        }

        /// <summary>
        /// Запрос остановки сервера
        /// </summary>
        public void Stop() => IsServerRunning = false;

        /// <summary>
        /// Задать паузу для одиночной игры
        /// </summary>
        public bool SetGamePauseSingle(bool value)
        {
            isGamePaused = !IsRunNet() && value;
            OnTextDebug();
            return isGamePaused;
        }

        #endregion

        #region Net

        /// <summary>
        /// Получить истину запущена ли сеть
        /// </summary>
        public bool IsRunNet() => socketServer != null && socketServer.IsConnected();

        /// <summary>
        /// Запустить на сервере сеть
        /// </summary>
        public void RunNet(int port)
        {
            if (!IsRunNet())
            {
                socketServer = new SocketServer(port);
                socketServer.ReceivePacket += SocketServer_ReceivePacket;
                socketServer.Stopped += SocketServer_Stopped;
                socketServer.Runned += SocketServer_Runned;
                socketServer.Error += SocketServer_Error;
                socketServer.UserJoined += SocketServer_UserJoined;
                socketServer.UserLeft += SocketServer_UserLeft;
                socketServer.WarningString += SocketServer_WarningString;
                if (!socketServer.Run())
                {
                    socketServer = null;
                }
            }
        }

        private void SocketServer_WarningString(object sender, StringEventArgs e)
        {
            Log.Server(e.Text);
            OnRecieveMessage(e.Text);
        }

        /// <summary>
        /// Пользователь присоединился
        /// </summary>
        private void SocketServer_UserJoined(object sender, PacketStringEventArgs e)
        {
            Log.Server(Srl.ConnectedToTheServer, e.Side.ToString());
            if (flagInLoop)
            {
                // Отправляем ему его id и uuid
                ResponsePacket(e.Side, new PacketS02LoadingGame(PacketS02LoadingGame.EnumStatus.BeginNet));
            }
            else
            {
                // Если мы не дошли до loop то предлагаем разорвать сокет, для повторного соединения
                PlayerDisconnect(e.Side, Sr.TheGameHasntStartedYet);
            }
        }

        private void SocketServer_UserLeft(object sender, PacketStringEventArgs e)
        {
            Players.PlayerRemove(e.Side, e.Cause);
            Log.Server(Srl.DisconnectedFromServer, e.Cause, e.Side.ToString());
        }
            

        private void SocketServer_Error(object sender, ErrorEventArgs e)
        {
            Log.Error(e.GetException());
            // В краш улетит
            OnError(e.GetException());
        }

        private void SocketServer_Runned(object sender, EventArgs e)
        {
            isGamePaused = false;
            Log.Server(Srl.NetworkModeIsEnabled);
        }

        private void SocketServer_Stopped(object sender, EventArgs e)
        {
            socketServer = null;
            if (IsServerRunning)
            {
                // Если сеть остановилась, а луп нет, 
                // запускаем остановку лупа и последующее завершение
                Stop();
            }
            else
            {
                // Если луп уже остановлен, то сразу к закрытию
                Stoped();
            }
        }

        private void SocketServer_ReceivePacket(object sender, PacketBufferEventArgs e)
        {
            LocalReceivePacket(e.Side, e.Buffer.bytes);
        }

        /// <summary>
        /// Локальная передача пакета
        /// </summary>
        public void LocalReceivePacket(SocketSide socketClient, byte[] buffer)
        {
            rx++;
            packets.ReceiveBuffer(socketClient, buffer);
        }

        /// <summary>
        /// Обновить количество клиентов
        /// </summary>
        public void UpCountClients()
        {
            strNet = Players.PlayerOwner == null ? "<S>" : Players.PlayerOwner.Login;
            if (IsRunNet())
            {
                strNet += " net[" + Players.PlayerNetCount() + "] " + Players.ToStringPlayersNet();
            }
        }

        /// <summary>
        /// Отправить пакет сетевому клиенту,
        /// Если SocketSide null то владельцу
        /// </summary>
        public void ResponsePacket(SocketSide socketClient, IPacket packet)
        {
            if (socketClient == null)
            {
                ResponsePacketOwner(packet);
            }
            else
            {
                tx++;
                socketClient.SendPacket(packet);
            }
        }

        /// <summary>
        /// Отправить пакет владельцу
        /// </summary>
        public void ResponsePacketOwner(IPacket packet)
        {
            tx++;
            OnRecievePacket(new PacketBufferEventArgs(new PacketBuffer(WritePacket.TranciveToArray(packet)), null));
        }

        /// <summary>
        /// Разорвать соединение с игроком
        /// </summary>
        public void PlayerDisconnect(SocketSide socketSide, string cause)
            => socketServer.DisconnectHandler(socketSide, cause);

        #endregion

        #region Loop & Tick

        /// <summary>
        /// До игрового цикла
        /// </summary>
        private void ToLoop()
        {
            if (Players.PlayerOwner != null)
            {
                // Если игрок владелец имеется, то грузим мир под него
                // Тут должен знать его Ник, и место спавна
                // Отправляем количество шагов на загрузку
                ushort step = 100;// 625;
                ResponsePacketOwner(new PacketS02LoadingGame(step));
                // Запуск чанков (шагов)
                for (int i = 0; i < step; i++)
                {
                    ResponsePacketOwner(new PacketS02LoadingGame(PacketS02LoadingGame.EnumStatus.Step));
                }
                // Загрузка закончена, последний штрих передаём id игрока и его uuid
                Players.JoinGameOwner();
            }
            else
            {
                // Это для сервера без игроков, запускаемся
                // Убираем скрин загрузки
                ResponsePacketOwner(new PacketS02LoadingGame(PacketS02LoadingGame.EnumStatus.ServerGo));
                // Сразу включаем порт сети
                RunNet(portToServer);
            }
        }

        /// <summary>
        /// Метод должен работать в отдельном потоке
        /// </summary>
        private void Loop()
        {
            try
            {
                ToLoop();
                Log.Server(Srl.Go);

                // Начальное время мс
                long beginTime;
                // Текущее время для расчёта сна потока
                long currentTime;
                // Накопленное время для определении сна
                long accumulatedTime = 0;
                // Время сна потока, в мс
                int timeSleep;
                // Разница времени для такта
                long differenceTime;

                // Начальное время такта
                long beginTicks;
                // время выполнения такта
                long timeExecutionTicks;

                // Для фиксации конца времени в тактах
                long endTicks;
                // Для фиксации конца времени а мс
                long endTime;

                currentTime = endTime = stopwatchTps.ElapsedMilliseconds;
                endTicks = stopwatchTps.ElapsedTicks;

                // Меняем флаг
                flagInLoop = true;

                // Рабочий цикл сервера
                while (IsServerRunning)
                {
                    beginTime = stopwatchTps.ElapsedMilliseconds;
                    // Разница в милсекунда с прошлого такта
                    differenceTime = beginTime - currentTime;
                    if (differenceTime < 0) differenceTime = 0;

                    // Если выше 2 секунд задержка
                    if (differenceTime > 2000 && currentTime - timeOfLastWarning >= 15000)
                    {
                        // Не успеваю!Изменилось ли системное время, или сервер перегружен?
                        // Отставание на {differenceTime} мс, пропуск тиков({differenceTime / 50}) 
                        Log.Server(Srl.LaggingBehind, differenceTime, differenceTime / 50);
                        differenceTime = 2000;
                        timeOfLastWarning = currentTime;
                    }

                    accumulatedTime += differenceTime;
                    currentTime = beginTime;

                    while (IsServerRunning && accumulatedTime > Ce.Tick​​Time)
                    {
                        accumulatedTime -= Ce.Tick​​Time;
                        // Счётчик реального времени
                        beginTime = stopwatchTps.ElapsedMilliseconds;
                        // фиксируем начальное время такта
                        beginTicks = stopwatchTps.ElapsedTicks;

                        if (!isGamePaused)
                        {
                            // Счётчик тиков
                            TickCounter++;
                            // Находим дельту времени между тактами
                            DeltaTime = (beginTicks - endTicks) / (double)frequencyMs;
                            // Добавить время сколько прошло с прошлого шага в мс
                            TimeCounter += beginTime - endTime;

                            // =======
                            OnTick();
                            // =======

                            // фиксируем время выполнения такта
                            timeExecutionTicks = stopwatchTps.ElapsedTicks - beginTicks;
                            // фиксируем время выполнения такта
                            tickTimeArray[TickCounter % Ce.Tps] = timeExecutionTicks;
                        }

                        // Разница времени для счётчика времени обновляем, даже если пауза
                        endTime = beginTime;
                        // фиксируем конечное время
                        endTicks = beginTicks;
                    }
                    timeSleep = Ce.Tick​​Time - (int)accumulatedTime;
                    Thread.Sleep(timeSleep > 0 ? timeSleep : 1);
                }
            }
            catch (Exception ex)
            {
                IsServerRunning = false;
                OnError(ex);
            }
            finally
            {
                Stoping();
            }
        }

        /// <summary>
        /// Сохраняет все необходимые данные для подготовки к остановке сервера
        /// </summary>
        private void Stoping()
        {
            Log.Server(Srl.Stopping);

            // Надо сохранить мир
            Worlds.Stoping();
            // Чистим все пакеты, так-как уже обрабатывать не будем
            packets.Clear();
            // Выкидываем всех игроков
            Players.PlayersRemoveStopingServer();
            // Сохранить игру
            GameFile.Write(Settings, this);

            if (socketServer != null && socketServer.IsConnected())
            {
                // Останавливаем сокет
                socketServer.Stop();
            }
            else
            {
                Stoped();
            }
        }

        /// <summary>
        /// Закрыт луп и сокет
        /// </summary>
        private void Stoped()
        {
            Log.Server(Srl.StoppedServer);
            OnCloseded();
        }

        /// <summary>
        /// Игровой тик
        /// </summary>
        private void OnTick()
        {
            // Сетевые пакеты
            packets.Update();

            // Прошла секунда
            if (TickCounter % Ce.Tps == 0)
            {
                UpCountClients();

                if (TickCounter % 600 == 0)
                {
                    // раз в 30 секунд обновляем тик с клиентом
                    Players.SendToAll(new PacketS04TimeUpdate(TickCounter));
                }

                rxPrev = rx;
                txPrev = tx;
                rx = 0;
                tx = 0;
            }

            // Тики менеджера игроков
            Filer.StartSection("PlayersTick");
            Players.Update();

            Filer.EndSection();
            
            //Thread.Sleep(10);
            // Тут игровые мировые тики
            Worlds.Update();

            // Прошла 1/3 секунда, или 10 тактов
            if (TickCounter % 10 == 0)
            {
                // лог статистика за это время
                OnTextDebug();
            }
        }

        /// <summary>
        /// Получить время в милисекундах с момента запуска проекта
        /// </summary>
        public long Time() => stopwatchTps.ElapsedMilliseconds;

        #endregion

        #region Get

        /// <summary>
        /// Счётчик зарегистрированных сущностей с начала запуска игры
        /// </summary>
        public int LastEntityId() => ++lastEntityId;

        #endregion

        #region Set

        /// <summary>
        /// Задать время с загрузки файла
        /// </summary>
        public void SetDataFile(long time, uint tick)
        {
            TickCounter = tick;
            TimeCounter = time;
        }

        #endregion

        #region Debug

        /// <summary>
        /// Строка для дебага, формируется по запросу
        /// </summary>
        private string ToStringDebugTps()
        {
            // Среднее время выполнения 4 тактов, должно быть меньше 50
            float averageTime = Mth.Average(tickTimeArray) / frequencyMs;
            // TPS за последние 4 тактов (1/5 сек), должен быть 20
            float tps = averageTime > Ce.Tick​​Time ? Ce.Tick​​Time / averageTime * Ce.Tps : Ce.Tps;
            return string.Format("Server: {0:0.00} tps {1:0.00} ms Rx {2} Tx {3} Tick {4} Time {5:0.0} s {7}" 
                + Ce.Br + "{6}" 
                + Ce.Br + "{8}",
                tps, averageTime, rxPrev, txPrev, TickCounter, TimeCounter / 1000f, // 0-5
                strNet, isGamePaused ? " PAUSE" : "", // 6-7
                debugText); // 8
        }

        public string debugText = "";

        #endregion

        #region Event

        /// <summary>
        /// Событие ошибки на сервере
        /// </summary>
        public event ThreadExceptionEventHandler Error;
        private void OnError(Exception ex)
            => Error?.Invoke(this, new ThreadExceptionEventArgs(ex));

        /// <summary>
        /// Событие закрыть
        /// </summary>
        public event EventHandler Closeded;
        private void OnCloseded() => Closeded?.Invoke(this, new EventArgs());

        /// <summary>
        /// Событие текст для отладки
        /// </summary>
        public event StringEventHandler TextDebug;
        private void OnTextDebug() 
            => TextDebug?.Invoke(this, new StringEventArgs(ToStringDebugTps()));

        /// <summary>
        /// Событие получить от сервера пакет
        /// </summary>
        public event PacketBufferEventHandler RecievePacket;
        private void OnRecievePacket(PacketBufferEventArgs e) 
            => RecievePacket?.Invoke(this, e);

        /// <summary>
        /// Событие получить от сервера сообщение
        /// </summary>
        public event StringEventHandler RecieveMessage;
        private void OnRecieveMessage(string message)
            => RecieveMessage?.Invoke(this, new StringEventArgs(message));

        #endregion
    }
}
