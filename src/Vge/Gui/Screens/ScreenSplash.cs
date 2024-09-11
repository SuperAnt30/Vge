using System;
using Vge.Realms;

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
            loading.Step += Loading_Step;
            loading.Finish += Loading_Finish;
            loading.Starting();
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
        protected virtual void LoadingCreate() => loading = new Loading();

        private void Loading_Finish(object sender, EventArgs e)
            => isFinish = true;

        private void Loading_Step(object sender, EventArgs e)
            => countStep++;

        /// <summary>
        /// Игровой такт
        /// </summary>
        public override void OnTick(float deltaTime)
        {
            if (isFinish)
            {
                window.Render.DeleteTextureSplash();
                window.ScreenMainMenu();
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
        protected virtual void RenderStep() { }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex) { }
    }
}
