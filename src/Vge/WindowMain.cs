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
        /// Объект экрана
        /// </summary>
        public ScreenBase Screen { get; protected set; }
        /// <summary>
        /// Игровой объект
        /// </summary>
        public GameBase Game { get; protected set; }
        /// <summary>
        /// Дельта последнего тика в mc
        /// </summary>
        public float DeltaTime { get; private set; }

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

        public WindowMain() : base()
        {
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
        /// Получить объект OpenGL
        /// </summary>
        public GL GetOpenGL() => gl;

        /// <summary>
        /// Получить время в милисекундах с момента запуска проекта
        /// </summary>
        public long Time() => ticker.Time();
        /// <summary>
        /// Получить время в тактах с момента запуска проекта
        /// </summary>
        public long TimeTicks() => ticker.TimeTicks();

        #region On...

        protected override void OnMouseMove(int x, int y)
        {
            MouseX = x;
            MouseY = y;
        }

        protected override void OnKeyDown(Keys keys)
        {
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
            //gl.ClearColor(.7f, .4f, .4f, 1f);
            ///gl.Enable(GL.GL_DEPTH_TEST);
            // группа для сглаживания, но может жутко тормазить
            gl.BlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);
            gl.Enable(GL.GL_BLEND);

            Render.Draw(ticker.Interpolation);
            //renderMvk.Draw();
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
        public override void Begined() => Screen = new ScreenSplash(this);

        /// <summary>
        /// Инициализаця объекта рендера
        /// </summary>
        protected virtual void RenderInitialized() => Render = new RenderMain(this);

        /// <summary>
        /// Закрыть приложение
        /// </summary>
        protected override void Close()
        {
            if (Game != null)
            {
                flagClose = true;
                Game.GameStoping("Закрытие приложения");
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
        /// Создать скрин заставки
        /// </summary>
        public virtual void ScreenSplash() => Screen = new ScreenSplash(this);
        /// <summary>
        /// Создать скрин по индексу
        /// </summary>
        public virtual void ScreenMainMenu() => Screen = new ScreenMainMenu(this);

        /// <summary>
        /// Создать скрин по объекту, который есть в ядре
        /// </summary>
        public void ScreenCreate(ScreenBase screen) => Screen = screen;
        /// <summary>
        /// Закрыть скрин
        /// </summary>
        public void ScreenClose() => Screen = null;

        #endregion

        #region Game

        /// <summary>
        /// Запустить игру по сети
        /// </summary>
        protected void GameNetRun(string ipAddress, int port)
        {
            if (Game == null)
            {
                Game = new GameNet(this, ipAddress, port);
                GameRun();
            }
        }

        /// <summary>
        /// Запустить локальную игру
        /// </summary>
        protected void GameLocalRun()
        {
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
        protected virtual void Game_Stoped(object sender, StringEventArgs e)
        {
            Game = null;
            debug.server = e.Text;// "";
            if (flagClose)
            {
                // Если закрытие игры из-за закритии приложения, 
                // повторяем закрытие приложения
                Close();
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
                Game.GameStoping("Пользователь остановил игру");
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
            
            if (Screen != null)
            {
                Screen.OnTick(DeltaTime);
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

            // Отладка на экране
            Render.SetTextDebug(debug.ToText());
        }

        #endregion

    }
}
