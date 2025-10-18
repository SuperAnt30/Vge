using System.Collections.Generic;
using Vge.Gui.Controls;
using Vge.Network;
using WinGL.Actions;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Абстрактный класс экрана
    /// </summary>
    public abstract class ScreenBase : Warp
    {
        /// <summary>
        /// Колекция всех контролов
        /// </summary>
        private readonly List<WidgetBase> controls = new List<WidgetBase>();
        /// <summary>
        /// Размер интерфеса
        /// </summary>
        protected int _si = 1;

        /// <summary>
        /// Нужен ли дополнительный рендер
        /// </summary>
        protected bool _isRenderAdd = true;

        /// <summary>
        /// Объект подсказки
        /// </summary>
        protected ToolTip _toolTip;

        public ScreenBase(WindowMain window) : base(window) => _si = Gi.Si;

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        public void Initialize()
        {
            OnInitialize();
            Resized();
        }

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// Запуск от родителя
        /// </summary>
        public virtual void LaunchFromParent(EnumScreenParent enumParent) { }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        public void Resized()
        {
            _si = Gi.Si;
            foreach (WidgetBase control in controls)
            {
                control.OnResized();
            }
            OnResized();
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected virtual void OnResized() { }

        public void AddControls(WidgetBase control) => controls.Add(control);

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            window.Render.ShaderBindGuiColor();

            if (_isRenderAdd)
            {
                _RenderingAdd();
            }
            _DrawAdd();

            foreach (WidgetBase control in controls)
            {
                if (control.Visible)
                {
                    if (control.IsRender)
                    {
                        control.Rendering();
                    }
                    control.Draw(timeIndex);
                }
            }

            //_toolTip?.Draw();
        }

        /// <summary>
        /// Дополнительный рендер не контролов
        /// </summary>
        protected virtual void _RenderingAdd() { }
        /// <summary>
        /// Дополнительная прорисовка не контролов
        /// </summary>
        protected virtual void _DrawAdd() { }

        #region Tick

        /// <summary>
        /// Игровой такт
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void OnTick(float deltaTime)
        {
            foreach (WidgetBase control in controls)
            {
                if (control.Visible && control.Enabled) control.OnTick(deltaTime);
            }
        }

        #endregion

        /// <summary>
        /// Получить ширину, с перерасчётом для инерфейса
        /// </summary>
        public int Width => Gi.Width / _si;
        /// <summary>
        /// Получить высоту, с перерасчётом для инерфейса
        /// </summary>
        public int Height => Gi.Height / _si;

        public override void Dispose()
        {
            base.Dispose();
            foreach (WidgetBase control in controls)
            {
                control.Dispose();
            }
            _toolTip?.Dispose();
        }

        #region OnMouse

        /// <summary>
        /// Перемещение мыши
        /// </summary>
        public override void OnMouseMove(int x, int y)
        {
            bool tt = _toolTip != null;
            bool ttCheck = false;
            foreach (WidgetBase control in controls)
            {
                if (control.Visible && control.Enabled)
                {
                    control.OnMouseMove(x, y);
                    if (tt && !ttCheck && control.Enter)
                    {
                        ttCheck = true;
                        _toolTip.SetText(control.GetToolTip());
                    }
                }
            }
            if (tt)
            {
                _toolTip.OnMouseMove(x, y);
                if (!ttCheck) _toolTip.Hide();
            }
        }

        /// <summary>
        /// Нажатие курсора мыши
        /// </summary>
        public override void OnMouseDown(MouseButton button, int x, int y)
        {
            foreach (WidgetBase control in controls)
            {
                if (control.Visible && control.Enabled) control.OnMouseDown(button, x, y);
            }
        }

        /// <summary>
        /// Отпустил курсор мыши
        /// </summary>
        public override void OnMouseUp(MouseButton button, int x, int y)
        {
            foreach (WidgetBase control in controls)
            {
                if (control.Visible && control.Enabled) control.OnMouseUp(button, x, y);
            }
        }

        /// <summary>
        /// Вращение колёсика мыши
        /// </summary>
        public override void OnMouseWheel(int delta, int x, int y)
        {
            foreach (WidgetBase control in controls)
            {
                if (control.Visible && control.Enabled) control.OnMouseWheel(delta, x, y);
            }
        }

        #endregion

        #region OnKey

        /// <summary>
        /// Клавиша нажата
        /// </summary>
        public override void OnKeyDown(Keys keys)
        {
            foreach (WidgetBase control in controls)
            {
                if (control.Visible && control.Enabled && control.Focus) control.OnKeyDown(keys);
            }
        }

        /// <summary>
        /// Клавиша отпущена
        /// </summary>
        public override void OnKeyUp(Keys keys)
        {
            foreach (WidgetBase control in controls)
            {
                if (control.Visible && control.Enabled && control.Focus) control.OnKeyUp(keys);
            }
        }

        /// <summary>
        /// Нажата клавиша в char формате
        /// </summary>
        public override void OnKeyPress(char key)
        {
            foreach (WidgetBase control in controls)
            {
                if (control.Visible && control.Enabled && control.Focus) control.OnKeyPress(key);
            }
        }

        #endregion

        /// <summary>
        /// Получить сетевой пакет
        /// </summary>
        public virtual void AcceptNetworkPackage(IPacket packet) { }
    }
}
