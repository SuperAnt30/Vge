using Mvk2;
using Mvk2.Renderer;
using System.Runtime.CompilerServices;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Скрин окна чата
    /// </summary>
    public class ScreenChatMvk : ScreenChat
    {
        private readonly RenderMvk _renderMvk;

        public ScreenChatMvk(WindowMvk window) : base(window)
            => _renderMvk = window.GetRender();

        /// <summary>
        /// Название заголовка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override string _GetTitle() => L.T("Chat");

        /// <summary>
        /// Запустить текстуру фона
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _BindTextureBg() => _renderMvk.BindTextureChat();
    }
}
