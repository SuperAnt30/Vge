using System.Collections.Generic;
using Vge.Gui.Controls;
using WinGL.Actions;
using WinGL.OpenGL;

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
        protected int si = 1;

        public ScreenBase(WindowMain window) : base(window) => si = Gi.Si;

        /// <summary>
        /// Получить объект OpenGL
        /// </summary>
        public GL GetOpenGL() => gl;

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
        /// Изменён размер окна
        /// </summary>
        public void Resized()
        {
            si = Gi.Si;
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

        public override void Draw(float timeIndex)
        {
            base.Draw(timeIndex);

            // TODO:: это можно подумать как заменить для 2д т.е. GUI
            window.Render.ShaderBindGuiColor();

            foreach (WidgetBase control in controls)
            {
                if (control.IsRender)
                {
                    control.Render();
                }
                control.Draw(timeIndex);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (WidgetBase control in controls)
            {
                control.Dispose();
            }
        }

        #region OnMouse

        /// <summary>
        /// Перемещение мыши
        /// </summary>
        public override void OnMouseMove(int x, int y)
        {
            foreach (WidgetBase control in controls)
            {
                if (control.Visible && control.Enabled) control.OnMouseMove(x, y);
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

    }
}
