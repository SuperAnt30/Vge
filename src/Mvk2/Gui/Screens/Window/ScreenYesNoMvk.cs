using Mvk2.Renderer;
using System.Runtime.CompilerServices;
using Vge.Gui.Screens;

namespace Mvk2.Gui.Screens
{
    public class ScreenYesNoMvk : ScreenYesNo
    {
        private readonly RenderMvk _renderMvk;

        public ScreenYesNoMvk(WindowMvk window, ScreenBase parent, string text) 
            : base(window, parent, text)
            => _renderMvk = window.GetRender();

        /// <summary>
        /// Запустить текстуру фона
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _BindTextureBg() => _renderMvk.BindTextureWindowSmall();
    }
}
