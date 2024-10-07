using WinGL.OpenGL;
using WinGL.Actions;
using System.Reflection;
using Vge;
using Mvk2.Util;
using Mvk2.Audio;
using Mvk2.Renderer;
using Vge.Network.Packets.Client;
using Mvk2.Gui.Screens;
using Vge.Renderer.Font;
using Vge.Renderer;
using Vge.World;
using Mvk2.World;
using WinGL.Util;
using Vge.Games;

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
            LScreen = new LaunchScreenMvk(this);

            // Загружаем опции
            OptionsLoad();
            // Объявление объекта звуков, загрузка семплов в loadinge
            audio = new AudioMvk();
        }

        /// <summary>
        /// Прочесть настройки
        /// </summary>
        protected override void OptionsLoad() => new OptionsFileMvk().Load();
        /// <summary>
        /// Записать настройки
        /// </summary>
        public override void OptionsSave() => new OptionsFileMvk().Save();

        #endregion

        /// <summary>
        /// Получить объект рендера Млювек
        /// </summary>
        public RenderMvk GetRender() => renderMvk;

        #region Debug

        private string textDebug = "";
        private bool isTextDebug = false;
        private MeshGuiColor meshTextDebug;

        protected override void DrawDebug()
        {
            if (Ready)
            {
                if (isTextDebug)
                {
                    isTextDebug = false;
                    Render.FontMain.Clear();
                    Render.FontMain.SetFontFX(EnumFontFX.Shadow).SetColor(new System.Numerics.Vector3(.9f, .9f, .9f));
                    Render.FontMain.RenderText(10 * Gi.Si, 10 * Gi.Si, textDebug);
                    Render.FontMain.RenderFX();
                    meshTextDebug.Reload(Render.FontMain.ToBuffer());
                    Render.FontMain.Clear();
                }
                // Прорисовка отладки
                Render.ShaderBindGuiColor();
                Render.FontMain.BindTexture();
                meshTextDebug.Draw();
            }
        }

        protected override void OnTick()
        {
            base.OnTick();
            // Отладка на экране
            textDebug = debug.ToText();
            isTextDebug = true;
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
            if (keys == Keys.Space)
            {
                audio.PlaySound(0, 0, 0, 0, 1, 1);
            }
            else if (keys == Keys.Enter)
            {
                SoundClick(1);
            }

            if (Game != null)
            {
                bool b = false;
                if (keys == Keys.Left)
                {
                    Game.Player.chPos.X-=1;
                    b = true;
                }
                else if (keys == Keys.Right)
                {
                    Game.Player.chPos.X+=2;
                    b = true;
                }
                else if (keys == Keys.Up)
                {
                    Game.Player.chPos.Y-=1;
                    b = true;
                }
                else if (keys == Keys.Down)
                {
                    Game.Player.chPos.Y+=1;
                    b = true;
                }
                if (b)
                {
                    Game.TrancivePacket(new PacketC04PlayerPosition(
                        new System.Numerics.Vector3(Game.Player.chPos.X, Game.Player.chPos.Y, 0),
                        false, false, false));
                    Debug.player = Game.Player.chPos;
                }

                if (keys == Keys.PageUp)
                {
                    if (Game.Player.OverviewChunk < 49)
                    {
                        Game.Player.SetOverviewChunk((byte)(Game.Player.OverviewChunk + 1));
                        Game.TrancivePacket(new PacketC15PlayerSetting(Game.Player.OverviewChunk));
                    }
                }
                else if (keys == Keys.PageDown)
                {
                    if (Game.Player.OverviewChunk > 2)
                    {
                        Game.Player.SetOverviewChunk((byte)(Game.Player.OverviewChunk - 1));
                        Game.TrancivePacket(new PacketC15PlayerSetting(Game.Player.OverviewChunk));
                    }
                }
            }

            //map.ContainsKey(keys);
            //textDb = "d* " + keys.ToString();// + " " + Convert.ToString(lParam.ToInt32(), 2);
        }

        #endregion

        #region On...

        protected override void OnOpenGLInitialized()
        {
            base.OnOpenGLInitialized();

            meshTextDebug = new MeshGuiColor(gl);

            //gl.ShadeModel(GL.GL_SMOOTH);
            //gl.ClearColor(0.0f, .5f, 0.0f, 1f);
            //gl.Clear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);
            //gl.ClearDepth(1.0f);
            //gl.Enable(GL.GL_DEPTH_TEST);
            gl.DepthFunc(GL.GL_LEQUAL);
            //gl.Hint(GL.GL_PERSPECTIVE_CORRECTION_HINT, GL.GL_NICEST);
        }

        /// <summary>
        /// Изменить размер интерфейса
        /// </summary>
        public override void UpdateSizeInterface()
        {
            base.UpdateSizeInterface();
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
            // Цвет белы, фон загрузчика
            gl.ClearColor(1, 1, 1, 1);
        }

        #endregion

        #region Game

        /// <summary>
        /// Создание миров
        /// </summary>
        protected override AllWorlds CreateAllWorlds() => new AllWorldsMvk();

        #endregion

        #region Sound

        /// <summary>
        /// Получить объект звука
        /// </summary>
        public AudioMvk GetAudio() => (AudioMvk)audio;

        /// <summary>
        /// Звук клика
        /// </summary>
        public override void SoundClick(float volume) => audio.PlaySound(1, 0, 0, 0, volume, 1);

        #endregion
    }
}
