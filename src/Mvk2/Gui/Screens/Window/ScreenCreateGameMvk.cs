using Mvk2.Renderer;
using System.Runtime.CompilerServices;
using Vge.Gui.Screens;

namespace Mvk2.Gui.Screens
{
    public class ScreenCreateGameMvk : ScreenCreateGame
    {
        private readonly RenderMvk _renderMvk;

        public ScreenCreateGameMvk(WindowMvk window, int slot) : base(window, slot)
            => _renderMvk = window.GetRender();

        /// <summary>
        /// Запустить текстуру фона
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _BindTextureBg() => _renderMvk.BindTextureWindowSmall();
    }
}
