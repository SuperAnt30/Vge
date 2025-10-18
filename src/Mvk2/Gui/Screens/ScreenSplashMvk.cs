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
        private readonly MeshGuiColor meshLogo;
        
        public ScreenSplashMvk(WindowMvk window) : base(window)
        {
            meshLogo = new MeshGuiColor(window.GetOpenGL());
        }

        /// <summary>
        /// Объвление объекта загрузки
        /// </summary>
        protected override void LoadingCreate() => loading = new LoadingMvk((WindowMvk)window);

        /// <summary>
        /// Начальный рендер
        /// </summary>
        protected override void RenderBegin()
        {
            int w = Gi.Width / 2;
            int h = (Gi.Height - 608 * _si) / 2;
            int wh = 512 * _si;
            meshLogo.Reload(RenderFigure.Rectangle(w - wh, h, w + wh, h + wh, 0, 0, 1, 1));
        }

        /// <summary>
        /// Логотип
        /// </summary>
        protected override void DrawLogo() => meshLogo.Draw();
    }
}
