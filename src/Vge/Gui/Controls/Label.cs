using System;
using System.Numerics;
using Vge.Renderer;
using WinGL.Actions;

namespace Vge.Gui.Controls
{
    public class Label : WidgetBase
    {
        private Mesh2d meshTxt;

        public Label(WindowMain window, int width, int height, string text) : base(window)
        {
            SetText(text);
            SetSize(width, height);
        }

        public override void Initialize()
        {
            base.Initialize();
            meshTxt = new Mesh2d(gl);
        }

        #region OnMouse

        /// <summary>
        /// Нажатие клавиши мышки
        /// </summary>
        public override void OnMouseDown(MouseButton button, int x, int y)
        {
            if (button == MouseButton.Left)
            {
                OnMouseMove(x, y);
                if (enter)
                {
                    // Звук клика
                    window.SoundClick(.3f);
                    OnClick();
                }
            }
        }

        #endregion

        #region Draw

        public override void Render()
        {
            IsRender = false;
            RenderInside(window.Render, PosX * si, PosY * si);
        }

        /// <summary>
        /// Прорисовка внутреняя
        /// </summary>
        /// <param name="x">Позиция X с учётом интерфейса</param>
        /// <param name="y">Позиция Y с учётом интерфейса</param>
        protected virtual void RenderInside(RenderMain render, int x, int y)
        {
            render.FontControl.BufferClear();
            int biasX = (Width - render.FontControl.WidthString(Text)) / 2 * si;
            // TODO::2024-09-09 Задать правила цвета
            Vector3 color = Enabled ? enter ? new Vector3(.9f, .9f, .5f) : new Vector3(.8f) : new Vector3(.5f);
            render.FontControl.RenderText(x + biasX, y + 12 * si, Text, color);
            render.FontControl.Reload(meshTxt);
        }

        public override void Draw(float timeIndex)
        {
            // Рисуем текст кнопки
            window.Render.BindTexutreFontControl();
            meshTxt.Draw();
        }

        #endregion

        #region Event

        /// <summary>
        /// Событие клика по кнопке
        /// </summary>
        public event EventHandler Click;
        /// <summary>
        /// Событие клика по кнопке
        /// </summary>
        protected virtual void OnClick() => Click?.Invoke(this, new EventArgs());

        #endregion
    }
}
