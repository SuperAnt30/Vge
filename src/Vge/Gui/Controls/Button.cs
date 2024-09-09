using Vge.Renderer;

namespace Vge.Gui.Controls
{
    public class Button : Label
    {
        private Mesh2d meshBg;

        public Button(WindowMain window, int width, int height, string text)
            : base(window, width, height, text) { }

        public override void Initialize()
        {
            base.Initialize();
            meshBg = new Mesh2d(gl);
        }

        #region Draw

        /// <summary>
        /// Прорисовка внутреняя
        /// </summary>
        /// <param name="x">Позиция X с учётом интерфейса</param>
        /// <param name="y">Позиция Y с учётом интерфейса</param>
        protected override void RenderInside(RenderMain render, int x, int y)
        {
            base.RenderInside(render, x, y);
            float v1 = Enabled ? enter ? .15625f : .078125f : 0f;
            meshBg.Reload(RectangleTwo(x, y, 0, v1, 1, 1, 1));
        }

        public override void Draw(float timeIndex)
        {
            // Рисуем фон кнопки
            window.Render.BindTexutreWidgets();
            meshBg.Draw();
            // Рисуем текст кнопки
            base.Draw(timeIndex);
        }

        #endregion
    }
}
