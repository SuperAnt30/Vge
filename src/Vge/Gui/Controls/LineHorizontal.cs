using Vge.Renderer;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Контрол без откликов, чисто прорисовка горизонтальной линии в 2px
    /// </summary>
    public class LineHorizontal : WidgetBase
    {
        /// <summary>
        /// Сетка фона
        /// </summary>
        protected readonly MeshGuiColor _meshBg;

        public LineHorizontal(WindowMain window, int width) : base(window)
        {
            SetSize(width, 2);
            _meshBg = new MeshGuiColor(gl);
        }

        #region Draw

        public override void Rendering()
        {
            _RenderInside(window.Render, PosX * _si, PosY * _si);
            IsRender = false;
        }

        /// <summary>
        /// Прорисовка внутреняя
        /// </summary>
        /// <param name="x">Позиция X с учётом интерфейса</param>
        /// <param name="y">Позиция Y с учётом интерфейса</param>
        protected virtual void _RenderInside(RenderMain render, int x, int y)
        {
            _meshBg.Reload(RenderFigure.Rectangle(x, y, x + Width * _si, y + Height * _si,
                0, .99609375f, .5f, 1f));
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            // Рисуем фон
            window.Render.BindTextureWidgets();
            _meshBg.Draw();
        }

        #endregion

        public override void Dispose() => _meshBg?.Dispose();
    }
}
