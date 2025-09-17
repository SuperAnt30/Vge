using Mvk2;
using Vge.Renderer;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Скрин окна чата
    /// </summary>
    public class ScreenChatMvk : ScreenChat
    {
        /// <summary>
        /// Сетка фона
        /// </summary>
        private readonly MeshGuiColor _meshBg;

        private readonly WindowMvk _windowMvk;

        public ScreenChatMvk(WindowMvk window) : base(window)
        {
            _windowMvk = window;
            _meshBg = new MeshGuiColor(gl);
            // Размер окна
            WidthWindow = 512;
            HeightWindow = 354;
        }

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            // Расположение окна
            PosX = 8;
            PosY = Height - HeightWindow - 8;
            //PosX = 100;
            //PosY = Height / 4;
            base.OnResized();
            _isRenderAdd = true;
        }

        protected override void _RenderingAdd()
        {
            _meshBg.Reload(RenderFigure.Rectangle(PosX * si, PosY * si,
                (PosX + WidthWindow) * si, (PosY + HeightWindow) * si,
                0, 0, 1, HeightWindow / (float)WidthWindow));
            _isRenderAdd = false;
        }

        protected override void _DrawAdd()
        {
            _windowMvk.GetRender().BindTextureChat();
            _meshBg.Draw();
        }

        public override void Dispose()
        {
            base.Dispose();
            if (_meshBg != null) _meshBg.Dispose();
        }
    }
}
