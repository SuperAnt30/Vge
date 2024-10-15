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
    /// <summary>
    /// Объект игрового сервера
    /// </summary>
    public class GameServer
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
        private readonly Stopwatch _stopwatchTps = new Stopwatch();
        /// <summary>
        /// Для перевода тактов в мили секунды Stopwatch.Frequency / 1000;
        /// </summary>
        private readonly long _frequencyMs;
        /// <summary>
        /// Устанавливается при появлении предупреждения «Не могу угнаться», 
        /// которое срабатывает снова через 15 секунд. 
        /// </summary>
        private long _timeOfLastWarning;
        /// <summary>
        /// Хранение тактов за 1/5 секунды игры, для статистики определения среднего времени такта
        /// </summary>
        private readonly long[] _tickTimeArray = new long[Ce.Tps];
        /// <summary>
        /// Пауза в игре, только для одиночной версии
        /// </summary>
        private bool _isGamePaused = false;
        /// <summary>
        /// Флаг уже в loopе сервера, false - елё ещё не дошли до loop
        /// </summary>
        private bool _flagInLoop = false;
        /// <summary>
        /// Порт для режима сервера без владельца
        /// </summary>
        private int _portToServer = 32021;

        /// <summary>
        /// Принято пакетов за секунду
        /// </summary>
        private int _rx = 0;
        /// <summary>
        /// Передано пакетов за секунду
        /// </summary>
        private int _tx = 0;
        /// <summary>
        /// Принято пакетов за предыдущую секунду
        /// </summary>
        private int _rxPrev = 0;
        /// <summary>
        /// Передано пакетов за предыдущую секунду
        /// </summary>
        private int _txPrev = 0;
        /// <summary>
        /// статус запуска сервера
        /// </summary>
        private string _strNet = "";
        /// <summary>
        /// Сокет для сети
        /// </summary>
        private SocketServer _socketServer;
        /// <summary>
        /// Объект работы с пакетами
        /// </summary>
        private ProcessServerPackets _packets;

        /// <summary>
        /// Счётчик зарегистрированных сущностей с начала запуска игры
        /// </summary>
        private int _lastEntityId = 0;

        public GameServer(Logger log, GameSettings gameSettings, AllWorlds worlds)
        {
            Ce.InitServer();
            Settings = gameSettings;
            Log = log;
            Filer = new Profiler(Log, "[Server] ");
            _packets = new ProcessServerPackets(this);
            Players = new PlayerManager(this);
            Worlds = worlds;
            
            _frequencyMs = Stopwatch.Frequency / 1000;
            _stopwatchTps.Start();
        }

        /// <summary>
        /// Инициализация, если true то разрешение на запуск
        /// </summary>
        public bool Init()
        {
            bool flag = Settings.Init(this);
            Worlds.Init(this);
            return flag;
        }

        #region StartStopPause

        /// <summary>
        /// Запустить сервер в отдельном потоке для сервера без владельца игрока
        /// </summary>
        public void Starting(int port)
        {
            _portToServer = port;
            Log.Server(Srl.Starting, Settings.Slot, Ce.IndexVersion);
            Thread myThread = new Thread(_Loop) { Name = "ServerLoop" };
            myThread.Start();
        }

        /// <summary>
        /// Запустить сервер в отдельном потоке с игроком владельцем
        /// </summary>
        public void Starting(string login, string token)
        {
            Players.PlayerOwnerAdd(login, token);
            Log.Server(Srl.Starting, Settings.Slot, Ce.IndexVersion);
            Thread myThread = new Thread(_Loop) { Name = "ServerLoop" };
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
            _isGamePaused = !IsRunNet() && value;
            if (Ce.IsDebugDraw) _OnTextDebug();
            return _isGamePaused;
        }

        #endregion

        #region Net

        /// <summary>
        /// Получить истину запущена ли сеть
        /// </summary>
        public bool IsRunNet() => _socketServer != null && _socketServer.IsConnected();

        /// <summary>
        /// Запустить на сервере сеть
        /// </summary>
        public void RunNet(int port)
        {
            if (!IsRunNet())
            {
                _socketServer = new SocketServer(port);
                _socketServer.ReceivePacket += _SocketServer_ReceivePacket;
                _socketServer.Stopped += _SocketServer_Stopped;
                _socketServer.Runned += _SocketServer_Runned;
                _socketServer.Error += _SocketServer_Error;
                _socketServer.UserJoined += _SocketServer_UserJoined;
                _socketServer.UserLeft += _SocketServer_UserLeft;
                _socketServer.WarningString += _SocketServer_WarningString;
                if (!_socketServer.Run())
                {
                    _socketServer = null;
                }
            }
        }

        private void _SocketServer_WarningString(object sender, StringEventArgs e)
        {
            Log.Server(e.Text);
            _OnRecieveMessage(e.Text);
        }

        /// <summary>
        /// Пользователь присоединился
        /// </summary>
        private void _SocketServer_UserJoined(object sender, PacketStringEventArgs e)
        {
            Log.Server(Srl.ConnectedToTheServer, e.Side.ToString());
            if (_flagInLoop)
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

        private void _SocketServer_UserLeft(object sender, PacketStringEventArgs e)
        {
            Players.PlayerRemove(e.Side, e.Cause);
            Log.Server(Srl.DisconnectedFromServer, e.Cause, e.Side.ToString());
        }
            

        private void _SocketServer_Error(object sender, ErrorEventArgs e)
        {
            Log.Error(e.GetException());
            // В краш улетит
            _OnError(e.GetException());
        }

        private void _SocketServer_Runned(object sender, EventArgs e)
        {
            _isGamePaused = false;
            Log.Server(Srl.NetworkModeIsEnabled);
        }

        private void _SocketServer_Stopped(object sender, EventArgs e)
        {
            _socketServer = null;
            if (IsServerRunning)
            {
                // Если сеть остановилась, а луп нет, 
                // запускаем остановку лупа и последующее завершение
                Stop();
            }
            else
            {
                // Если луп уже остановлен, то сразу к закрытию
                _Stoped();
            }
        }

        private void _SocketServer_ReceivePacket(object sender, PacketBufferEventArgs e)
        {
            LocalReceivePacket(e.Side, e.Buffer.bytes);
        }

        /// <summary>
        /// Локальная передача пакета
        /// </summary>
        public void LocalReceivePacket(SocketSide socketClient, byte[] buffer)
        {
            _rx++;
            _packets.ReceiveBuffer(socketClient, buffer);
        }

        /// <summary>
        /// Обновить количество клиентов
        /// </summary>
        public void UpCountClients()
        {
            _strNet = Players.PlayerOwner == null ? "<S>" : Players.PlayerOwner.Login;
            if (IsRunNet())
            {
                _strNet += " net[" + Players.PlayerNetCount() + "] " + Players.ToStringPlayersNet();
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
                _tx++;
                socketClient.SendPacket(packet);
            }
        }

        /// <summary>
        /// Отправить пакет владельцу
        /// </summary>
        public void ResponsePacketOwner(IPacket packet)
        {
            _tx++;
            _OnRecievePacket(new PacketBufferEventArgs(new PacketBuffer(WritePacket.TranciveToArray(packet)), null));
        }

        /// <summary>
        /// Разорвать соединение с игроком
        /// </summary>
        public void PlayerDisconnect(SocketSide socketSide, string cause)
            => _socketServer.DisconnectHandler(socketSide, cause);

        #endregion

        #region Loop & Tick

        /// <summary>
        /// До игрового цикла
        /// </summary>
        private void _ToLoop()
        {
            if (Players.PlayerOwner != null)
            {
                // Если игрок владелец имеется, то грузим мир под него
                // Тут должен знать его Ник, и место спавна
                // Отправляем количество шагов на загрузку

                WorldServer world = Players.PlayerOwner.GetWorld();
                int cpx = Players.PlayerOwner.ChunkPositionX;
                int cpy = Players.PlayerOwner.ChunkPositionY;
                int radius = Players.PlayerOwner.ActiveRadius + FragmentManager.AddOverviewChunkServer;
                int step = (radius + radius + 1) * (radius + radius + 1);
                ResponsePacketOwner(new PacketS02LoadingGame((ushort)step));
                // Запуск чанков (шагов)
                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        world.ChunkPrServ.NeededChunk(cpx + x, cpy + y, false);
                        ResponsePacketOwner(new PacketS02LoadingGame(PacketS02LoadingGame.EnumStatus.Step));
                    }
                }
                if (Ce.IsDebugDraw && Ce.IsDebugDrawChunks)
                {
                    OnTagDebug(Debug.Key.ChunkReady.ToString(), world.ChunkPr.GetListDebug());
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
                RunNet(_portToServer);
            }
        }

        /// <summary>
        /// Метод должен работать в отдельном потоке
        /// </summary>
        private void _Loop()
        {
            try
            {
                _ToLoop();
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

                currentTime = endTime = _stopwatchTps.ElapsedMilliseconds;
                endTicks = _stopwatchTps.ElapsedTicks;

                // Меняем флаг
                _flagInLoop = true;

                // Рабочий цикл сервера
                while (IsServerRunning)
                {
                    beginTime = _stopwatchTps.ElapsedMilliseconds;
                    // Разница в милсекунда с прошлого такта
                    differenceTime = beginTime - currentTime;
                    if (differenceTime < 0) differenceTime = 0;

                    // Если выше 2 секунд задержка
                    if (differenceTime > 2000 && currentTime - _timeOfLastWarning >= 15000)
                    {
                        // Не успеваю!Изменилось ли системное время, или сервер перегружен?
                        // Отставание на {differenceTime} мс, пропуск тиков({differenceTime / 50}) 
                        Log.Server(Srl.LaggingBehind, differenceTime, differenceTime / 50);
                        differenceTime = 2000;
                        _timeOfLastWarning = currentTime;
                    }

                    accumulatedTime += differenceTime;
                    currentTime = beginTime;

                    while (IsServerRunning && accumulatedTime > Ce.Tick​​Time)
                    {
                        accumulatedTime -= Ce.Tick​​Time;
                        // Счётчик реального времени
                        beginTime = _stopwatchTps.ElapsedMilliseconds;
                        // фиксируем начальное время такта
                        beginTicks = _stopwatchTps.ElapsedTicks;

                        if (!_isGamePaused)
                        {
                            // Счётчик тиков
                            TickCounter++;
                            // Находим дельту времени между тактами
                            DeltaTime = (beginTicks - endTicks) / (double)_frequencyMs;
                            // Добавить время сколько прошло с прошлого шага в мс
                            TimeCounter += beginTime - endTime;

                            // =======
                            _OnTick();
                            // =======

                            // фиксируем время выполнения такта
                            timeExecutionTicks = _stopwatchTps.ElapsedTicks - beginTicks;
                            // фиксируем время выполнения такта
                            _tickTimeArray[TickCounter % Ce.Tps] = timeExecutionTicks;
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
                _OnError(ex);
            }
            finally
            {
                _Stoping();
            }
        }

        /// <summary>
        /// Сохраняет все необходимые данные для подготовки к остановке сервера
        /// </summary>
        private void _Stoping()
        {
            Log.Server(Srl.Stopping);

            // Надо сохранить мир
            Worlds.Stoping();
            // Чистим все пакеты, так-как уже обрабатывать не будем
            _packets.Clear();
            // Выкидываем всех игроков
            Players.PlayersRemoveStopingServer();
            // Сохранить игру
            GameFile.Write(Settings, this);

            if (_socketServer != null && _socketServer.IsConnected())
            {
                // Останавливаем сокет
                _socketServer.Stop();
            }
            else
            {
                _Stoped();
            }
        }

        /// <summary>
        /// Закрыт луп и сокет
        /// </summary>
        private void _Stoped()
        {
            Log.Server(Srl.StoppedServer);
            _OnCloseded();
        }

        /// <summary>
        /// Игровой тик
        /// </summary>
        private void _OnTick()
        {
            // Сетевые пакеты
            _packets.Update();

            // Прошла секунда
            if (TickCounter % Ce.Tps == 0)
            {
                UpCountClients();

                if (TickCounter % 600 == 0)
                {
                    // раз в 30 секунд обновляем тик с клиентом
                    Players.SendToAll(new PacketS04TimeUpdate(TickCounter));
                }

                _rxPrev = _rx;
                _txPrev = _tx;
                _rx = 0;
                _tx = 0;
            }

            // Тики менеджера игроков
            Filer.StartSection("PlayersTick");
            Players.Update();
            Filer.EndSection();
            
            //Thread.Sleep(10);
            // Тут игровые мировые тики
            Worlds.Update();

            // Прошла 1/3 секунда, или 10 тактов
            if (Ce.IsDebugDraw && TickCounter % 10 == 0)
            {
                // лог статистика за это время
                _OnTextDebug();
            }
        }

        /// <summary>
        /// Получить время в милисекундах с момента запуска проекта
        /// </summary>
        public long Time() => _stopwatchTps.ElapsedMilliseconds;

        #endregion

        #region Get

        /// <summary>
        /// Счётчик зарегистрированных сущностей с начала запуска игры
        /// </summary>
        public int LastEntityId() => ++_lastEntityId;

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
        private string _ToStringDebugTps()
        {
            // Среднее время выполнения 4 тактов, должно быть меньше 50
            float averageTime = Mth.Average(_tickTimeArray) / _frequencyMs;
            // TPS за последние 4 тактов (1/5 сек), должен быть 20
            float tps = averageTime > Ce.Tick​​Time ? Ce.Tick​​Time / averageTime * Ce.Tps : Ce.Tps;
            return string.Format("[Server]: {0:0.00} tps {1:0.00} ms Rx {2} Tx {3} Tick {4} Time {5:0.0} s {6}"
                + Ce.Br + Worlds.ToString()
                + "Owner: " + Players.PlayerOwner.ToString()
                + Ce.Br + _strNet
                + Ce.Br + debugText,
                tps, averageTime, _rxPrev, _txPrev, TickCounter, TimeCounter / 1000f, // 0-5
                _isGamePaused ? " PAUSE" : "" // 6
                );
        }

        public string debugText = "";

        #endregion

        #region Event

        /// <summary>
        /// Событие ошибки на сервере
        /// </summary>
        public event ThreadExceptionEventHandler Error;
        private void _OnError(Exception ex)
            => Error?.Invoke(this, new ThreadExceptionEventArgs(ex));

        /// <summary>
        /// Событие закрыть
        /// </summary>
        public event EventHandler Closeded;
        private void _OnCloseded() => Closeded?.Invoke(this, new EventArgs());

        /// <summary>
        /// Событие текст для отладки
        /// </summary>
        public event StringEventHandler TextDebug;
        private void _OnTextDebug() 
            => TextDebug?.Invoke(this, new StringEventArgs(_ToStringDebugTps()));

        /// <summary>
        /// Событие любого объекта с сервера для отладки
        /// </summary>
        public event StringEventHandler TagDebug;
        public void OnTagDebug(string title, object tag)
            => TagDebug?.Invoke(this, new StringEventArgs(title, tag));

        /// <summary>
        /// Событие получить от сервера пакет
        /// </summary>
        public event PacketBufferEventHandler RecievePacket;
        private void _OnRecievePacket(PacketBufferEventArgs e) 
            => RecievePacket?.Invoke(this, e);

        /// <summary>
        /// Событие получить от сервера сообщение
        /// </summary>
        public event StringEventHandler RecieveMessage;
        private void _OnRecieveMessage(string message)
            => RecieveMessage?.Invoke(this, new StringEventArgs(message));

        #endregion
    }
}
