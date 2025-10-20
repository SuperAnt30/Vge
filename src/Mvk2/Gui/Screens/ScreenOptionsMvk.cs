using Mvk2.Renderer;
using System.Runtime.CompilerServices;
using Vge.Gui.Screens;

namespace Mvk2.Gui.Screens
{
    public class ScreenOptionsMvk : ScreenOptions
    {
        private readonly RenderMvk _renderMvk;

        public ScreenOptionsMvk(WindowMvk window, ScreenBase parent, bool inGame) 
            : base(window, parent, inGame)
            => _renderMvk = window.GetRender();

        /// <summary>
        /// Запустить текстуру фона
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _BindTextureBg() => _renderMvk.BindTextureOptions();
    }
}
