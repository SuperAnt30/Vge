﻿using System.Diagnostics;
using Vge.Renderer;

namespace Vge.Gui.Controls
{
    public class Button : Label
    {
        private Mesh2d meshBg;

        /// <summary>
        /// Коэфициент смещения вертикали для текстуры
        /// </summary>
        private readonly float vk;

        public Button(WindowMain window, int width, int height, string text)
            : base(window, width, height, text)
        {
            vk = height / 512f;
        }

        public override void Initialize()
        {
            base.Initialize();
            meshBg = new Mesh2d(gl);
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

        public override void Draw(float timeIndex)
        {
            // Рисуем фон кнопки
            window.Render.BindTexutreWidgets();
            meshBg.Draw();
            // Рисуем текст кнопки
            base.Draw(timeIndex);
        }

        #endregion
    }
}