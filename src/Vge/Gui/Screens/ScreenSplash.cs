using System;
using Vge.Realms;
using Vge.Renderer;
using Vge.Util;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Заставка
    /// </summary>
    public class ScreenSplash : ScreenBase
    {
        /// <summary>
        /// Шагов выполнено и прорисованно
        /// </summary>
        protected int countDraw = 0;

        private readonly MeshGuiColor meshProcess;
        private readonly ListFlout list = new ListFlout();
        private readonly int max;

        /// <summary>
        /// Объект загрузчика
        /// </summary>
        protected Loading loading;
        /// <summary>
        /// Шагов выполнено
        /// </summary>
        private int countStep = 0;
        /// <summary>
        /// Был ли финиш
        /// </summary>
        private bool isFinish = false;

        public ScreenSplash(WindowMain window) : base(window)
        {
            LoadingCreate();
            max = loading.GetMaxCountSteps();
            loading.Step += Loading_Step;
            loading.Finish += Loading_Finish;
            loading.Starting();
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
        /// Объвление объекта загрузки
        /// </summary>
        protected virtual void LoadingCreate() => loading = new Loading(window);

        private void Loading_Finish(object sender, EventArgs e)
            => isFinish = true;

        private void Loading_Step(object sender, EventArgs e)
            => countStep++;

        /// <summary>
        /// Игровой такт
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void OnTick(float deltaTime)
        {
            if (isFinish)
            {
                window.Render.AtFinishLoading(loading.Buffereds);
                loading.Buffereds.Clear();
                window.Render.DeleteTextureSplash();
                window.Readed();
                window.LScreen.MainMenu();
            }
            else
            {
                if (countStep != countDraw)
                {
                    countDraw = countStep;
                    // Тут рендер ползунка
                    RenderStep();
                }
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
            int wcl = countDraw * 600 / max;
            list.AddRange(MeshGuiColor.Rectangle(w - 300, h - 32, w - 300 + wcl, h - 8, .13f, .44f, .91f));

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
            gl.ClearColor(1, 1, 1, 1);
            window.Render.ShaderBindGuiColor();
            window.Render.BindTextureSplash();
            DrawLogo();
            meshProcess.Draw();
        }

        public override void Dispose()
        {
            base.Dispose();
            meshProcess.Dispose();
        }
    }
}
