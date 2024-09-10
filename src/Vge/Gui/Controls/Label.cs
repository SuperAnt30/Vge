using System;
using System.Diagnostics;
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

        public override void OnMouseMove(int x, int y)
        {
            base.OnMouseMove(x, y);
            if (enter)
            {
                IsRender = true;
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
            Vector3 color = Enabled ? enter ? new Vector3(.9f, .9f, .5f) : new Vector3(.8f) : new Vector3(.5f);
            Vector3 colorBg = new Vector3(0);

            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            //for (int i = 0; i < 100; i++)
            {

                render.FontMain.Clear();
                int biasX = (Width - render.FontMain.WidthString(Text)) / 2 * si;
                //int biasX = 0;

                render.FontMain.SetColor(color, colorBg).SetFontFX(Renderer.Font.EnumFontFX.Outline);
                render.FontMain.RenderString(x + biasX, y + (Height - 16) / 2 * si, Text);
                render.FontMain.RenderFX();

            }

            //stopwatch.Stop();
            //string s = ((float)(stopwatch.ElapsedTicks / (Stopwatch.Frequency / 1000f))).ToString("0.000");
            //render.FontWidget.Clear();
            //render.FontWidget.SetColor(color, colorBg).SetFontFX(Renderer.Font.EnumFontFX.Outline);
            //render.FontWidget.RenderText(x, y + (Height - 16) / 2 * si, Text + " " + s);
            //render.FontWidget.RenderFX();
            render.FontMain.Reload(meshTxt);
            
            return;
        }

        public override void Draw(float timeIndex)
        {
            // Рисуем текст кнопки
            window.Render.BindTexutreFontMain();
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
