using System;
using WinGL.OpenGL;
using WinGL.Actions;
using System.Reflection;
using Vge;
using Mvk2.Util;
using Mvk2.Audio;
using Mvk2.Renderer;
using Vge.Network.Packets.Client;
using Mvk2.Gui.Screens;

namespace Mvk2
{
    public class WindowMvk : WindowMain
    {
        /// <summary>
        /// Виден ли курсор
        /// </summary>
        public bool cursorShow = false;

        /// <summary>
        /// Объект отвечающий за прорисовку Малювек
        /// </summary>
        private RenderMvk renderMvk;

        #region Initialized

        public WindowMvk() : base() { }

        protected override void Initialized()
        {
            Version = "Test Малювек2 by SuperAnt ver. " + Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // Загружаем опции
            new OptionsFileMvk().Load();

            audio = new AudioMvk();
            // Инициализация звука и загрузка семплов
            audio.Initialize(2);
            audio.InitializeSample();
        }

        #endregion

        #region OnMouse

        protected override void OnMouseDown(MouseButton button, int x, int y)
        {
            base.OnMouseDown(button, x, y);
            if (button == MouseButton.Left) cursorShow = true;
        }

        protected override void OnMouseUp(MouseButton button, int x, int y)
        {
            base.OnMouseUp(button, x, y);
            if (button == MouseButton.Left) cursorShow = false;
        }

        protected override void OnMouseEnter()
        {
            base.OnMouseEnter();
            //cursorShow = true;
            //CursorShow(false);
        }

        protected override void OnMouseLeave()
        {
            cursorShow = false;
            //CursorShow(true);
        }

        #endregion

        #region OnKey

        protected override void OnKeyDown(Keys keys)
        {
            base.OnKeyDown(keys);
            if (keys == Keys.F2)
            {
                // Включить сервер по сети
                GameNetRun(OptionsMvk.IpAddress, 32021);
                ScreenCreate(new ScreenDebug(this));
            }
            if (keys == Keys.F3)
            {
                // Включить сервер локальный
                GameLocalRun();
                ScreenCreate(new ScreenDebug(this));
            }
            else if (keys == Keys.F4)
            {
                // Пауза
                if (Game != null)
                {
                    Game.SetGamePauseSingle(!Game.IsGamePaused);
                }
            }
            else if (keys == Keys.F5)
            {
                // Остановить сервер
                GameStoping();
            }
            else if (keys == Keys.Space)
            {
                audio.PlaySound(0, 0, 0, 0, 1, 1);
            }
            else if (keys == Keys.Enter)
            {
                SoundClick(1);
            }

            if (Game != null)
            {
                Game.TrancivePacket(new PacketC04PlayerPosition(keys.ToString()));
            }

            //map.ContainsKey(keys);
            //textDb = "d* " + keys.ToString();// + " " + Convert.ToString(lParam.ToInt32(), 2);
        }

        protected override void OnKeyUp(Keys keys)
        {
            base.OnKeyUp(keys);
            //textDb = "up " + keys.ToString();
        }

        protected override void OnKeyPress(char key)
        {
            try
            {
                //textDb += key;
                return;
            }
            catch (Exception ex)
            {
                return;
            }
        }

        #endregion

        #region On...

        protected override void OnOpenGLInitialized()
        {
            base.OnOpenGLInitialized();

            //gl.ShadeModel(GL.GL_SMOOTH);
            //gl.ClearColor(0.0f, .5f, 0.0f, 1f);
            //gl.Clear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);
            //gl.ClearDepth(1.0f);
            //gl.Enable(GL.GL_DEPTH_TEST);
            gl.DepthFunc(GL.GL_LEQUAL);
            //gl.Hint(GL.GL_PERSPECTIVE_CORRECTION_HINT, GL.GL_NICEST);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized(int width, int height)
        {
            base.OnResized(width, height);
            if (renderMvk != null)
            {
                renderMvk.FontLarge.UpdateSizeInterface();
                renderMvk.FontSmall.UpdateSizeInterface();
            }
        }

        #endregion

        #region WindowOverride

        /// <summary>
        /// Инициализаця объекта рендера
        /// </summary>
        protected override void RenderInitialized()
        {
            Render = renderMvk = new RenderMvk(this);
            renderMvk.InitializeFirst();
            gl.ClearColor(1, 1, 1, 1);
        }

        #endregion

        #region Screen

        /// <summary>
        /// Создать скрин заставки
        /// </summary>
        public override void ScreenSplash() => ScreenCreate(new ScreenSplashMvk(this));
        /// <summary>
        /// Создать скрин главного меню
        /// </summary>
        public override void ScreenMainMenu() => ScreenCreate(new ScreenDebug(this));

        #endregion

        #region Game

        protected override void Game_Tick(object sender, EventArgs e)
        {
            if (Screen != null && Screen is ScreenDebug screenDebug)
            {
                if (screenDebug.xx2++ > 900) screenDebug.xx2 = 0;
            }
        }

        #endregion

        #region Sound

        /// <summary>
        /// Звук клика
        /// </summary>
        public override void SoundClick(float volume) => audio.PlaySound(1, 0, 0, 0, volume, 1);

        #endregion
    }
}
