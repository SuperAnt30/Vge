using Mvk2.Renderer;
using System.Runtime.CompilerServices;
using Vge.Gui.Screens;

namespace Mvk2.Gui.Screens
{
    public class ScreenMultiplayerMvk : ScreenMultiplayer
    {
        private readonly RenderMvk _renderMvk;

        public ScreenMultiplayerMvk(WindowMvk window) : base(window)
            => _renderMvk = window.GetRender();

        /// <summary>
        /// Запустить текстуру фона
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _BindTextureBg() => _renderMvk.BindTextureOptions();
    }
}
