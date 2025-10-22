using Mvk2;
using Mvk2.Renderer;
using System.Runtime.CompilerServices;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран выбора одиночной игры
    /// </summary>
    public class ScreenSingleMvk : ScreenSingle
    {
        private readonly RenderMvk _renderMvk;

        public ScreenSingleMvk(WindowMvk window) : base(window)
            => _renderMvk = window.GetRender();

        /// <summary>
        /// Запустить текстуру фона
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _BindTextureBg() => _renderMvk.BindTextureWindowBig();
    }
}
