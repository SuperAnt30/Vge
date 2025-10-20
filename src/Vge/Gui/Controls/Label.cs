using System;
using Vge.Renderer;
using Vge.Renderer.Font;
using WinGL.Actions;
using WinGL.Util;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Текстовая метка, на которую можно нажать
    /// </summary>
    public class Label : WidgetBase
    {
        /// <summary>
        /// Выравнивания текста по горизонтали
        /// </summary>
        public EnumAlight TextAlight { get; protected set; } = EnumAlight.Center;
        /// <summary>
        /// Выравнивания текста по вертикали
        /// </summary>
        public EnumAlightVert TextAlightVert { get; protected set; } = EnumAlightVert.Middle;
        /// <summary>
        /// Объект шрифта
        /// </summary>
        public FontBase Font { get; protected set; }

        /// <summary>
        /// Может ли быть несколько строк 
        /// </summary>
        protected bool _multiline = false;
        /// <summary>
        /// Ограничение по высоте, если включено, текст за рамку не выйдет
        /// </summary>
        protected bool _limitationHeight = false;

        private readonly MeshGuiColor _meshTxt;
        private readonly MeshGuiLine _meshLine;
        

        /// <summary>
        /// Коэфициент смещения вертикали для текстуры
        /// </summary>
        protected readonly float vk = .078125f; // 40 / 512f;

        /// <summary>
        /// Текстовая метка, на которую можно нажать
        /// </summary>
        /// <param name="isLine">Нужен ли контур, для отладки</param>
        public Label(WindowMain window, FontBase font, bool isLine = false) : base(window)
        {
            Font = font;
            _meshTxt = new MeshGuiColor(gl);
            if (isLine)
            {
                _meshLine = new MeshGuiLine(gl);
            }
        }
        /// <summary>
        /// Текстовая метка, на которую можно нажать
        /// </summary>
        /// <param name="isLine">Нужен ли контур, для отладки</param>
        public Label(WindowMain window, FontBase font, string text, bool isLine = false)
            : this(window, font, isLine) => SetText(text);

        /// <summary>
        /// Текстовая метка, на которую можно нажать
        /// </summary>
        /// <param name="isLine">Нужен ли контур, для отладки</param>
        public Label(WindowMain window, FontBase font, int width, int height, string text, bool isLine = false)
            : this(window, font, text, isLine)
        {
            SetSize(width, height);
            //vk = height / 512f;
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
                if (Enter)
                {
                    _OnClick();
                }
            }
        }

        #endregion

        #region Draw

        public override void Rendering()
        {
            _RenderInside(window.Render, PosX * _si, PosY * _si);
            IsRender = false;
        }

        /// <summary>
        /// Прорисовка внутреняя
        /// </summary>
        /// <param name="x">Позиция X с учётом интерфейса</param>
        /// <param name="y">Позиция Y с учётом интерфейса</param>
        protected virtual void _RenderInside(RenderMain render, int x, int y)
            => _RenderInside(render, x, y, Text);

        protected void _RenderInside(RenderMain render, int x, int y, string text)
        {
            // Определяем цвет текста
            Vector3 color = Enabled ? Enter ? Gi.ColorTextEnter : Gi.ColorText : Gi.ColorTextInactive;
            // Чистим буфер
            Font.Clear();
            // Указываем опции
            Font.SetColor(color);

            if (Enabled)
            {
                Font.SetFontFX(EnumFontFX.None);
            }
            else
            {
                Font.SetFontFX(EnumFontFX.Shadow).SetColorShadow(new Vector3(1));
            }

            int biasX = 0;
            int biasY = 0;

            if (_multiline)
            {
                // Обрезка текста согласно ширины
                text = Font.TransferWidth(text, Width);
                // Определяем смещение
                if (TextAlight == EnumAlight.Left & !_limitationHeight)
                {
                    // Определяем смещение BiasY
                    if (TextAlightVert == EnumAlightVert.Middle)
                    {
                        biasY = (Height * _si - (Font.Transfer.GetStringsCount() * Font.GetVertStep())) / 2;
                    }
                    else if (TextAlightVert == EnumAlightVert.Bottom)
                    {
                        biasY = Height * _si - (Font.Transfer.GetStringsCount() * Font.GetVertStep());
                    }
                    // Готовим рендер текста
                    Font.RenderText(x + biasX, y + _si + biasY, text);
                }
                else
                {
                    // Готовим рендер текста
                    // Кажду строку отдельно перепроверять по смещению
                    string[] vs = Font.Transfer.GetStrings();
                    int count = vs.Length;
                    if (_limitationHeight)
                    {
                        // Если есть ограничение по высоте, надо отрезать часть строк
                        int max = (Height - Font.GetVert()) / Font.GetVertStep() + 1;
                        if (max < count)
                        {
                            count = max;
                            if (count < 1) count = 1;
                            vs[count - 1] = Font.TransferString(vs[count - 1], Width, false) + Ce.Ellipsis;
                        }
                    }
                    if (TextAlightVert == EnumAlightVert.Middle)
                    {
                        biasY = (Height * _si - (count * Font.GetVertStep())) / 2;
                    }
                    else if (TextAlightVert == EnumAlightVert.Bottom)
                    {
                        biasY = Height * _si - (count * Font.GetVertStep());
                    }
                    int center = TextAlight == EnumAlight.Center ? 2 : 1;
                    bool left = TextAlight == EnumAlight.Left;
                    string s;
                    for (int i = 0; i < count; i++)
                    {
                        s = vs[i];
                        biasX = left ? 0 : (Width - Font.WidthString(s)) * _si / center;
                        // Готовим рендер текста
                        Font.RenderString(x + biasX, y + biasY, s);
                        y += Font.GetVertStep();
                    }
                }
            }
            else
            {
                // Обрезка текста согласно ширины
                text = Font.TransferString(text, Width);
                // Определяем смещение BiasX
                if (TextAlight == EnumAlight.Center)
                {
                    biasX = (Width - Font.WidthString(text)) / 2 * _si;
                }
                else if (TextAlight == EnumAlight.Right)
                {
                    biasX = (Width - Font.WidthString(text)) * _si;
                }
                // Определяем смещение BiasY
                if (TextAlightVert == EnumAlightVert.Middle)
                {
                    biasY = (Height * _si - Font.GetVert()) / 2;
                }
                else if (TextAlightVert == EnumAlightVert.Bottom)
                {
                    biasY = Height * _si - Font.GetVert();
                }
                // Готовим рендер текста
                Font.StyleReset();
                Font.RenderString(x + biasX, y + biasY, text);
            }

            // Имеется Outline значит рендерим FX
            if (!Enabled)
            {
                Font.RenderFX();
            }
            // Вносим сетку
            Font.Reload(_meshTxt);

            // Если нужен контур, то рендерим сетку
            if (_meshLine != null)
            {
                _meshLine.Reload(RenderFigure.RectangleLine(PosX * _si, PosY * _si, (PosX + Width) * _si, (PosY + Height) * _si,
                    0, 0, 0, .5f));
            }
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            // Рисуем контур если имеется
            if (_meshLine != null)
            {
                // И заменить шейдёр
                window.Render.ShaderBindGuiLine();
                _meshLine.Draw();
                window.Render.ShaderBindGuiColor();
            }
            // Рисуем текст
            Font.BindTexture();
            _meshTxt.Draw();
        }

        #endregion

        #region Set...

        /// <summary>
        /// Заменить шрифт
        /// </summary>
        public Label SetFont(FontBase font)
        {
            Font = font;
            IsRender = true;
            return this;
        }

        /// <summary>
        /// Задать выравнивание текста
        /// </summary>
        public Label SetTextAlight(EnumAlight alight, EnumAlightVert alightVert)
        {
            TextAlight = alight;
            TextAlightVert = alightVert;
            IsRender = true;
            return this;
        }

        /// <summary>
        /// Включить поддержу несколько строк 
        /// </summary>
        public Label Multiline()
        {
            _multiline = true;
            return this;
        }

        /// <summary>
        /// Ограничение по высоте, текст за рамку не выйдет
        /// </summary>
        public Label LimitationHeight()
        {
            _limitationHeight = true;
            return this;
        }

        #endregion

        public override void Dispose()
        {
            _meshLine?.Dispose();
            _meshTxt?.Dispose();
        }

        #region Event

        /// <summary>
        /// Событие клика по кнопке
        /// </summary>
        public event EventHandler Click;
        /// <summary>
        /// Событие клика по кнопке
        /// </summary>
        protected virtual void _OnClick() => Click?.Invoke(this, new EventArgs());

        #endregion
    }
}
