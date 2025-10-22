using Mvk2.Renderer;
using System.Runtime.CompilerServices;
using Vge.Gui.Screens;

namespace Mvk2.Gui.Screens
{
    /// <summary>
    /// Экран оповещения, ошибки
    /// </summary>
    public class ScreenNotificationMvk : ScreenNotification
    {
        private readonly RenderMvk _renderMvk;

        public ScreenNotificationMvk(WindowMvk window, string notification) 
            : base(window, notification) => _renderMvk = window.GetRender();

        /// <summary>
        /// Запустить текстуру фона
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _BindTextureBg() => _renderMvk.BindTextureWindowSmall();
    }
}
