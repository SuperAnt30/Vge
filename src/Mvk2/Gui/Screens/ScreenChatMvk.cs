using Mvk2;
using Vge.Renderer;
using Vge.Renderer.Font;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Скрин окна чата
    /// </summary>
    public class ScreenChatMvk : ScreenChat
    {
        /// <summary>
        /// Сетка фона
        /// </summary>
        private readonly MeshGuiColor _meshBg;

        private readonly WindowMvk _windowMvk;

        public ScreenChatMvk(WindowMvk window) : base(window)
        {
            _windowMvk = window;
           // FontBase font = window.Render.FontMain;
            _meshBg = new MeshGuiColor(gl);
        }

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            base.OnResized();
            _isRenderAdd = true;

        }

        protected override void _RenderingAdd()
        {
            _meshBg.Reload(_Rectangle(8 * si, (Height - 354 - 8) * si,
                (8 + 512) * si, (Height - 8) * si,
                0, 0, 1, 354 / 512f, 1, 1, 1, 1));
            _isRenderAdd = false;
        }

        /// <summary>
        /// Прямоугольник
        /// </summary>
        protected float[] _Rectangle(int x1, float y1, int x2, int y2,
            float u1, float v1, float u2, float v2,
            float r, float g, float b, float a = 1f)
        {
            return new float[]
            {
                x1, y1, u1, v1, r, g, b, a,
                x1, y2, u1, v2, r, g, b, a,
                x2, y2, u2, v2, r, g, b, a,
                x2, y1, u2, v1, r, g, b, a
            };
        }

        protected override void _DrawAdd()
        {
            _windowMvk.GetRender().BindTextureChat();
            _meshBg.Draw();
        }

        public override void Dispose()
        {
            base.Dispose();
            if (_meshBg != null) _meshBg.Dispose();
        }
    }
}
