using System;
using WinGL;
using WinGL.Util;
using WinGL.OpenGL;
using System.Reflection;
using System.Threading;
using WinGL.Actions;
using Vge.Renderer.Font;

namespace Vge
{
    public class WindowMain : Window
    {
        /// <summary>
        /// Шейдоры для 2д
        /// </summary>
        public static Shader2d shader2D;
        /// <summary>
        /// Шейдоры для текста
        /// </summary>
        public static ShaderText shaderText;

        
        /// <summary>
        /// Координата мыши Х
        /// </summary>
        protected int mouseX;
        /// <summary>
        /// Координата мыши Y
        /// </summary>
        protected int mouseY;
        /// <summary>
        /// Счётчик фпс за кадр
        /// </summary>
        protected int fps = 0;
        protected int tps = 0;
        /// <summary>
        /// Версия OpenGL
        /// </summary>
        protected string versionOpenGL = "";
        /// <summary>
        /// Версия программы
        /// </summary>
        protected string version = "";
        /// <summary>
        /// Объект текстур
        /// </summary>
        protected TextureMap textureMap;
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
            version = "Vge " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
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
            tps = counterTps.CountTick;
        }

        protected virtual void CounterFps_Tick(object sender, EventArgs e)
        {
           // if (InvokeRequired) Invoke(new EventHandler(Ticker_Frame), sender, e);
            SetTitle(versionOpenGL + "FPS " + counterFps.CountTick);
            fps = counterFps.CountTick;
        }

        protected override void OnMouseMove(int x, int y)
        {
            mouseX = x;
            mouseY = y;
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
            FontRenderer.Init(gl);

            shader2D = new Shader2d(gl);
            shaderText = new ShaderText(gl);

            if (openGLVersion == OpenGLVersion.OpenGL2_1)
            {
                versionOpenGL = "OpenGL 2.1 ";
            }
            else
            {
                var majVers = new int[1];
                var minVers = new int[1];
                gl.GetInteger(GL.GL_MAJOR_VERSION, majVers);
                gl.GetInteger(GL.GL_MINOR_VERSION, minVers);
                versionOpenGL = "OpenGL " + majVers[0] + "." + minVers[0] + " ";
            }
        }

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
