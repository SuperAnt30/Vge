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
        protected RenderBase render;
        /// <summary>
        /// Объект создающий последовательные кадры и тики
        /// </summary>
        protected Ticker ticker;
        /// <summary>
        /// Игровой объект
        /// </summary>
        protected GameBase game;
        /// <summary>
        /// Объект звуков
        /// </summary>
        protected AudioBase audio;

        /// <summary>
        /// Флаг на удаление, ждём когда закроется сервер
        /// </summary>
        private bool flagClose = false;

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
        public override void Begined()
        {
            // TODO::2024-08-22 тут загрузчик включит, текстур и прочего
        }

        /// <summary>
        /// Инициализаця объекта рендера
        /// </summary>
        protected virtual void RenderInitialized() => render = new RenderBase(this, gl);

        /// <summary>
        /// Закрыть приложение
        /// </summary>
        protected override void Close()
        {
            if (game != null)
            {
                flagClose = true;
                game.GameStoping();
            }
            else
            {
                base.Close();
            }
        }

        /// <summary>
        /// Тик в лупе
        /// </summary>
        protected override void LoopTick() => ticker.DoTick();

        #endregion

        #region Game

        /// <summary>
        /// Запустить игру по сети
        /// </summary>
        protected void GameNetRun()
        {
            if (game == null)
            {
                game = new GameNet();
                GameRun();
            }
        }

        /// <summary>
        /// Запустить локальную игру
        /// </summary>
        protected void GameLocalRun()
        {
            if (game == null)
            {
                game = new GameLocal();
                GameRun();
            }
        }

        private void GameRun()
        {
            game.Stoped += Game_Stoped;
            game.Error += Game_Error;
            game.ServerTextDebug += Game_ServerTextDebug;
            game.GameStarting();
        }

        private void Game_ServerTextDebug(object sender, StringEventArgs e)
            => debug.server = e.Text;

        /// <summary>
        /// Игра остановлена
        /// </summary>
        protected virtual void Game_Stoped(object sender, EventArgs e)
        {
            game = null;
            debug.server = "";
            if (flagClose)
            {
                // Если закрытие игры из-за закритии приложения, 
                // повторяем закрытие приложения
                Close();
            }
        }

        private void Game_Error(object sender, ThreadExceptionEventArgs e)
        {
            MessageBoxCrach(e.Exception);
        }

        /// <summary>
        /// Остановить игру
        /// </summary>
        protected void GameStoping()
        {
            if (game != null)
            {
                game.GameStoping();
            }
        }

        #endregion

        #region Ticker

        protected virtual void Ticker_Frame(object sender, EventArgs e)
        {
            DrawFrame();
        }

        private void Ticker_Tick(object sender, EventArgs e)
        {
            timeBegin = TimeTicks();
            OnTick();
            render.UpdateTick((float)(TimeTicks() - timeBegin) / (float)Ticker.TimerFrequency);
        }

        private long timeBegin;

        protected virtual void OnTick()
        {
            audio.Tick();
            debug.audio = audio.StrDebug;
            debug.client = game != null ? game.ToString() : "null";

            // Отладка на экране
            render.SetTextDebug(debug.ToText());
        }

        #endregion

    }
}
