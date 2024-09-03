using System;
using WinGL.OpenGL;
using WinGL.Actions;
using System.Reflection;
using Vge;
using Mvk2.Util;
using Mvk2.Audio;
using Mvk2.Renderer;
using Vge.Util;
using Vge.Games;
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

        public WindowMvk() : base() { }

        protected override void Initialized()
        {
            Version = "Test VBO by Ant " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            

            // TODO::2024-08-22 продумать перезагрузку опций
            new OptionsFileMvk().Load();
            new OptionsFileMvk().Save();

            audio = new AudioMvk();
            // Инициализация звука и загрузка семплов
            audio.Initialize(2);
            audio.InitializeSample();
        }

        protected override void Game_Tick(object sender, EventArgs e)
        {
            if (renderMvk.xx2++ > 900) renderMvk.xx2 = 0;
        }

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
        /// Инициализаця объекта рендера
        /// </summary>
        protected override void RenderInitialized()
        {
            Render = renderMvk = new RenderMvk(this);
            renderMvk.InitializeFirst();
        }

        //protected override void OnResized(int width, int height)
        //{
        //    base.OnResized(width, height);
        //}

        /// <summary>
        /// Прорисовка кадра
        /// </summary>
        //protected override void OnOpenGlDraw()
        //{
        //    base.OnOpenGlDraw();
            
        //    gl.Clear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);
        //    gl.Enable(GL.GL_DEPTH_TEST);
        //    // группа для сглаживания, но может жутко тормазить
        //    gl.BlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);
        //    gl.ClearColor(.7f, .4f, .4f, 1f);
        //    gl.Enable(GL.GL_BLEND);

        //    renderMvk.Draw();
        //}

        /// <summary>
        /// Стабильный игровой такт
        /// </summary>
        protected override void OnTick()
        {
            base.OnTick();


            //int x = renderMvk.xx;
            //x -= 100;
            //if (x < 0) x = 0;
            //renderMvk.xx = x;
        }

        protected override void OnKeyDown(Keys keys)
        {
            base.OnKeyDown(keys);
            if (keys == Keys.F2)
            {
                // Включить сервер по сети
                GameNetRun(OptionsMvk.IpAddress, 32021);
            }
            if (keys == Keys.F3)
            {
                // Включить сервер локальный
                GameLocalRun();
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
                audio.PlaySound(1, 0, 0, 0, 1, 1);
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

        #region Screen

        /// <summary>
        /// Создать скрин по индексу
        /// </summary>
        public override void ScreenMainMenu() => Screen = new ScreenDebug(this);

        #endregion
    }
}
