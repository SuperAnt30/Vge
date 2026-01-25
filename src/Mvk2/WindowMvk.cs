using WinGL.Actions;
using System.Reflection;
using Vge;
using Mvk2.Util;
using Mvk2.Audio;
using Mvk2.Renderer;
using Mvk2.Gui.Screens;
using Vge.Renderer.Font;
using Vge.Renderer;
using Vge.World;
using Mvk2.World;
using WinGL.Util;
using Mvk2.World.Block;
using Mvk2.Entity;
using Vge.Games;
using Mvk2.Games;
using Mvk2.Item;
using Vge.Item;
using Mvk2.World.BlockEntity;

namespace Mvk2
{
    public class WindowMvk : WindowMain
    {
        /// <summary>
        /// Объект отвечающий за прорисовку Малювек
        /// </summary>
        private RenderMvk _renderMvk;

        /// <summary>
        /// Объект запуска экрана Mvk
        /// </summary>
        public LaunchScreenMvk LScreenMvk { get; private set; }

        #region Initialized

        public WindowMvk() : base()
        {
            Title = "Малювекi 2 - debug";
            // Ширина наружного окна, HD+
          //  _width = 1600;
            // Высота наружного окна, HD+
            //_height = 900;
        }

        protected override void Initialized()
        {
            Version = "Test Малювек2 by SuperAnt ver. " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            LScreen = LScreenMvk = new LaunchScreenMvk(this);

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
        public RenderMvk GetRender() => _renderMvk;

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
                    Render.FontMain.SetFontFX(EnumFontFX.Shadow).SetColor(new Vector3(.9f, .9f, .9f));
                    Render.FontMain.RenderText(10 * Gi.Si, 10 * Gi.Si, textDebug);
                    Render.FontMain.RenderFX();
                    Render.FontMain.Reload(meshTextDebug);
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
            if (Ce.IsDebugDraw)
            {
                textDebug = debug.ToText();
                isTextDebug = true;
            }
        }

        #endregion

        #region OnMouse

        protected override void OnMouseDown(MouseButton button, int x, int y)
        {
            base.OnMouseDown(button, x, y);
        }

        protected override void OnMouseUp(MouseButton button, int x, int y)
        {
            base.OnMouseUp(button, x, y);
        }

        protected override void OnMouseEnter()
        {
            base.OnMouseEnter();
            //cursorShow = true;
            //CursorShow(false);
        }

        protected override void OnMouseLeave()
        {
            //CursorShow(true);
        }

        #endregion

        #region OnKey

        protected override void OnKeyDown(Keys keys)
        {
            base.OnKeyDown(keys);
            //if (keys == Keys.Space)
            //{
            //    audio.PlaySound(0, 0, 0, 0, 1, 1);
            //}
            //else if (keys == Keys.Enter)
            //{
            //    SoundClick(1);
            //}
        }

        #endregion

        #region On...

        protected override void OnOpenGLInitialized()
        {
            base.OnOpenGLInitialized();

            meshTextDebug = new MeshGuiColor(gl);
        }

        /// <summary>
        /// Изменить размер интерфейса
        /// </summary>
        public override void UpdateSizeInterface()
        {
            base.UpdateSizeInterface();
            if (_renderMvk != null)
            {
                _renderMvk.FontLarge?.UpdateSizeInterface();
                _renderMvk.FontSmall?.UpdateSizeInterface();
            }
        }

        #endregion

        #region WindowOverride

        /// <summary>
        /// Инициализаця объекта рендера
        /// </summary>
        protected override void RenderInitialized()
        {
            Render = _renderMvk = new RenderMvk(this);
            _renderMvk.InitializeFirst();
            // Цвет белы, фон загрузчика
            gl.ClearColor(1, 1, 1, 1);
        }

        #endregion

        #region Game

        /// <summary>
        /// Создание миров
        /// </summary>
        protected override AllWorlds _CreateAllWorlds() => new AllWorldsMvk();

        /// <summary>
        /// Инициализация блоков
        /// </summary>
        protected override void _InitializationBlocks()
        {
            // Затать количество спрайтов в длинну и ширину и размер спрайта
            Ce.SetSpriteAtlasSize(64, 16);
            base._InitializationBlocks();
            BlocksRegMvk.Initialization();
        }

        /// <summary>
        /// Инициализация предметов
        /// </summary>
        protected override void _InitializationItems()
        {
            // Так-как у нас спрайты в 2 раза больше блока (32 вместо 16), то фигуру надо указать в увеличенном от блока.
            // А размер спрайта (SizeSprite) не менять, оставить 16, он в атласе занимает 2*2=4 спрайта.
            ItemsReg.SizeShape = 32; 
            base._InitializationItems();
            ItemsRegMvk.Initialization();
        }

        /// <summary>
        /// Инициализация сущностей
        /// </summary>
        protected override void _InitializationEntities()
        {
            base._InitializationEntities();
            EntitiesRegMvk.Initialization();
        }

        /// <summary>
        /// Инициализация блоков сущностей
        /// </summary>
        protected override void _InitializationBlocksEntity()
        {
            base._InitializationBlocksEntity();
            BlocksEntityRegMvk.Initialization();
        }

        /// <summary>
        /// Создаём клиентский игровой мод 
        /// </summary>
        protected override GameModClient _CreateGameModClient() => new GameModClientMvk(this);

        /// <summary>
        /// Создаём сетевой игровой мод 
        /// </summary>
        public override GameModServer CreateGameModServer() => new GameModServerMvk();

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
