using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Vge.Actions;
using Vge.Event;
using Vge.Management;
using Vge.Network;
using Vge.Network.Packets;
using Vge.Network.Packets.Server;
using Vge.Renderer.Huds;
using Vge.Renderer.World;
using Vge.Util;
using Vge.World;
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
        /// Объект нажатия клавиатуры
        /// </summary>
        public readonly Keyboard Key;

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
        public readonly PlayerClientOwner Player;
        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient World { get; private set; }
        /// <summary>
        /// Рендер мира
        /// </summary>
        public readonly WorldRenderer WorldRender;
        /// <summary>
        /// Список игроков во всех мирах, для чата 
        /// </summary>
        public readonly Dictionary<int, string> Players = new Dictionary<int, string>();

        /// <summary>
        /// Увеличивается каждый тик 
        /// </summary>
        public uint TickCounter { get; private set; } = 0;
        /// <summary>
        /// Флаг запущена ли игра, надо для сервера
        /// </summary>
        public bool FlagGameStarted { get; private set; }

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
        /// Объект работы с пакетами
        /// </summary>
        protected readonly ProcessClientPackets _packets;

        /// <summary>
        /// Объект времени с момента запуска проекта
        /// </summary>
        private readonly Stopwatch _stopwatch = new Stopwatch();
       
        /// <summary>
        /// Флаг было ли уже остановление
        /// </summary>
        private bool _flagStoped = false;
        /// <summary>
        /// Имеется ли упровление мышкой вида от первого лица
        /// </summary>
        private bool _isMouseFirstPersonView;
        /// <summary>
        /// Зажата ли левая клавиша мыши
        /// </summary>
        private bool _isMouseDownLeft;
        /// <summary>
        /// Зажата ли праваля клавиша мыши
        /// </summary>
        private bool _isMouseDownRight;
        /// <summary>
        /// Флаг первого запуска упровление мышкой вида от первого лица
        /// </summary>
        private bool _flagFirstMouseFPV;

        public GameBase(WindowMain window) : base(window)
        {
            Ce.InitClient();
            Log = new Logger("Logs");
            Filer = new Profiler(Log, "[Client] ");
            _packets = new ProcessClientPackets(this);
            Player = new PlayerClientOwner(this);
            Key = new Keyboard(this);
            Key.InGameMenu += _Key_InGameMenu;
            Key.InChat += _Key_InChat;
            WorldRender = new WorldRenderer(this);
        }

        /// <summary>
        /// Дельта последнего кадра в mc
        /// </summary>
        public float DeltaTimeFrame => window.DeltaTimeFrame;
        /// <summary>
        /// Дельта последнего тика в mc
        /// </summary>
        public float DeltaTime => window.DeltaTime;

        private void _Key_InChat(object sender, EventArgs e)
        {
            window.LScreen.Chat();
        }

        /// <summary>
        /// Активация окна меню
        /// </summary>
        private void _Key_InGameMenu(object sender, EventArgs e)
        {
            SetGamePauseSingle(true);
            window.LScreen.InGameMenu();
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
        /// Запуск игры и миров, после всех сетевых проверок, только для сети
        /// </summary>
        public virtual void GameStartingNet() { }

        /// <summary>
        /// Запуск игры и миров, по сети миры создаём чуть позже, после получения от сервера инфу о блоках и сущностях
        /// </summary>
        public virtual void GameStarting()
        {
            World = new WorldClient(this);
            World.TagDebug += World_TagDebug;
            Player.TagDebug += World_TagDebug;
            Player.WorldStarting();
            _stopwatch.Start();
            // Рендер мира информируем, что готово всё
            WorldRender.GameStarting();
            //Ce.IsDebugDrawChunks = 
            Ce.IsDebugDraw = true;
            FlagGameStarted = true;
        }

        private void World_TagDebug(object sender, StringEventArgs e) => _OnTagDebug(e);

        /// <summary>
        /// Остановка игры
        /// </summary>
        public virtual void GameStoping(string notification, bool isWarning)
        {
            World.Stoping();
        }

        /// <summary>
        /// Задать паузу для одиночной игры
        /// </summary>
        public virtual void SetGamePauseSingle(bool value) { }

        /// <summary>
        /// Получить истину запущена ли сеть
        /// </summary>
        public virtual bool IsRunNet() => true;

        /// <summary>
        /// Игрок на сервере
        /// </summary>
        public void PlayerOnTheServer(int id, string uuid)
        {
            Player.PlayerOnTheServer(id, uuid);

            // Отправим обзор 
            Player.SetOverviewChunk(Options.OverviewChunk, false);
            // Запущена игра
            StartedGame();

            World.SpawnEntityInWorld(Player);
            _flagTick = true;
        }

        /// <summary>
        /// Получили пакет загрузки от сервера
        /// </summary>
        public virtual void PacketLoadingGame(PacketS02LoadingGame packet) { }

        /// <summary>
        /// Запущена игра
        /// </summary>
        protected void StartedGame()
        {
            // Закрываем скрин загрузки
            window.ScreenClose();
            Hud = new HubDebug(this);
        }

        #endregion

        #region OnMouse

        /// <summary>
        /// Включить или выключить вид от первого лица
        /// </summary>
        public void MouseFirstPersonView(bool on)
        {
            if (_isMouseFirstPersonView != on)
            {
                _isMouseFirstPersonView = on;
                window.CursorShow(!on);
                if (on)
                {
                    // Включить вид от первого лица
                    _flagFirstMouseFPV = true;
                }
                else
                {
                    // Выключить вид от первого лица
                    Player.ActionStop();

                    // При отключении режима от первого лица, надо убрать клики мыши
                    if (_isMouseDownLeft || _isMouseDownRight)
                    {
                        _isMouseDownLeft = false;
                        Player.UndoHandAction();
                        if (_isMouseDownRight)
                        {
                            _isMouseDownRight = false;
                            Player.StoppedUsingItem();
                        }
                    }
                }
            }
        }

        public override void OnMouseMove(int x, int y)
        {
            if (_isMouseFirstPersonView)
            {
                int centerX = Gi.Width / 2;
                int centerY = Gi.Height / 2;

                if (_flagFirstMouseFPV)
                {
                    _flagFirstMouseFPV = false;
                }
                else
                {
                    // Передаём откланение от центра
                    Player.MouseMove(x - centerX, y - centerY);
                }
                // Смещаем курсор в центр
                window.SetCursorPosition(centerX, centerY);
            }
        }

        public override void OnMouseDown(MouseButton button, int x, int y)
        {
            if (_isMouseFirstPersonView)
            {
                // Если режим от первого лица, начинаем кликать руками игрока
                if (button == MouseButton.Left)
                {
                    _isMouseDownLeft = true;
                    Player.HandAction();
                }
                else if (button == MouseButton.Right)
                {
                    _isMouseDownRight = true;
                    Player.ItemUse();
                }
            }

            // Включить вид от первого лица, если это необходимо
            MouseFirstPersonView(true);
        }

        public override void OnMouseUp(MouseButton button, int x, int y)
        {
            Player.UndoHandAction();
            if (button == MouseButton.Left)
            {
                _isMouseDownLeft = false;
            }
            else if (button == MouseButton.Right)
            {
                _isMouseDownRight = false;
                Player.StoppedUsingItem();
            }
        }

        #endregion

        #region OnKey

        /// <summary>
        /// Клавиша нажата
        /// </summary>
        public override void OnKeyDown(Keys keys) => Key.OnKeyDown(keys);

        /// <summary>
        /// Клавиша отпущена
        /// </summary>
        public override void OnKeyUp(Keys keys) => Key.OnKeyUp(keys);

        /// <summary>
        /// Нажата клавиша в char формате
        /// </summary>
        public override void OnKeyPress(char key) => Key.OnKeyPress(key);

        #endregion

        #region TickDraw

        /// <summary>
        /// Игровой такт
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void OnTick(float deltaTime)
        {
            // почерёдно получаем пакеты с сервера
            //Filer.StartSection("Packets.Update");
            _packets.Update();
            //Filer.EndSection(10);
            if (_flagTick && !IsGamePaused)
            {
                _tickCounterClient++;
                TickCounter++;

                //Filer.StartSection("Player.Update");
                // Обновить игрока
                //Player.Update();
               // Filer.EndStartSection("World.Update", 10);
                // Обновить мир
                World.UpdateClient();
                //Filer.EndStartSection("WorldRender.Update", 10);
               
                // Обновить рендоровский мир
                WorldRender.Update();
                
                // Filer.EndSection(5);

                // Тут индикация если имеется
                if (Hud != null)
                {
                    Hud.OnTick(deltaTime);
                }

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
            //   TrancivePacket(new PacketC04PlayerPosition(new Vector3(1, 2.5f, 3.5f), true, false, true));
        }

        /// <summary>
        /// Получить время в милисекундах с момента запуска проекта
        /// </summary>
        public long Time() => _stopwatch.ElapsedMilliseconds;

        /// <summary>
        /// Получить время в тактах объекта Stopwatch с момента запуска проекта
        /// </summary>
        public long ElapsedTicks() => _stopwatch.ElapsedTicks;

        /// <summary>
        /// Задать время с сервера
        /// </summary>
        public void SetTickCounter(uint time) => TickCounter = time;

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        public void OnResized(int width, int height)
        {
            // Тут индикация если имеется
            if (Hud != null)
            {
                Hud.OnResized(width, height);
            }
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            // Тут прорисовка мира
            gl.ClearColor(.4f, .4f, .7f, 1f);
            // Мир
            if (FlagGameStarted)
            {
                // Мир появляется, как загрузятся 
                WorldRender.Draw(timeIndex);
            }

            // Тут индикация если имеется
            if (Hud != null)
            {
                Hud.Draw(timeIndex);
            }

            if (Ce.IsDebugDrawChunks)
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

        /// <summary>
        /// Сетевой игрок зашёл или вышел из игры сервера, для чата
        /// </summary>
        public void PlayerEntryRemove(PacketS06PlayerEntryRemove packet)
        {
            if (packet.Login == "")
            {
                // Remove
                Players.Remove(packet.Index);
            }
            else
            {
                // Entry
                if (Players.ContainsKey(packet.Index))
                {
                    Players[packet.Index] = packet.Login;
                }
                else
                {
                    Players.Add(packet.Index, packet.Login);
                }
            }
        }

        #endregion

        public override void Dispose()
        {
            base.Dispose();
            // Надо очистить сетки
            if (Hud != null)
            {
                Hud.Dispose();
            }
            WorldRender.Dispose();
        }

        public override string ToString()
        {
            return string.Format("{0} ping: {1} ms Traffic: {3}{2} Tick {4}\r\n{5}\r\n{6} {7}",
                IsLoacl ? "Local" : "Net", // 0
                Player.Ping, IsGamePaused ? " Pause" : "", // 1-2
                _ToTraffic(),TickCounter, // 3-4
                Player, // 5
                WorldRender.ToString(), // 6
                World != null ? World.ToString() : "" // 7
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
                Ce.IsDebugDrawChunks = Ce.IsDebugDraw = false;
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
        {
            WorldRender.Stoping();
            Error?.Invoke(this, new ThreadExceptionEventArgs(ex));
        }

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
