using Vge.Renderer;
using Vge.Renderer.Font;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Контрол кнопки
    /// </summary>
    public class Button : Label
    {
        /// <summary>
        /// Сетка фона
        /// </summary>
        private readonly MeshGuiColor meshBg;

        public Button(WindowMain window, FontBase font, int width, string text)
            : base(window, font, width, 40, text)
        {
            meshBg = new MeshGuiColor(gl);
        }

        #region Draw

        /// <summary>
        /// Прорисовка внутреняя
        /// </summary>
        /// <param name="x">Позиция X с учётом интерфейса</param>
        /// <param name="y">Позиция Y с учётом интерфейса</param>
        protected override void RenderInside(RenderMain render, int x, int y)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            //for (int i = 0; i < 1000; i++)
            //{
                base.RenderInside(render, x, y);
                float v1 = Enabled ? enter ? vk + vk : vk : 0f;
                meshBg.Reload(_RectangleTwo(x, y, 0, v1, vk, 1, 1, 1));
            //}
            //stopwatch.Stop();
            //string s = stopwatch.ElapsedMilliseconds.ToString();
            return;
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            // Рисуем фон кнопки
            window.Render.BindTextureWidgets();
            meshBg.Draw();
            // Рисуем текст кнопки
            base.Draw(timeIndex);
        }

        #endregion

        protected override void OnClick()
        {
            // Звук клика
            window.SoundClick(.3f);
            base.OnClick();
        }

        public override void Dispose()
        {
            base.Dispose();
            if (meshBg != null) meshBg.Dispose();
        }

    }
}
