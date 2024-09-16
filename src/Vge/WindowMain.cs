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
        /// Список одиночных игр
        /// </summary>
        public ListSingleGame ListSingle { get; private set; }

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
        /// Объект экрана
        /// </summary>
        private ScreenBase screen;

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
            ListSingle = new ListSingleGame(this);
            Initialized();

            openGLVersion = OpenGLVersion.OpenGL3_3;

            ticker = new Ticker();
            ticker.Tick += Ticker_Tick;
            ticker.Frame += Ticker_Frame;
            ticker.SetWishFrame(60);
        }

        protected virtual void Initialized()
        {
            Version = "Vge " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// Вызывается перед очисткой окна
        /// </summary>
        protected override void CleanWindow() => Render.Dispose();

        /// <summary>
        /// Получить объект OpenGL
        /// </summary>
        public GL GetOpenGL() => gl;

        #endregion

        #region OnMouse

        /// <summary>
        /// Перемещение мыши
        /// </summary>
        protected override void OnMouseMove(int x, int y)
        {
            MouseX = x;
            MouseY = y;
            if (screen != null) screen.OnMouseMove(x, y);
            if (Game != null) Game.OnMouseMove(x, y);
        }

        /// <summary>
        /// Нажатие курсора мыши
        /// </summary>
        protected override void OnMouseDown(MouseButton button, int x, int y)
        {
            MouseX = x;
            MouseY = y;
            if (screen != null) screen.OnMouseDown(button, x, y);
            if (Game != null) Game.OnMouseDown(button, x, y);
        }

        /// <summary>
        /// Отпустил курсор мыши
        /// </summary>
        protected override void OnMouseUp(MouseButton button, int x, int y)
        {
            MouseX = x;
            MouseY = y;
            if (screen != null) screen.OnMouseUp(button, x, y);
            if (Game != null) Game.OnMouseUp(button, x, y);
        }

        /// <summary>
        /// Вращение колёсика мыши
        /// </summary>
        protected override void OnMouseWheel(int delta, int x, int y)
        {
            MouseX = x;
            MouseY = y;
            if (screen != null) screen.OnMouseWheel(delta, x, y);
            if (Game != null) Game.OnMouseWheel(delta, x, y);
        }

        #endregion

        #region OnKey

        /// <summary>
        /// Клавиша нажата
        /// </summary>
        protected override void OnKeyDown(Keys keys)
        {
            if (screen != null) screen.OnKeyDown(keys);
            if (Game != null) Game.OnKeyDown(keys);

            if (keys == Keys.F11)
            {
                FullScreen = !FullScreen;
                ReloadGLWindow();
            }
            else if (keys == Keys.F12)
            {
                SetVSync(!VSync);
            }
            else if (keys == Keys.F8)
            {
                ticker.SetWishFrame(120);
            }
            else if (keys == Keys.F7)
            {
                ticker.SetWishFrame(20);
            }
            else if (keys == Keys.F6)
            {
                ticker.SetWishFrame(280);
            }
        }

        /// <summary>
        /// Клавиша отпущена
        /// </summary>
        protected override void OnKeyUp(Keys keys)
        {
            if (screen != null) screen.OnKeyUp(keys);
            if (Game != null) Game.OnKeyUp(keys);
        }

        /// <summary>
        /// Нажата клавиша в char формате
        /// </summary>
        protected override void OnKeyPress(char key)
        {
            if (screen != null) screen.OnKeyPress(key);
        }

        #endregion

        #region On...

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
        /// После перезапуска FullScreen происходит
        /// </summary>
        protected override void OnReloadGLWindow()
        {
            if (screen != null)
            {
                screen.Initialize();
            }
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
                if (screen == null)
                {
                    // Отсутствует прорисовка
                    throw new Exception(Sr.ThereIsNoDrawing);
                }
                else
                {
                    screen.Draw(ticker.Interpolation);
                }
            }
            else
            {
                // Есть игра
                Game.Draw(ticker.Interpolation);
                if (screen != null)
                {
                    screen.Draw(ticker.Interpolation);
                }
            }
            Render.DrawEnd();
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized(int width, int height)
        {
            base.OnResized(width, height);
            Gi.Width = Width;
            Gi.Height = Height;
            Gi.UpdateSizeInterface();
            if (Render != null)
            {
                Render.FontMain.UpdateSizeInterface();
            }
            if (screen != null) 
            {
                screen.Resized();
            }
        }

        #endregion

        #region WindowOverride

        /// <summary>
        /// Включить или выключить вертикальную сенхронизацию
        /// </summary>
        protected override void SetVSync(bool on)
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
        protected virtual void RenderInitialized()
        {
            Render = new RenderMain(this);
        }

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
            if (dispose && this.screen != null) this.screen.Dispose();
            this.screen = screen;
            this.screen.Initialize();
        }
        /// <summary>
        /// Запуск от родителя с параметром
        /// </summary>
        public void ScreenLaunchFromParent(ScreenBase screen, EnumScreenParent enumParent = EnumScreenParent.None)
        {
            this.screen = screen;
            this.screen.LaunchFromParent(enumParent);
            this.screen.Resized();
        }
        /// <summary>
        /// Закрыть скрин
        /// </summary>
        public void ScreenClose()
        {
            if (screen != null) screen.Dispose();
            screen = null;
        }

        #endregion

        #region Game

        /// <summary>
        /// Запустить игру по сети
        /// </summary>
        public void GameNetRun(string ipAddress, int port)
        {
            LScreen.Close();
            if (Game == null)
            {
                Game = new GameNet(this, ipAddress, port);
                GameRun();
            }
        }

        /// <summary>
        /// Запустить локальную игру
        /// </summary>
        public void GameLocalRun()
        {
            LScreen.Close();
            if (Game == null)
            {
                Game = new GameLocal(this);
                GameRun();
            }
        }

        private void GameRun()
        {
            Game.Stoped += Game_Stoped;
            Game.Error += Game_Error;
            Game.ServerTextDebug += Game_ServerTextDebug;
            Game.Tick += Game_Tick;
            Game.GameStarting();
        }

        protected virtual void Game_Tick(object sender, EventArgs e) { }

        private void Game_ServerTextDebug(object sender, StringEventArgs e)
            => debug.server = e.Text;

        /// <summary>
        /// Игра остановлена
        /// </summary>
        protected virtual void Game_Stoped(object sender, GameStopEventArgs e)
        {
            Game.Dispose();
            Game = null;
            debug.server = e.Notification;
            if (flagClose)
            {
                // Если закрытие игры из-за закритии приложения, 
                // повторяем закрытие приложения
                Close();
            }
            else
            {
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

        /// <summary>
        /// Остановить игру
        /// </summary>
        protected void GameStoping()
        {
            if (Game != null)
            {
                Game.GameStoping(Srl.TheUserStoppedTheGame, false);
            }
        }

        #endregion

        #region Ticker

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
            debug.audio = audio.StrDebug;
            
            if (screen != null)
            {
                screen.OnTick(DeltaTime);
            }

            if (Game == null)
            {
                debug.client = "null";
            }
            else
            {
                debug.client = Game.ToString();
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
