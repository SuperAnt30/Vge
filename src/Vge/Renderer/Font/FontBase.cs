using System;
using Vge.Util;
using WinGL.OpenGL;
using WinGL.Util;

namespace Vge.Renderer.Font
{
    /// <summary>
    /// Базовый класс шрифта
    /// </summary>
    public class FontBase
    {
        /// <summary>
        /// Разбитие строк
        /// </summary>
        public readonly TransferText Transfer;

        /// <summary>
        /// Массив символов
        /// </summary>
        private readonly Symbol[] _items = new Symbol[162];
        /// <summary>
        /// Горизонтальное смещение начала следующего глифа
        /// </summary>
        private readonly int _horiAdvance;
        /// <summary>
        /// Вертикальное смещение начала следующего глифа 
        /// </summary>
        private readonly int _vertAdvance;
        /// <summary>
        /// Растояние между буквами в пикселях
        /// </summary>
        private readonly byte _stepFont;
        /// <summary>
        /// Буфер сетки данного шрифта
        /// </summary>
        private readonly ListFlout _buffer = new ListFlout();
        /// <summary>
        /// Сетка шрифта
        /// </summary>
        private Mesh _mesh;
        /// <summary>
        /// Горизонтальное смещение начала следующего глифа с учётом размера интерфейса
        /// </summary>
        private int _hori;
        /// <summary>
        /// Вертикальное смещение начала следующего глифа с учётом размера интерфейса 
        /// </summary>
        private int _vert;
        /// <summary>
        /// Размера интерфейса
        /// </summary>
        private int _si;
        /// <summary>
        /// Стаил шрифта
        /// </summary>
        private readonly FontStyle _style = new FontStyle();
        /// <summary>
        /// Цвет по умолчанию
        /// </summary>
        private Vector3 _colorText = Gi.ColorText;
        /// <summary>
        /// Эффекты к шрифту
        /// </summary>
        private EnumFontFX _fontFX = EnumFontFX.None;
        /// <summary>
        /// Объект рендера
        /// </summary>
        private readonly RenderMain _render;
        /// <summary>
        /// Индест текстурки
        /// </summary>
        private uint _texture;

        /// <summary>
        /// Класс шрифта
        /// </summary>
        /// <param name="textureFont">Объект изображения</param>
        /// <param name="stepFont">растояние между буквами в пикселях</param>
        /// <param name="render">объект рендера, только для основного потока</param>
        /// <param name="texture">индест текстурки</param>
        public FontBase(BufferedImage textureFont, byte stepFont, RenderMain render)
        {
            _render = render;
            _horiAdvance = textureFont.Width >> 4;
            _vertAdvance = textureFont.Height >> 4;
            _stepFont = stepFont;
            Transfer = new TransferText(this);

            string keys = " !\"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~ АБВГДЕЖЗИКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдежзиклмнопрстуфхцчшщъыьэюяЁёЙй§";
            char[] vc = keys.ToCharArray();
            int key;
            int index;
            byte width;
            char symb;
            for (int i = 0; i < vc.Length; i++)
            {
                symb = vc[i];
                index = keys.IndexOf(symb) + 32;
                if (index == -1)
                {
                    throw new Exception(Sr.GetString(Sr.TheSymbolIsNotInTheList, vc[i]));
                }
                width = GetWidth(textureFont, index);
                Symbol symbol = new Symbol(symb, index, width);
                key = symb;
                _items[Convert(key)] = symbol;

                //index = keys.IndexOf(symb) + 32;
                //if (index == -1)
                //{
                //    throw new Exception(Sr.GetString(Sr.TheSymbolIsNotInTheList, vc[i]));
                //}
                //width = GetWidth(textureFont, index);
                //Symbol symbol = new Symbol(symb, index, width);
                //key = symb;
                //if (key == 167) key = 100;
                //items[key - (key > 1000 ? 929 : 32)] = symbol;
            }
            UpdateSizeInterface();
        }

        /// <summary>
        /// Конвертируем index символа, на наш символ для хранения в кэше
        /// </summary>
        private int Convert(int key)
        {
            if (key < 32) return 0;
            if (key == 167) return 95; // § (Alt+21)
            if (key == 1025) return 96; // Ё
            if (key == 1105) return 97; // ё
            if (key > 1103)
            {
                throw new Exception(Sr.GetString(Sr.TheSymbolIsNotInTheList, (char)key));
            }
            if (key > 1039) return key - 942;
            if (key > 126)
            {
                throw new Exception(Sr.GetString(Sr.TheSymbolIsNotInTheList, (char)key));
            }
            return key - 32;
        }

        /// <summary>
        /// Создать меш если это надо
        /// </summary>
        public void CreateMesh(GL gl, uint index) 
        {
            _texture = index;
            _mesh = new MeshGuiColor(gl);
        }

        /// <summary>
        /// Обновить размер инерфейса
        /// </summary>
        public void UpdateSizeInterface()
        {
            _si = Gi.Si;
            _hori = _horiAdvance * _si;
            _vert = _vertAdvance * _si;
        }

        /// <summary>
        /// Забиндить текстуру шрифта
        /// </summary>
        public void BindTexture() => _render.BindTexture(_texture);

        /// <summary>
        /// Получить объект символа
        /// </summary>
        public Symbol Get(char key)
        {
            try
            {
                return _items[Convert(key)];
                //if (key < 32) return items[0];
                //return items[key - (key > 1000 ? 929 : 32)];
            }
            catch
            {
                return _items[0];
            }
        }

        /// <summary>
        /// Проверить, присутствует ли такой символ
        /// </summary>
        public bool IsPresent(char key) => key == ' ' || !Get(key).Equals(_items[0]);

        #region Width

        /// <summary>
        /// Получить ширину символа
        /// </summary>
        private byte GetWidth(BufferedImage bi, int index)
        {
            int x0 = (index & 15) * _horiAdvance;
            int y0 = (index >> 4) * _horiAdvance;
            int x1 = x0 + _horiAdvance - 1;
            int y1 = y0 + _horiAdvance;

            for (int x = x1; x >= x0; x--)
            {
                for (int y = y0; y < y1; y++)
                {
                    if (bi.GetPixelAlpha(x, y) > 0)
                    {
                        return (byte)(x - x0 + 1);
                    }
                }
            }
            return 4;
        }

        /// <summary>
        /// Узнать ширину символа
        /// </summary>
        public int WidthChar(char letter) => Get(letter).Width;

        /// <summary>
        /// Получить шаг между символами
        /// </summary>
        private int GetStepFont() => _style.IsBolb() ? _stepFont + 1 : _stepFont;

        /// <summary>
        /// Узнать ширину текста без размера интерфейса
        /// </summary>
        public int WidthString(string text)
        {
            char[] vc = text.ToCharArray();
            int w = 0;
            int w0;
            Symbol symbol;
            int count = vc.Length;
            int check = count - 1;
            for (int i = 0; i < count; i++)
            {
                symbol = Get(vc[i]);
                if (symbol.IsAmpersand() && i < check)
                {
                    // Этап определения стиля шрифта
                    symbol = Get(vc[++i]);
                    _style.SetSymbol(symbol);
                }
                else
                {
                    w0 = symbol.Width;
                    if (w0 > 0)
                    {
                        w += w0 + GetStepFont();
                    }
                }
            }
            return w;
        }

        /// <summary>
        /// Обрезка строки и ставится ... если не влазит в ширину
        /// </summary>
        /// <param name="ellipsis">Надо ставить многоточие, false не ставим, но ширину учитываем</param>
        public string TransferString(string text, int width, bool ellipsis = true)
        {
            int width3dot = WidthString(Ce.Ellipsis);
            Transfer.Run(text, width - width3dot, _si);
            string[] strs = Transfer.OutText.Split(TransferText.StringSeparators, StringSplitOptions.None);
            if (strs.Length > 0) text = strs[0];
            if (ellipsis && strs.Length > 1) text += Ce.Ellipsis;
            return text;
        }

        /// <summary>
        /// Перенести текст согласно ширине контрола
        /// </summary>
        public string TransferWidth(string text, int width)
        {
            Transfer.Run(text, width, _si);
            return Transfer.OutText;
        }

        /// <summary>
        /// Получить строку с конца по заданной ширине
        /// </summary>
        public string GetStringEndToWidth(string text, int width)
        {
            char[] vc = text.ToCharArray();
            int i, w0;
            int w = 0;
            int count = vc.Length;
            count--;
            for (i = count; i >= 0; i--)
            {
                w0 = WidthChar(vc[i]);
                if (w0 > 0)
                {
                    w += w0 + _stepFont;
                    if (w > width)
                    {
                        return text.Substring(i + 1);
                    }
                }
            }
            return text;
        }

        #endregion

        #region FX

        /// <summary>
        /// Дополнение к эффектам, делается в конце по итогу заполнения буфера
        /// </summary>
        public void RenderFX()
        {
            if (_fontFX == EnumFontFX.Outline)
            {
                BufferOutline();
            }
            else if (_fontFX == EnumFontFX.Shadow)
            {
                BufferShadow();
            }
        }

        /// <summary>
        /// Корректируем буфер с тенью
        /// </summary>
        private void BufferShadow()
        {
            // Текст готов, пробую сделать фон на основании текущего буффера
            int count = _buffer.Count;
            // Делаем копию
            _buffer.AddCopy(0, count);
            // Делаем смещение в сторону, и центральный последний меняем цвет
            for (int i = 0; i < count; i += 8)
            {
                _buffer[i] = _buffer[i] + _si;
                _buffer[i + 1] = _buffer[i + 1] + _si;
                _buffer[i + 4] = _buffer[i + count + 4] / 4f;
                _buffer[i + 5] = _buffer[i + count + 5] / 4f;
                _buffer[i + 6] = _buffer[i + count + 6] / 4f;
            }
        }
        /// <summary>
        /// Корректируем буфер с контуром
        /// </summary>
        private void BufferOutline()
        {
            // Текст готов, пробую сделать фон на основании текущего буффера
            int count = _buffer.Count;
            int count2 = count * 2;
            int count3 = count * 3;
            int count4 = count * 4;

            // Делаем финишную копию
            _buffer.AddCopy(0, count, count4);
            // Красим первый контур в затемнёный цвет
            for (int i = 0; i < count; i += 8)
            {
                _buffer[i + 4] = _buffer[i + count4 + 4] / 4f;
                _buffer[i + 5] = _buffer[i + count4 + 5] / 4f;
                _buffer[i + 6] = _buffer[i + count4 + 6] / 4f;
            }
            // Делаем ещё 3 копии контура
            _buffer.AddCopy(0, count, count);
            _buffer.AddCopy(0, count, count2);
            _buffer.AddCopy(0, count, count3);

            // Делаем смещение в 4 стороны
            for (int i = 0; i < count; i += 8)
            {
                _buffer[i + count] = _buffer[i] + _si;
                _buffer[i + count2] = _buffer[i] - _si;
                _buffer[i + count3 + 1] = _buffer[i + 1] + _si;
                _buffer[i + 1] = _buffer[i + 1] - _si;
            }
        }

        #endregion

        /// <summary>
        /// Шаг смещения вертикали с учётом интерфейса
        /// </summary>
        public int GetVertStep() => _vert + 4 * _si;

        /// <summary>
        /// Вертикальное смещение начала следующего глифа с учётом размера интерфейса 
        /// </summary>
        public int GetVert() => _vert;

        #region Render

        /// <summary>
        /// Прорисовка текста
        /// </summary>
        public void RenderText(int x, int y, string text)
        {
            string[] strs = text.Split(TransferText.StringSeparators, StringSplitOptions.None);

            foreach (string str in strs)
            {
                RenderString(x, y, str);
                y += GetVertStep();
            }
        }

        /// <summary>
        /// Прорисовка строки, возращает ширину строки
        /// </summary>
        public int RenderString(int x, int y, string text)
        {
            char[] vc = text.ToCharArray();
            Symbol symbol;
            int x0 = x;
            int count = vc.Length;
            int check = count - 1;
            int y2 = y + _vert;
            for (int i = 0; i < count; i++)
            {
                symbol = Get(vc[i]);
                if (symbol.IsAmpersand() && i < check)
                {
                    // Этап определения стиля шрифта
                    symbol = Get(vc[++i]);
                    _style.SetSymbol(symbol);
                }
                else
                {
                    GenBuffer(symbol, x, y, x + _hori, y2, _colorText.X, _colorText.Y, _colorText.Z);
                    if (symbol.Width > 0) x += (symbol.Width + GetStepFont()) * _si;
                }
            }
            return x - x0;
        }

        #endregion

        #region Rectangle

        /// <summary>
        /// Сгенерировать буфер символа с учётом стиля
        /// </summary>
        private void GenBuffer(Symbol symbol, int x1, int y1, int x2, int y2, float colorR, float colorG, float colorB)
        {
            float v1 = symbol.V1;
            float u1 = symbol.U1;
            float v2 = symbol.V2;
            float u2 = symbol.U2;

            float r, g, b;
            if (_style.IsColor())
            {
                int index = _style.GetColor();
                r = Gi.ColorReg[index];
                g = Gi.ColorGreen[index];
                b = Gi.ColorBlue[index];
            }
            else
            {
                r = colorR;
                g = colorG;
                b = colorB;
            }
            
            _buffer.AddRange(Rectangle(x1, y1, x2, y2, v1, u1, v2, u2, r, g, b));
            if (_style.IsBolb())
            {
                _buffer.AddRange(Rectangle(x1 + _si, y1, x2 + _si, y2, v1, u1, v2, u2, r, g, b));
            }
            if (_style.IsUnderline())
            {
                _buffer.AddRange(Rectangle(x1 - _si, y2 - _si, x1 + (symbol.Width + 1) * _si, y2, 0, 0, .0625f, .0625f, r, g, b));
            }
            if (_style.IsStrikethrough())
            {
                y2 -= (y2 - y1) / 2;
                _buffer.AddRange(Rectangle(x1 - _si, y2 - _si, x1 + (symbol.Width + 1) * _si, y2, 0, 0, .0625f, .0625f, r, g, b));
            }
        }

        /// <summary>
        /// Нарисовать прямоугольник в 2д, с цветом [2, 2, 3]
        /// </summary>
        private float[] Rectangle(int x1, int y1, int x2, int y2, float v1, float u1, float v2, float u2,
            float r, float g, float b, float a = 1f)
        {
            if (_style.IsItalic())
            {
                return new float[]
                {
                    x1 + 1.8f, y1, v1, u1, r, g, b, a,
                    x1 - 1.8f, y2, v1, u2, r, g, b, a,
                    x2 - 1.8f, y2, v2, u2, r, g, b, a,
                    x2 + 1.8f, y1, v2, u1, r, g, b, a
                };
            }
            return new float[]
            {
                x1, y1, v1, u1, r, g, b, a,
                x1, y2, v1, u2, r, g, b, a,
                x2, y2, v2, u2, r, g, b, a,
                x2, y1, v2, u1, r, g, b, a
            };
        }

        #endregion

        #region Reload

        /// <summary>
        /// Перезалить буфер и прорисовыать
        /// </summary>
        public void ReloadDraw()
        {
            if (_mesh != null)
            {
                _mesh.Reload(_buffer.GetBufferAll(), _buffer.Count);
                _mesh.Draw();
            }
        }

        /// <summary>
        /// Перезалить буфер
        /// </summary>
        public void Reload()
        {
            if (_mesh != null)
            {
                _mesh.Reload(_buffer.GetBufferAll(), _buffer.Count);
            }
        }

        /// <summary>
        /// Перезалить в сторонюю сетку
        /// </summary>
        public void Reload(Mesh mesh)
        {
            if (mesh != null)
            {
                mesh.Reload(_buffer.GetBufferAll(), _buffer.Count);
            }
        }

        #endregion

        #region Buffer

        /// <summary>
        /// Очистить буфер сетки и прочие настройки
        /// </summary>
        public void Clear(bool isColorDefault = true)
        {
            _buffer.Clear();
            _style.Reset();
            if (isColorDefault)
            {
                _colorText = Gi.ColorText;
            }
        }

        /// <summary>
        /// Сколько полигон
        /// </summary>
        public int ToPoligons() => _buffer.Count / 14;

        #endregion

        #region Style

        /// <summary>
        /// Сбросить стиль
        /// </summary>
        public void StyleReset() => _style.Reset();
        /// <summary>
        /// Задать цвет по умолчпнию, если не будет выбран стилем
        /// </summary>
        public FontBase SetColor(Vector3 colorText)
        {
            this._colorText = colorText;
            return this;
        }
        /// <summary>
        /// Указать эффект к шрифту
        /// </summary>
        public FontBase SetFontFX(EnumFontFX fontFX)
        {
            this._fontFX = fontFX;
            return this;
        }

        #endregion
    }
}
