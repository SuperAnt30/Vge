using System;
using WinGL;
using WinGL.OpenGL;
using System.Reflection;
using System.Threading;
using WinGL.Actions;
using Vge.Renderer;
using Vge.Util;
using Vge.Games;
using Vge.Audio;
using Vge.Event;
using Vge.Gui.Screens;
using Vge.World;
using Vge.World.Block;

namespace Vge
{
    public class WindowMain : Window
    {
        #region Properties

        /// <summary>
        /// Версия OpenGL
        /// </summary>
        public string VersionOpenGL { get; protected set; } = "";
        /// <summary>
        /// Версия программы
        /// </summary>
        public string Version { get; protected set; } = "";
        /// <summary>
        /// Координата мыши Х
        /// </summary>
        public int MouseX { get; protected set; }
        /// <summary>
        /// Координата мыши Y
        /// </summary>
        public int MouseY { get; protected set; }
        /// <summary>
        /// Отладочный класс
        /// </summary>
        public readonly Debug debug = new Debug();

        /// <summary>
        /// Объект отвечающий за прорисовку
        /// </summary>
        public RenderMain Render { get; protected set; }
        /// <summary>
        /// Объект запуска экрана
        /// </summary>
        public LaunchScreen LScreen { get; protected set; }
        /// <summary>
        /// Игровой объект
        /// </summary>
        public GameBase Game { get; protected set; }
        /// <summary>
        /// Дельта последнего тика в mc
        /// </summary>
        public float DeltaTime { get; private set; }
       
        /// <summary>
        /// Готово ли окно для графики и прочего, меняется статус после Splash
        /// </summary>
        public bool Ready { get; private set; } = false;
        /// <summary>
        /// Объект экрана
        /// </summary>
        public ScreenBase Screen { get; private set; }

        #endregion

        #region Variables

        /// <summary>
        /// Объект создающий последовательные кадры и тики
        /// </summary>
        protected Ticker ticker;
        /// <summary>
        /// Объект звуков
        /// </summary>
        protected AudioBase audio;

        /// <summary>
        /// Флаг на удаление, ждём когда закроется сервер
        /// </summary>
        private bool flagClose = false;
        /// <summary>
        /// Фиксация времени начала тика
        /// </summary>
        private long timeBegin;
        /// <summary>
        /// Фиксация конечное время тика
        /// </summary>
        private long endTime;
        /// <summary>
        /// Фиксация текущее время тика
        /// </summary>
        private long currentTime;

        #endregion

        #region Initialized

        public WindowMain() : base()
        {
            LScreen = new LaunchScreen(this);
            Initialized();
            vSync = Options.VSync;
            FullScreen = Options.FullScreen;

            openGLVersion = OpenGLVersion.OpenGL3_3;

            ticker = new Ticker();
            ticker.Tick += Ticker_Tick;
            ticker.Frame += Ticker_Frame;
            ticker.SetWishFrame(Ce.FpsOffside);
        }

        protected virtual void Initialized()
        {
            Version = "Vge " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// Готово окно для графики и прочего, меняем статус после Splash
        /// </summary>
        public void Readed() => Ready = true;

        /// <summary>
        /// Вызывается перед очисткой окна
        /// </summary>
        protected override void CleanWindow()
        {
            Ready = false;
            if (Screen != null) Screen.Dispose();
            if (Game != null) Game.Dispose();
            if (Render != null) Render.Dispose();
            if (audio != null) audio.Clear();
        }

        /// <summary>
        /// Получить объект OpenGL
        /// </summary>
        public GL GetOpenGL() => gl;

        /// <summary>
        /// Прочесть настройки
        /// </summary>
        protected virtual void OptionsLoad() => new OptionsFile().Load();
        /// <summary>
        /// Записать настройки
        /// </summary>
        public virtual void OptionsSave() => new OptionsFile().Save();

        #endregion

        #region OnMouse

        /// <summary>
        /// Перемещение мыши
        /// </summary>
        protected override void OnMouseMove(int x, int y)
        {
            MouseX = x;
            MouseY = y;
            if (Screen != null) Screen.OnMouseMove(x, y);
            else if (Game != null) Game.OnMouseMove(x, y);
        }

        /// <summary>
        /// Нажатие курсора мыши
        /// </summary>
        protected override void OnMouseDown(MouseButton button, int x, int y)
        {
            MouseX = x;
            MouseY = y;
            if (Screen != null) Screen.OnMouseDown(button, x, y);
            else if (Game != null) Game.OnMouseDown(button, x, y);
        }

        /// <summary>
        /// Отпустил курсор мыши
        /// </summary>
        protected override void OnMouseUp(MouseButton button, int x, int y)
        {
            MouseX = x;
            MouseY = y;
            if (Screen != null) Screen.OnMouseUp(button, x, y);
            else if (Game != null) Game.OnMouseUp(button, x, y);
        }

        /// <summary>
        /// Вращение колёсика мыши
        /// </summary>
        protected override void OnMouseWheel(int delta, int x, int y)
        {
            MouseX = x;
            MouseY = y;
            if (Screen != null) Screen.OnMouseWheel(delta, x, y);
            else if (Game != null) Game.OnMouseWheel(delta, x, y);
        }

        #endregion

        #region OnKey

        /// <summary>
        /// Клавиша нажата
        /// </summary>
        protected override void OnKeyDown(Keys keys)
        {
            if (Screen != null) Screen.OnKeyDown(keys);
            else if (Game != null) Game.OnKeyDown(keys);
        }

        /// <summary>
        /// Клавиша отпущена
        /// </summary>
        protected override void OnKeyUp(Keys keys)
        {
            if (Screen != null) Screen.OnKeyUp(keys);
            else if (Game != null) Game.OnKeyUp(keys);
        }

        /// <summary>
        /// Нажата клавиша в char формате
        /// </summary>
        protected override void OnKeyPress(char key)
        {
            if (Screen != null) Screen.OnKeyPress(key);
        }

        #endregion

        #region On...

        /// <summary>
        /// Активация или деакциваия окна
        /// </summary>
        protected override void OnActivate(bool active)
        {
            if (!active && Game != null)
            {
                // Если происходит деактивация окна, а мы находились в игре,
                // Выключаем вид от первого лица
                Game.MouseFirstPersonView(false);
            }
        }

        protected override void OnOpenGLInitialized()
        {
            if (openGLVersion == OpenGLVersion.OpenGL2_1)
            {
                VersionOpenGL = "OpenGL 2.1 ";
            }
            else
            {
                var majVers = new int[1];
                var minVers = new int[1];
                gl.GetInteger(GL.GL_MAJOR_VERSION, majVers);
                gl.GetInteger(GL.GL_MINOR_VERSION, minVers);
                VersionOpenGL = "OpenGL " + majVers[0] + "." + minVers[0] + " ";
            }

            RenderInitialized();
        }

        /// <summary>
        /// Прорисовка кадра
        /// </summary>
        protected override void OnOpenGlDraw()
        {
            base.OnOpenGlDraw();

            gl.Clear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);
           // gl.ClearColor(.7f, .4f, .4f, 1f);
            gl.Enable(GL.GL_DEPTH_TEST);
            // группа для сглаживания, но может жутко тормазить
            gl.BlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);
            gl.Enable(GL.GL_BLEND);

            Render.DrawBegin();
            if (Game == null)
            {
                // Нет игры
                if (Screen == null)
                {
                    // Отсутствует прорисовка
                    if (IsRunning())
                    {
                        throw new Exception(Sr.ThereIsNoDrawing);
                    }
                }
                else
                {
                    Screen.Draw(ticker.Interpolation);
                }
            }
            else
            {
                // Есть игра
                Game.Draw(ticker.Interpolation);
                if (Screen != null)
                {
                    Screen.Draw(ticker.Interpolation);
                }
            }
            if (Ce.IsDebugDraw)
            {
                DrawDebug();
            }
            Render.DrawEnd();
        }

        /// <summary>
        /// Отладка на экране
        /// </summary>
        protected virtual void DrawDebug() { }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized(int width, int height)
        {
            base.OnResized(width, height);
            Gi.Width = Width;
            Gi.Height = Height;
            UpdateSizeInterface();
            if (Screen != null) 
            {
                Screen.Resized();
            }
        }

        #endregion

        /// <summary>
        /// Изменить размер интерфейса
        /// </summary>
        public virtual void UpdateSizeInterface()
        {
            Gi.UpdateSizeInterface();
            if (Render != null)
            {
                Render.FontMain.UpdateSizeInterface();
            }
        }

        #region WindowOverride

        /// <summary>
        /// Включить или выключить вертикальную сенхронизацию
        /// </summary>
        public override void SetVSync(bool on)
        {
            base.SetVSync(on);
            ticker.ResetTimeFrame();
        }

        /// <summary>
        /// Запущено окно
        /// </summary>
        public override void Begined() => LScreen.Splash();

        /// <summary>
        /// Инициализаця объекта рендера
        /// </summary>
        protected virtual void RenderInitialized() => Render = new RenderMain(this);

        /// <summary>
        /// Закрыть приложение
        /// </summary>
        public void Exit() => Close();

        /// <summary>
        /// Закрыть приложение
        /// </summary>
        protected override void Close()
        {
            if (Game != null)
            {
                flagClose = true;
                Game.GameStoping(Srl.ClosingTheApplication, false);
            }
            else
            {
                base.Close();
            }
        }

        protected override void Begin()
        {
            try
            {
                base.Begin();
            }
            catch (Exception e)
            {
                Logger.Crash(e, "Begin");
                throw e;
            }
        }

        /// <summary>
        /// Тик в лупе
        /// </summary>
        protected override void LoopTick() => ticker.DoTick();

        #endregion

        #region Screen

        /// <summary>
        /// Создать скрин по объекту, который есть в ядре
        /// </summary>
        public void ScreenCreate(ScreenBase screen, bool dispose = true)
        {
            if (dispose && Screen != null) Screen.Dispose();
            Screen = screen;
            Screen.Initialize();
            // Если находимся в игре, то выключаем управление вида от первого лица
            if (Game != null)
            {
                Game.MouseFirstPersonView(false);
            }
        }
        /// <summary>
        /// Запуск от родителя с параметром
        /// </summary>
        public void ScreenLaunchFromParent(ScreenBase screen, EnumScreenParent enumParent = EnumScreenParent.None)
        {
            if (Screen != null) Screen.Dispose();
            Screen = screen;
            Screen.LaunchFromParent(enumParent);
            Screen.Resized();
        }
        /// <summary>
        /// Закрыть скрин
        /// </summary>
        public void ScreenClose()
        {
            if (Screen != null) Screen.Dispose();
            Screen = null;
            // Если находимся в игре, то включаем управление вида от первого лица
            if (Game != null)
            {
                Game.MouseFirstPersonView(true);
            }
        }

        #endregion

        #region Game

        /// <summary>
        /// Создание миров
        /// </summary>
        protected virtual AllWorlds CreateAllWorlds() => new AllWorlds();

        /// <summary>
        /// Инициализация блоков
        /// </summary>
        protected virtual void _InitializationBlocks() => BlocksReg.Initialization();

        /// <summary>
        /// Запустить игру по сети
        /// </summary>
        public void GameNetRun(string ipAddress, int port)
        {
            if (Game == null)
            {
                LScreen.Process(L.T("Connection") + Ce.Ellipsis);
                _InitializationBlocks();
                Game = new GameNet(this, ipAddress, port);
                GameRun();
            }
        }

        /// <summary>
        /// Запустить локальную игру
        /// </summary>
        public void GameLocalRun(GameSettings gameSettings)
        {
            if (Game == null)
            {
                LScreen.Working();
                _InitializationBlocks();
                Game = new GameLocal(this, gameSettings, CreateAllWorlds());
                GameRun();
            }
        }

        private void GameRun()
        {
            Game.Stoped += Game_Stoped;
            Game.Error += Game_Error;
            Game.ServerTextDebug += Game_ServerTextDebug;
            Game.TagDebug += Game_TagDebug;
            SetWishFrame(Options.Fps);
            Game.GameStarting();
        }

        /// <summary>
        /// Остановить игру
        /// </summary>
        public void GameStoping()
        {
            if (Game != null)
            {
                Game.GameStoping(Srl.TheUserStoppedTheGame, false);
            }
        }

        private void Game_ServerTextDebug(object sender, StringEventArgs e)
            => debug.Server = e.Text;

        private void Game_TagDebug(object sender, StringEventArgs e)
            => debug.SetTag(e);

        /// <summary>
        /// Игра остановлена
        /// </summary>
        protected virtual void Game_Stoped(object sender, GameStopEventArgs e)
        {
            Game.Dispose();
            Game = null;
            SetWishFrame(Ce.FpsOffside);
            debug.Server = e.Notification;
            if (flagClose)
            {
                // Если закрытие игры из-за закритии приложения, 
                // повторяем закрытие приложения
                Close();
            }
            else
            {
                // Чистка мусора
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Тут надо вызвать запуск окна с ошибкой
                if (e.IsWarning)
                {
                    // Окно оповещения
                    LScreen.Notification(e.Notification);
                }
                else
                {
                    // Меню
                    LScreen.MainMenu();
                }
                return;
            }
        }

        private void Game_Error(object sender, ThreadExceptionEventArgs e)
        {
            Logger.Crash(e.Exception, "WindowMain");
            MessageBoxCrash(e.Exception);
        }

        #endregion

        #region Ticker

        /// <summary>
        /// Задать желаемый фпс
        /// </summary>
        public void SetWishFrame(int frame) => ticker.SetWishFrame(frame);

        protected virtual void Ticker_Frame(object sender, EventArgs e) => DrawFrame();

        private void Ticker_Tick(object sender, EventArgs e)
        {
            timeBegin = TimeTicks();
            OnTick();
            // фиксируем текущее время такта
            currentTime = TimeTicks();
            // Находим дельту времени между тактами
            DeltaTime = (currentTime - endTime) / (float)Ticker.TimerFrequency;
            // фиксируем конечное время
            endTime = currentTime;
            // Считаем время выполнение такта и тикаем рендер
            Render.SetExecutionTime((currentTime - timeBegin) / (float)Ticker.TimerFrequency);
        }

        /// <summary>
        /// Стабильный игровой такт
        /// </summary>
        protected virtual void OnTick()
        {
            audio.Tick();
            debug.Audio = audio.StrDebug;
            
            if (Screen != null)
            {
                Screen.OnTick(DeltaTime);
            }

            if (Game == null)
            {
                debug.Client = "null";
            }
            else
            {
                debug.Client = Game.ToString();
                Game.OnTick(DeltaTime);
            }
        }

        /// <summary>
        /// Получить время в милисекундах с момента запуска проекта
        /// </summary>
        public long Time() => ticker.Time();
        /// <summary>
        /// Получить время в тактах с момента запуска проекта
        /// </summary>
        public long TimeTicks() => ticker.TimeTicks();

        #endregion

        #region Sound

        /// <summary>
        /// Звук клика
        /// </summary>
        public virtual void SoundClick(float volume) { }

        #endregion
    }
}
