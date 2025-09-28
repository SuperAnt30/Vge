using Mvk2.Renderer;
using Vge.Gui;

namespace Mvk2.Gui
{
    /// <summary>
    /// Объект для скрина, для прорисовки всплывающей подсказки Малювеки 2
    /// </summary>
    public class ToolTipMvk : ToolTip
    {
        private readonly WindowMvk _windowMvk;

        protected readonly RenderMvk _render;

        public ToolTipMvk(WindowMvk window) 
            : base(window, window.GetRender().FontSmall, 25, 25)
        {
            _windowMvk = window;
            _render = _windowMvk.GetRender();
        }


    }
}
