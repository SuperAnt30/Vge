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
        private readonly Dictionary<int, string> _items = new Dictionary<int, string>();

        /// <summary>
        /// Зажата ли левая клавиша мыши
        /// </summary>
        private bool _isLeftDown = false;

        /// <summary>
        /// Сетка фона
        /// </summary>
        private readonly MeshGuiColor _meshBg;
        /// <summary>
        /// Сетка кнопки
        /// </summary>
        private readonly MeshGuiColor _meshBt;

        public Slider(WindowMain window, FontBase font, int width, int min, int max, int step, string text)
            : base(window, font, width, 40, text)
        {
            _meshBg = new MeshGuiColor(gl);
            _meshBt = new MeshGuiColor(gl);
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
            _items.Add(value, text);
            return this;
        }

        #region Draw

        /// <summary>
        /// Прорисовка внутреняя
        /// </summary>
        /// <param name="x">Позиция X с учётом интерфейса</param>
        /// <param name="y">Позиция Y с учётом интерфейса</param>
        protected override void _RenderInside(RenderMain render, int x, int y)
        {
            string textParam = _items.ContainsKey(Value) ? _items[Value] : "";
            string text = textParam == "" ? (Text + " " + Value) : textParam;
             
            // Рендер текста
            _RenderInside(render, x, y, text);

            float v1 = Enabled ? Enter ? vk + vk : vk : 0f;

            // Рендер фона
            _meshBg.Reload(_RectangleTwo(x, y, 0, 0, vk, 1, 1, 1));

            v1 = Hor1 + (Enabled ? _isLeftDown ? Hor + Hor : Hor : 0f);

            // Рендер кнопки
            float mm = Max - Min;
            float index = mm == 0 ? 0 : (Value - Min) / mm;
            int px = (int)((Width - SliderWidth) * index) * _si;
            _meshBt.Reload(RenderFigure.Rectangle(x + px, y, x + px + SliderWidth * _si, y + Height * _si,
                v1, Ver1, v1 + Hor, Ver2));
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            // Рисуем фон кнопки
            window.Render.BindTextureWidgets();
            _meshBg.Draw();
            if (Enter)
            {
                // Рисуем кнопку
                _meshBt.Draw();
                // Рисуем текст кнопки
                base.Draw(timeIndex);
            }
            else
            {
                // Рисуем текст кнопки
                base.Draw(timeIndex);
                // Рисуем кнопку
                window.Render.BindTextureWidgets();
                _meshBt.Draw();
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
                if (Enter)
                {
                    _isLeftDown = true;
                    CheckMouse(x);
                }
            }
        }
        /// <summary>
        /// Отпущена клавиша мышки
        /// </summary>
        public override void OnMouseUp(MouseButton button, int x, int y)
        {
            if (button == MouseButton.Left && _isLeftDown)
            {
                _isLeftDown = false;
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
            if (_isLeftDown) CheckMouse(x);
        }

        /// <summary>
        /// Проверка по активации мышки
        /// </summary>
        /// <param name="x">координата мыши по X</param>
        private void CheckMouse(int x)
        {
            float xm = (x / _si - PosX - 4) / (float)(Width - 8);
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
            if (_meshBg != null) _meshBg.Dispose();
            if (_meshBt != null) _meshBt.Dispose();
        }
    }
}

