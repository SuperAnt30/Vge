using Mvk2.Realms;
using Vge.Gui.Screens;
using Vge.Renderer;

namespace Mvk2.Gui.Screens
{
    /// <summary>
    /// Заставка
    /// </summary>
    public class ScreenSplashMvk : ScreenSplash
    {
        private readonly MeshGuiColor _meshLogo;
        
        public ScreenSplashMvk(WindowMvk window) : base(window)
        {
            _meshLogo = new MeshGuiColor(window.GetOpenGL());
        }

        /// <summary>
        /// Объвление объекта загрузки
        /// </summary>
        protected override void _LoadingCreate() => _loading = new LoadingMvk((WindowMvk)window);

        /// <summary>
        /// Начальный рендер
        /// </summary>
        protected override void _RenderBegin()
        {
            int w = Gi.Width / 2;
            int h = (Gi.Height - 480 * _si) / 2;
            int w2 = 256 * _si;
            int h2 = 384 * _si;
            _meshLogo.Reload(RenderFigure.Rectangle(w - w2, h, w + w2, h + h2, 0, 0, 1, .75f));
        }

        /// <summary>
        /// Логотип
        /// </summary>
        protected override void _DrawLogo() => _meshLogo.Draw();
    }
}
