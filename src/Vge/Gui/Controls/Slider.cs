using System.Collections.Generic;
using Vge.Renderer;
using Vge.Renderer.Font;
using WinGL.Actions;
using WinGL.Util;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Контрол ползунка
    /// </summary>
    public class Slider : Label
    {
        /// <summary>
        /// Ширина ползунка
        /// </summary>
        private const int SliderWidth = 16;
        /// <summary>
        /// Вертикаль к текстуре 200 / 512
        /// </summary>
        private const float Ver1 = .390625f;
        /// <summary>
        /// Вертикаль к текстуре 240 / 512
        /// </summary>
        private const float Ver2 = .46875f;
        /// <summary>
        /// Для смещения горизонтали 240 / 512
        /// </summary>
        private const float Hor1 = .46875f;
        /// <summary>
        /// Для смещения горизонтали 16 / 512
        /// </summary>
        private const float Hor = .03125f;

        /// <summary>
        /// Значение
        /// </summary>
        public int Value { get; private set; }
        /// <summary>
        /// Минимальное значение
        /// </summary>
        public int Min { get; private set; }
        /// <summary>
        /// Максимальное значение
        /// </summary>
        public int Max { get; private set; }
        /// <summary>
        /// Шаг
        /// </summary>
        public int Step { get; private set; }
        /// <summary>
        /// Параметры для текстовки
        /// </summary>
        private readonly Dictionary<int, string> items = new Dictionary<int, string>();

        /// <summary>
        /// Зажата ли левая клавиша мыши
        /// </summary>
        private bool isLeftDown = false;

        /// <summary>
        /// Сетка фона
        /// </summary>
        private readonly MeshGuiColor meshBg;
        /// <summary>
        /// Сетка кнопки
        /// </summary>
        private readonly MeshGuiColor meshBt;

        public Slider(WindowMain window, FontBase font, int width, int min, int max, int step, string text)
            : base(window, font, width, 40, text)
        {
            meshBg = new MeshGuiColor(gl);
            meshBt = new MeshGuiColor(gl);
            Min = min;
            Max = max;
            Step = step;
        }

        /// <summary>
        /// Задать значение
        /// </summary>
        public Slider SetValue(int value)
        {
            Value = value;
            IsRender = true;
            return this;
        }

        /// <summary>
        /// Задать параметр для текстовки
        /// </summary>
        /// <param name="value">значение</param>
        /// <param name="text">текст при значении</param>
        public Slider AddParam(int value, string text)
        {
            items.Add(value, text);
            return this;
        }

        #region Draw

        /// <summary>
        /// Прорисовка внутреняя
        /// </summary>
        /// <param name="x">Позиция X с учётом интерфейса</param>
        /// <param name="y">Позиция Y с учётом интерфейса</param>
        protected override void RenderInside(RenderMain render, int x, int y)
        {
            string textParam = items.ContainsKey(Value) ? items[Value] : "";
            string text = textParam == "" ? (Text + " " + Value) : textParam;
             
            // Рендер текста
            RenderInside(render, x, y, text);

            float v1 = Enabled ? enter ? vk + vk : vk : 0f;

            // Рендер фона
            meshBg.Reload(RectangleTwo(x, y, 0, 0, vk, 1, 1, 1));

            v1 = Hor1 + (Enabled ? isLeftDown ? Hor + Hor : Hor : 0f);

            // Рендер кнопки
            float mm = Max - Min;
            float index = mm == 0 ? 0 : (Value - Min) / mm;
            int px = (int)((Width - SliderWidth) * index) * si;
            meshBt.Reload(Rectangle(x + px, y, x + px + SliderWidth * si, y + Height * si,
                v1, Ver1, v1 + Hor, Ver2, 1, 1, 1));
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            // Рисуем фон кнопки
            window.Render.BindTextureWidgets();
            meshBg.Draw();
            if (enter)
            {
                // Рисуем кнопку
                meshBt.Draw();
                // Рисуем текст кнопки
                base.Draw(timeIndex);
            }
            else
            {
                // Рисуем текст кнопки
                base.Draw(timeIndex);
                // Рисуем кнопку
                window.Render.BindTextureWidgets();
                meshBt.Draw();
            }
        }

        #endregion

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
                    isLeftDown = true;
                    CheckMouse(x);
                }
            }
        }
        /// <summary>
        /// Отпущена клавиша мышки
        /// </summary>
        public override void OnMouseUp(MouseButton button, int x, int y)
        {
            if (button == MouseButton.Left && isLeftDown)
            {
                isLeftDown = false;
                OnMouseMove(x, y);
                CheckMouse(x);
            }
        }

        /// <summary>
        /// Перемещение мышки
        /// </summary>
        public override void OnMouseMove(int x, int y)
        {
            base.OnMouseMove(x, y);
            if (isLeftDown) CheckMouse(x);
        }

        /// <summary>
        /// Проверка по активации мышки
        /// </summary>
        /// <param name="x">координата мыши по X</param>
        private void CheckMouse(int x)
        {
            float xm = (x / si - PosX - 4) / (float)(Width - 8);
            if (xm < 0) xm = 0;
            if (xm > 1) xm = 1;

            Value = Mth.Round(((Max - Min) * xm + Min) / Step);
            Value *= Step;

            IsRender = true;
        }

        #endregion

        public override void Dispose()
        {
            base.Dispose();
            if (meshBg != null) meshBg.Dispose();
            if (meshBt != null) meshBt.Dispose();
        }
    }
}

