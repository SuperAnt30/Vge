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
        protected int _countDraw = 0;

        private readonly MeshGuiColor _meshProcess;
        private readonly ListFlout _list = new ListFlout();
        private readonly int _max;

        /// <summary>
        /// Объект загрузчика
        /// </summary>
        protected Loading _loading;
        /// <summary>
        /// Шагов выполнено
        /// </summary>
        private int _countStep = 0;
        /// <summary>
        /// Был ли финиш
        /// </summary>
        private bool _isFinish = false;

        public ScreenSplash(WindowMain window) : base(window)
        {
            _LoadingCreate();
            _max = _loading.GetMaxCountSteps();
            _loading.Step += _Loading_Step;
            _loading.Finish += _Loading_Finish;
            _loading.Starting();
            _meshProcess = new MeshGuiColor(window.GetOpenGL());
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void _OnResized()
        {
            _RenderBegin();
            _RenderStep();
        }

        /// <summary>
        /// Объвление объекта загрузки
        /// </summary>
        protected virtual void _LoadingCreate() => _loading = new Loading(window);

        private void _Loading_Finish(object sender, EventArgs e) => _isFinish = true;

        private void _Loading_Step(object sender, EventArgs e) => _countStep++;

        /// <summary>
        /// Игровой такт
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void OnTick(float deltaTime)
        {
            if (_isFinish)
            {
                window.Render.AtFinishLoading(_loading.Buffereds);
                _loading.Buffereds.Clear();
                window.Render.DeleteTextureSplash();
                window.Readed();
                window.LScreen.MainMenu();
            }
            else
            {
                if (_countStep != _countDraw)
                {
                    _countDraw = _countStep;
                    // Тут рендер ползунка
                    _RenderStep();
                }
            }
        }

        /// <summary>
        /// Начальный рендер
        /// </summary>
        protected virtual void _RenderBegin() { }

        /// <summary>
        /// Рендер загрузчика шага
        /// </summary>
        protected virtual void _RenderStep()
        {
            int w = Gi.Width / 2;
            int h = (Gi.Height - 608 * _si) / 2 + 512 * _si;
            _list.Clear();
            int w2 = 308 * _si;
            _list.AddRange(RenderFigure.Rectangle(w - w2, h - 40 * _si, w + w2, h, .13f, .44f, .91f));
            w2 = 304 * _si;
            _list.AddRange(RenderFigure.Rectangle(w - w2, h - 36 * _si, w + w2, h - 4 * _si, 1, 1, 1));
            int wcl = _countDraw * 600 * _si / _max;
            w2 = 300 * _si;
            _list.AddRange(RenderFigure.Rectangle(w - w2, h - 32 * _si, w - w2 + wcl * _si, h - 8 * _si, .13f, .44f, .91f));

            _meshProcess.Reload(_list.GetBufferAll(), _list.Count);
        }

        /// <summary>
        /// Логотип
        /// </summary>
        protected virtual void _DrawLogo() { }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            gl.ClearColor(1, 1, 1, 1);
            window.Render.ShaderBindGuiColor();
            window.Render.BindTextureSplash();
            _DrawLogo();
            _meshProcess.Draw();
        }

        public override void Dispose()
        {
            base.Dispose();
            _meshProcess.Dispose();
        }
    }
}
