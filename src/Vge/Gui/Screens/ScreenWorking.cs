using Vge.Gui.Controls;
using Vge.Realms;
using Vge.Renderer;
using Vge.Util;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран выполнения работы
    /// </summary>
    public class ScreenWorking : ScreenBase
    {
        protected readonly Label _label;

        /// <summary>
        /// Шагов выполнено и прорисованно
        /// </summary>
        protected int _countDraw = 0;

        private readonly MeshGuiColor _meshProcess;
        private readonly ListFlout _list = new ListFlout();
        private int _max = -1;

        /// <summary>
        /// Шагов выполнено
        /// </summary>
        private int _countStep = 0;

        public ScreenWorking(WindowMain window, string text) : base(window)
        {
            _label = new Label(window, window.Render.FontMain, 320, 64,
                ChatStyle.Bolb + text + ChatStyle.Reset);
            _label.SetTextAlight(EnumAlight.Center, EnumAlightVert.Middle);

            _meshProcess = new MeshGuiColor(window.GetOpenGL());
        }

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void _OnInitialize()
        {
            base._OnInitialize();
            _AddControls(_label);
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
        /// Игровой такт
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void OnTick(float deltaTime)
        {
            if (_countStep != _countDraw)
            {
                _countDraw = _countStep;
                // Тут рендер ползунка
                _RenderStep();
            }
        }

        /// <summary>
        /// Начальный рендер
        /// </summary>
        protected virtual void _RenderBegin()
        {
            _label.SetPosition(Width / 2 - 160, Height / 2);
        }

        /// <summary>
        /// Рендер загрузчика шага
        /// </summary>
        protected virtual void _RenderStep()
        {
            int w = Gi.Width / 2;
            int h = (Gi.Height - 608 * _si) / 2 + 512 * _si;
            _list.Clear();
            int w2 = 308 * _si;
            _list.AddRange(RenderFigure.Rectangle(w - w2, h - 40 * _si, w + w2, h, .314f, .384f, .427f));
            w2 = 304 * _si;
            _list.AddRange(RenderFigure.Rectangle(w - w2, h - 36 * _si, w + w2, h - 4 * _si, 1, 1, 1));

            if (_max > 0)
            {
                int wcl = _countDraw * 600 / _max;
                w2 = 300 * _si;
                _list.AddRange(RenderFigure.Rectangle(w - w2, h - 32 * _si, w - w2 + wcl * _si, h - 8 * _si, .314f, .384f, .427f));
            }

            _meshProcess.Reload(_list.GetBufferAll(), _list.Count);
        }

        protected override void _DrawAdd()
        {
            window.Render.ShaderBindGuiColor();
            window.Render.FontMain.BindTexture();
            _meshProcess.Draw();
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.486f, .569f, .616f, 1f);
            base.Draw(timeIndex);
        }

        public override void Dispose()
        {
            base.Dispose();
            _meshProcess?.Dispose();
        }

        #region Server

        /// <summary>
        /// Начало загрузки, получаем количество шагов
        /// </summary>
        public void ServerBegin(int count) => _max = count;

        /// <summary>
        /// Шаг загрузки
        /// </summary>
        public void ServerStep() => _countStep++;

        #endregion
    }
}
