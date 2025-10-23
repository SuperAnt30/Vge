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

        private int _biasX;
        private int _biasY;

        public ToolTipMvk(WindowMvk window) 
            : base(window, window.GetRender().FontSmall, 25, 25)
        {
            _windowMvk = window;
            _render = _windowMvk.GetRender();
            _meshBg = new MeshGuiColor(window.GetOpenGL());
        }

        /// <summary>
        /// Рендер контрола
        /// </summary>
        protected override void _Rendering()
        {
            base._Rendering();
            int x1 = 0;
            int y1 = 0; 
            int x2 = _sizeText.X + 16 * Gi.Si;
            int y2 = _sizeText.Y + 12 * Gi.Si;

            float a = 1f;
            float rBg = .396f;
            float gBg = .333f;
            float bBg = .38f;
            float r = 0;
            float g = 0;
            float b = 0;

            int si = Gi.Si;

            _meshBg.Reload(new float[]
            {
                // фон
                //x1, y1, 0, 0, rBg, gBg, bBg, a,
                //x1, y2, 0, 0, rBg, gBg, bBg, a,
                //x2, y2, 0, 0, rBg, gBg, bBg, a,
                //x2, y1, 0, 0, rBg, gBg, bBg, a,

                x1, y1, 0, 0, 1, 1, 1, a,
                x1, y2, 0, 0, 1, 1, 1, a,
                x2, y2, 0, 0, 1, 1, 1, a,
                x2, y1, 0, 0, 1, 1, 1, a,

                x1 + si, y1 + si, 0, 0, rBg, gBg, bBg, a,
                x1 + si, y2 - si, 0, 0, rBg, gBg, bBg, a,
                x2 - si, y2 - si, 0, 0, rBg, gBg, bBg, a,
                x2 - si, y1 + si, 0, 0, rBg, gBg, bBg, a,

                // up
                x1, y1 - si, 0, 0, r, g, b, a,
                x1, y1, 0, 0, r, g, b, a,
                x2, y1, 0, 0, r, g, b, a,
                x2, y1 - si, 0, 0, r, g, b, a,

                // up
                x1, y2, 0, 0, r, g, b, a,
                x1, y2 + si, 0, 0, r, g, b, a,
                x2, y2 + si, 0, 0, r, g, b, a,
                x2, y2, 0, 0, r, g, b, a,

                // left
                x1 - si, y1, 0, 0, r, g, b, a,
                x1 - si, y2, 0, 0, r, g, b, a,
                x1, y2, 0, 0, r, g, b, a,
                x1, y1, 0, 0, r, g, b, a,

                // right
                x2, y1, 0, 0, r, g, b, a,
                x2, y2, 0, 0, r, g, b, a,
                x2 + si, y2, 0, 0, r, g, b, a,
                x2 + si, y1, 0, 0, r, g, b, a
            });

            _biasX = 8 * Gi.Si;
            _biasY = 8 * Gi.Si;
        }

        protected override void _Draw()
        {
            _render.FontMain.BindTexture();
            _render.ShaderBindGuiColor(_mouseX - _biasX, _mouseY - _biasY);
            _meshBg.Draw();
            _render.ShaderBindGuiLine(0, 0);
            base._Draw();
        }

        public override void Dispose()
        {
            base.Dispose();
            _meshBg?.Dispose();
        }
    }
}
