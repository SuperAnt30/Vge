using Vge.Renderer;
using Vge.Renderer.Font;

namespace Vge.Gui.Controls
{
    public class Button : Label
    {
        private readonly MeshGuiColor meshBg;

        /// <summary>
        /// Коэфициент смещения вертикали для текстуры
        /// </summary>
        private readonly float vk;

        public Button(WindowMain window, FontBase font, int width, string text)
            : base(window, font, width, 40, text)
        {
            meshBg = new MeshGuiColor(gl);
            vk = .078125f; // 40 / 512f;
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
                meshBg.Reload(RectangleTwo(x, y, 0, v1, vk, 1, 1, 1));
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
