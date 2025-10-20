using Mvk2;
using System.Runtime.CompilerServices;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Скрин окна чата
    /// </summary>
    public class ScreenChatMvk : ScreenChat
    {
        private readonly WindowMvk _windowMvk;

        public ScreenChatMvk(WindowMvk window) : base(window, 512, 354)
        {
            _windowMvk = window;
        }

        /// <summary>
        /// Название заголовка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override string _GetTitle() => L.T("Chat");

        /// <summary>
        /// Запустить текстуру фона
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _BindTextureBg() => _windowMvk.GetRender().BindTextureChat();
    }
}
