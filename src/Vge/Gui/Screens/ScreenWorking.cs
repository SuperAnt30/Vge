using Vge.Renderer;
using Vge.Util;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран выполнения работы
    /// </summary>
    public class ScreenWorking : ScreenBase
    {
        /// <summary>
        /// Шагов выполнено и прорисованно
        /// </summary>
        protected int countDraw = 0;

        private readonly MeshGuiColor meshProcess;
        private readonly ListFlout list = new ListFlout();
        private int max = -1;

        /// <summary>
        /// Шагов выполнено
        /// </summary>
        private int countStep = 0;

        public ScreenWorking(WindowMain window) : base(window)
        {
            meshProcess = new MeshGuiColor(window.GetOpenGL());
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            RenderBegin();
            RenderStep();
        }

        /// <summary>
        /// Игровой такт
        /// </summary>
        public override void OnTick(float deltaTime)
        {
            if (countStep != countDraw)
            {
                countDraw = countStep;
                // Тут рендер ползунка
                RenderStep();
            }
        }

        /// <summary>
        /// Начальный рендер
        /// </summary>
        protected virtual void RenderBegin() { }

        /// <summary>
        /// Рендер загрузчика шага
        /// </summary>
        protected virtual void RenderStep()
        {
            int w = window.Width / 2;
            int h = (window.Height - 608) / 2 + 512;
            list.Clear();
            list.AddRange(MeshGuiColor.Rectangle(w - 308, h - 40, w + 308, h, .13f, .44f, .91f));
            list.AddRange(MeshGuiColor.Rectangle(w - 304, h - 36, w + 304, h - 4, 1, 1, 1));
            if (max > 0)
            {
                int wcl = countDraw * 600 / max;
                list.AddRange(MeshGuiColor.Rectangle(w - 300, h - 32, w - 300 + wcl, h - 8, .13f, .44f, .91f));
            }

            meshProcess.Reload(list.GetBufferAll(), list.Count);
        }

        /// <summary>
        /// Логотип
        /// </summary>
        protected virtual void DrawLogo() { }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
          //  gl.ClearColor(1, 1, 1, 1);
            window.Render.ShaderBindGuiColor();
            window.Render.BindTextureSplash();
            DrawLogo();
            window.Render.TextureDisable();
            meshProcess.Draw();
            window.Render.TextureEnable();
        }

        public override void Dispose()
        {
            base.Dispose();
            meshProcess.Dispose();
        }

        #region Server

        /// <summary>
        /// Начало загрузки, получаем количество шагов
        /// </summary>
        public void ServerBegin(int count) => max = count;

        /// <summary>
        /// Шаг загрузки
        /// </summary>
        public void ServerStep() => countStep++;

        #endregion
    }
}
