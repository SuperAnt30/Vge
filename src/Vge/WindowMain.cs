using System;
using WinGL;
using WinGL.Util;
using WinGL.OpenGL;
using System.Reflection;
using System.Threading;
using WinGL.Actions;
using Vge.Renderer;

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
        /// Счётчик кадров в секунду
        /// </summary>
        public int Fps { get; protected set; } = 0;
        /// <summary>
        /// Счётчик игровых тиков в секунду
        /// </summary>
        public int Tps { get; protected set; } = 0;
        
        /// <summary>
        /// Объект отвечающий за прорисовку
        /// </summary>
        protected RenderBase render;
        /// <summary>
        /// Объект сервера
        /// </summary>
        //protected Server server;
        /// <summary>
        /// Объект клиента плюс локальный луп сервера
        /// </summary>
        protected Client client;

        public CounterTick counterFps = new CounterTick();
        public CounterTick counterTps = new CounterTick();


        public WindowMain() : base()
        {
            Version = "Vge " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            openGLVersion = OpenGLVersion.OpenGL3_3;
            counterFps.Tick += CounterFps_Tick;
            counterTps.Tick += CounterTps_Tick;
        }

        /// <summary>
        /// Включить или выключить вертикальную сенхронизацию
        /// </summary>
        protected override void SetVSync(bool on)
        {
            base.SetVSync(on);
            client.ResetTimeFrame();
        }

        /// <summary>
        /// Запущено окно
        /// </summary>
        public override void Begined()
        {
            //StartServer();
            StartClient();
        }

        private void CounterTps_Tick(object sender, EventArgs e)
        {
            Tps = counterTps.CountTick;
        }

        protected virtual void CounterFps_Tick(object sender, EventArgs e)
        {
           // if (InvokeRequired) Invoke(new EventHandler(Ticker_Frame), sender, e);
            SetTitle(VersionOpenGL + "FPS " + counterFps.CountTick);
            Fps = counterFps.CountTick;
        }

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
                client.SetWishFrame(120);
            }
            else if (keys == Keys.F7)
            {
                client.SetWishFrame(20);
            }
            else if (keys == Keys.F6)
            {
                client.SetWishFrame(280);
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
        /// Инициализаця объекта рендера
        /// </summary>
        protected virtual void RenderInitialized() => render = new RenderBase(this, gl);

        protected override void OnOpenGlDraw()
        {
            counterFps.CalculateFrameRate();
        }

        /// <summary>
        /// Закрыть приложение
        /// </summary>
        protected override void Close()
        {
            base.Close();
            //client.Stoping();
            //server.Stop();
        }

        /// <summary>
        /// Тик в лупе
        /// </summary>
        protected override void LoopTick()
        {
            // DrawFrame();
            //Thread.Sleep(16);
            //Thread.Sleep(3);
            client.DoTick();
        }

        #region Client

        /// <summary>
        /// Запустить сервер в отдельном потоке
        /// </summary>
        private void StartClient()
        {
            client = new Client();
            client.Error += Client_Error;
            client.Tick += Client_Tick;
            client.Frame += Client_Frame;
            client.SetWishFrame(60);
        }

        protected virtual void Client_Error(object sender, ThreadExceptionEventArgs e)
        {
            MessageBoxCrach(e.Exception);
        }

        protected virtual void Client_Frame(object sender, EventArgs e)
        {
            DrawFrame();
        }

        protected virtual void Client_Tick(object sender, EventArgs e)
        {
            counterTps.CalculateFrameRate();
        }

        #endregion

        #region Server

        /// <summary>
        /// Запустить сервер в отдельном потоке
        /// </summary>
        //private void StartServer()
        //{
        //    server = new Server();
        //    server.Closeded += Server_Closeded;
        //    server.Error += Server_Error;
        //    server.Tick += Server_Tick;
        //    server.StartServer();
        //}

        //protected virtual void Server_Error(object sender, ThreadExceptionEventArgs e)
        //{
        //    MessageBoxCrach(e.Exception);
        //}

        //protected virtual void Server_Tick(object sender, EventArgs e) { }

        //protected virtual void Server_Closeded(object sender, EventArgs e)
        //{
        //    Close();
        //}

        #endregion


        
    }
}
