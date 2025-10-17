using System;
using Vge.Renderer;
using Vge.Renderer.Font;
using WinGL.Actions;
using WinGL.Util;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Поля для редактирования текста
    /// </summary>
    public class TextBox : WidgetBase
    {
        /// <summary>
        /// Смещение от начала рамки до текста
        /// </summary>
        private const int _marginLeft = 12;

        /// <summary>
        /// Сетка фона
        /// </summary>
        private readonly MeshGuiColor _meshBg;
        /// <summary>
        /// Сетка текста
        /// </summary>
        private readonly MeshGuiColor _meshTxt;
        /// <summary>
        /// Сетка курсора
        /// </summary>
        private readonly MeshGuiColor _meshCursor;
        /// <summary>
        /// Коэфициент смещения вертикали для текстуры
        /// </summary>
        private readonly float _vk;
        /// <summary>
        /// Ограничения набор символов 
        /// </summary>
        private readonly EnumRestrictions _restrictions;
        /// <summary>
        /// Объект шрифта
        /// </summary>
        private readonly FontBase _font;
        /// <summary>
        /// Максимальная длинна 
        /// </summary>
        private readonly int _limit;

        /// <summary>
        /// Счётчик для анимации
        /// </summary>
        private int _cursorCounter;
        /// <summary>
        /// Где стоит курсор номер символа
        /// </summary>
        private int _stepCursor = 0;
        /// <summary>
        /// Видимость курсора
        /// </summary>
        private bool _isVisibleCursor;
        /// <summary>
        /// Фиксированный фокус, к примеру для чата
        /// </summary>
        private bool _fixFocus = false;
        /// <summary>
        /// Нажата ли клавиша контрол
        /// </summary>
        private bool _keyControl = false;

        public TextBox(WindowMain window, FontBase font, int width, string text,
            EnumRestrictions restrictions, int limit = 24) : base(window)
        {
            _limit = limit;
            _font = font;
            _restrictions = restrictions;
            _meshBg = new MeshGuiColor(gl);
            _meshTxt = new MeshGuiColor(gl);
            _meshCursor = new MeshGuiColor(gl);
            _vk = .078125f; // 40 / 512f;
            SetText(_GetConvertCheck(text));
            SetSize(width, 40);
        }

        /// <summary>
        /// Сделать фиксированный вокус, чтоб не терять курсор
        /// </summary>
        public void FixFocus()
        {
            Focus = true;
            _fixFocus = true;
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
                    if (!Focus)
                    {
                        _isVisibleCursor = true;
                        Focus = true;
                    }
                    // Определяем куда установить курсор
                    UpCursor(x);
                    IsRender = true;
                }
                else if (!Enter && Focus && !_fixFocus)
                {
                    // Потерять фокус
                    _isVisibleCursor = false;
                    Focus = false;
                    IsRender = true;
                }
            }
        }

        /// <summary>
        /// Определяем куда установить курсор, x клик мыши
        /// </summary>
        public void UpCursor(int x = int.MaxValue)
        {
            int x0 = x / _si - PosX;
            int w1 = _marginLeft;
            string text = _GetTextDraw();
            int count = text.Length;
            _stepCursor = count;
            for (int i = 1; i <= count; i++)
            {
                w1 = _font.WidthString(text.Substring(0, i));
                if (w1 > x0)
                {
                    _stepCursor = i - 1;
                    break;
                }
            }
            int bias = Text.Length - text.Length;
            if (bias > 0)
            {
                _stepCursor += bias;
            }
        }

        #endregion

        #region OnKey

        public override void OnKeyPress(char key)
        {
            int id = Convert.ToInt32(key);
            if (id == 8)
            {
                // back
                if (_stepCursor > 0)
                {
                    string text = Text.Substring(0, _stepCursor - 1);
                    if (Text.Length > _stepCursor)
                    {
                        text += Text.Substring(_stepCursor);
                    }
                    _stepCursor--;
                    SetText(text);
                }
            }
            else if (Text.Length < _limit && _Check(key, id))
            {
                string text = Text.Substring(0, _stepCursor) + key;
                if (Text.Length > _stepCursor)
                {
                    text += Text.Substring(_stepCursor);
                }
                _stepCursor++;
                SetText(text);
            }
        }

        public override void OnKeyUp(Keys keys)
        {
            if (keys == Keys.ControlKey) _keyControl = false;
        }

        public override void OnKeyDown(Keys keys)
        {
            if (keys == Keys.ControlKey)
            {
                _keyControl = true;
            }
            else if (keys == Keys.V && _keyControl)
            {
                string textClipboard = Clipboard.GetText();
                if (textClipboard != null && textClipboard != "")
                {
                    textClipboard = _GetConvertCheck(textClipboard);
                    if (textClipboard != "")
                    {
                        string message;
                        if (_stepCursor == Text.Length)
                        {
                            message = Text + textClipboard;
                        }
                        else if (_stepCursor == 0)
                        {
                            message = textClipboard + Text;
                        }
                        else
                        {
                            message = Text.Substring(0, _stepCursor - 1)
                                + textClipboard
                                + Text.Substring(_stepCursor);
                        }

                        if (message.Length > _limit)
                        {
                            message = message.Substring(0, _limit);
                        }
                        _stepCursor += textClipboard.Length;
                        if (_stepCursor > _limit)
                        {
                            _stepCursor = _limit;
                        }
                        SetText(message);
                    }
                }
            }
            else if (keys == Keys.Delete)
            {
                if (_stepCursor < Text.Length)
                {
                    string text = "";
                    if (_stepCursor > 0)
                    {
                        text = Text.Substring(0, _stepCursor);
                    }
                    if (Text.Length > _stepCursor)
                    {
                        text += Text.Substring(_stepCursor + 1);
                    }
                    SetText(text);
                }
            }
            else if (keys == Keys.Left)
            {
                if (_stepCursor > 0)
                {
                    _stepCursor--;
                    IsRender = true;
                }
            }
            else if (keys == Keys.Right)
            {
                if (_stepCursor < Text.Length)
                {
                    _stepCursor++;
                    IsRender = true;
                }
            }
            else if (keys == Keys.Home)
            {
                if (_stepCursor > 0)
                {
                    _stepCursor = 0;
                    IsRender = true;
                }
            }
            else if (keys == Keys.End)
            {
                if (_stepCursor < Text.Length)
                {
                    _stepCursor = Text.Length;
                    IsRender = true;
                }
            }
            else if (keys == Keys.Up)
            {
                KeyUpDown(true);
            }
            else if (keys == Keys.Down)
            {
                KeyUpDown(false);
            }
        }

        /// <summary>
        /// Действие клавиши Up или Down
        /// </summary>
        protected virtual void KeyUpDown(bool up) { }

        /// <summary>
        /// Проверить и сконвертировать сообщение по текущие правила
        /// </summary>
        private string _GetConvertCheck(string message)
        {
            string s = "";
            char key;
            for(int i = 0; i < message.Length; i++)
            {
                key = message[i];
                if (s.Length < _limit && _Check(key, Convert.ToInt32(key)))
                {
                    s += key;
                }
            }
            return s;
        }

        private bool _Check(char key, int id)
        {
            switch (_restrictions)
            {
                case EnumRestrictions.IpPort:
                    return (id >= 48 && id <= 57) // цифры
                    || id == 46 || id == 58; // точка и двое точие
                case EnumRestrictions.Number:
                    return id >= 48 && id <= 57; // цифры
                case EnumRestrictions.Name:
                    return (id >= 48 && id <= 57) // цифры
                    || (id >= 65 && id <= 90) // Большие англ
                    || (id >= 97 && id <= 122); // Маленькие англ
                case EnumRestrictions.All:
                    return _font.IsPresent(key);
            }
            return false;
        }

        #endregion

        #region Draw

        public override void Rendering()
        {
            RenderInside(window.Render, PosX * _si, PosY * _si);
            IsRender = false;
        }


        private string _GetTextDraw() => _font.GetStringEndToWidth(Text, Width - 24);

        /// <summary>
        /// Прорисовка внутреняя
        /// </summary>
        /// <param name="x">Позиция X с учётом интерфейса</param>
        /// <param name="y">Позиция Y с учётом интерфейса</param>
        protected virtual void RenderInside(RenderMain render, int x, int y)
        {
            // Определяем цвет текста
            Vector3 color = Enabled ? Enter ? Gi.ColorTextEnter : Gi.ColorText : Gi.ColorTextInactive;
            int biasY = (Height * _si - _font.GetVert()) / 2;

            // Чистим буфер
            _font.Clear();
            // Указываем опции
            _font.SetColor(color).SetFontFX(EnumFontFX.Outline);

            // Обрезка текста согласно ширины
            string text = _GetTextDraw();
            int bias = Text.Length - text.Length;
            int lenght = _stepCursor - bias;
            if (lenght < 0)
            {
                // Тут надо подумать как смещать курсор в начало, 
                // перемещая текст, это меняем text и перерасчёт верхних строк.
                // Может это и не надо!... 2024-12-17
                _stepCursor -= lenght;
                lenght = 0;
            }
            // Готовим рендер текста
            _font.RenderString(x + _marginLeft * _si, y + biasY, text);

            // Имеется Outline значит рендерим FX
            _font.RenderFX();
            // Вносим сетку
            _font.Reload(_meshTxt);

            // Сетка фона
            float v1 = Enabled ? Enter ? _vk * 4 : _vk * 3 : 0f;
            _meshBg.Reload(_RectangleTwo(x, y, 0, v1 + .5f, _vk, 1, 1, 1));

            try
            {
                if (_isVisibleCursor)
                {
                    // Если нужен курсор, то рендерим сетку
                    int w = (PosX + _font.WidthString(text.Substring(0, lenght)) + _marginLeft) * _si;
                    // Чистим буфер
                    _font.Clear();
                    // Указываем опции
                    _font.SetColor(color).SetFontFX(EnumFontFX.Outline);
                    // Готовим рендер текста
                    _font.RenderString(w, y + biasY, _stepCursor == Text.Length ? "_" : "|");
                    // Имеется Outline значит рендерим FX
                    _font.RenderFX();
                    // Вносим сетку
                    _font.Reload(_meshCursor);
                }
            }
            catch(Exception ex)
            {
                 return;
            }
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            // Рисуем фон
            window.Render.BindTextureWidgets();
            _meshBg.Draw();
            // Рисуем текст
            _font.BindTexture();
            _meshTxt.Draw();

            // Рисуем контур если имеется
            if (_isVisibleCursor)
            {
                _meshCursor.Draw();
            }
        }

        #endregion

        /// <summary>
        /// Игровой такт
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void OnTick(float deltaTime)
        {
            if (Focus)
            {
                _cursorCounter++;

                if ((_cursorCounter >> 4) % 2 == 0)
                {
                    if (!_isVisibleCursor)
                    {
                        _isVisibleCursor = true;
                        IsRender = true;
                    }
                }
                else
                {
                    if (_isVisibleCursor)
                    {
                        _isVisibleCursor = false;
                        IsRender = true;
                    }
                }
            }
        }

        public override void Dispose()
        {
            if (_meshTxt != null) _meshTxt.Dispose();
            if (_meshBg != null) _meshBg.Dispose();
            if (_meshCursor != null) _meshCursor.Dispose();
        }

        /// <summary>
        /// Ограничения набор символов 
        /// </summary>
        public enum EnumRestrictions
        {
            /// <summary>
            /// Для ip. Цифры, точка и двоеточие
            /// </summary>
            IpPort,
            /// <summary>
            /// Только цифры
            /// </summary>
            Number,
            /// <summary>
            /// Цифры и английские буквы
            /// </summary>
            Name,
            /// <summary>
            /// Все доступные по графике
            /// </summary>
            All
        }
    }
}
