using Mvk2.Renderer;
using Vge.Gui;
using Vge.Renderer;

namespace Mvk2.Gui
{
    /// <summary>
    /// Объект для скрина, для прорисовки всплывающей подсказки Малювеки 2
    /// </summary>
    public class ToolTipMvk : ToolTip
    {
        private readonly WindowMvk _windowMvk;

        protected readonly RenderMvk _render;

        /// <summary>
        /// Сетка фона
        /// </summary>
        private readonly MeshGuiColor _meshBg;
        /// <summary>
        /// Ситка бордюра
        /// </summary>
        private readonly MeshGuiLine _meshLine;

        private int _biasX;
        private int _biasY;

        public ToolTipMvk(WindowMvk window) 
            : base(window, window.GetRender().FontSmall, 25, 25)
        {
            _windowMvk = window;
            _render = _windowMvk.GetRender();
            _meshBg = new MeshGuiColor(window.GetOpenGL());
            _meshLine = new MeshGuiLine(window.GetOpenGL());
        }

        /// <summary>
        /// Рендер контрола
        /// </summary>
        protected override void _Rendering()
        {
            base._Rendering();
            int w = _sizeText.X + 16 * Gi.Si;
            int h = _sizeText.Y + 12 * Gi.Si;
            _meshBg.Reload(RenderFigure.RectangleSolid(0, 0, w, h, .31f, .26f, .21f, .9f));
            _meshLine.Reload(RenderFigure.RectangleLine(0, 0, w, h, .89f, .78f, .66f));
            _biasX = 8 * Gi.Si;
            _biasY = 8 * Gi.Si;
        }

        protected override void _Draw()
        {
            _render.FontMain.BindTexture();
            _render.ShaderBindGuiColor(_mouseX - _biasX, _mouseY - _biasY);
            _meshBg.Draw();
            _render.ShaderBindGuiLine(_mouseX - _biasX, _mouseY - _biasY);
            _meshLine.Draw();
            _render.ShaderBindGuiLine(0, 0);
            base._Draw();
        }

        public override void Dispose()
        {
            base.Dispose();
            _meshBg?.Dispose();
            _meshLine?.Dispose();
        }
    }
}
