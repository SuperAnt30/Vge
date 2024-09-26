using System;
using System.Numerics;
using Vge.Gui.Controls;
using Vge.Realms;
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
        private readonly Symbol[] items = new Symbol[162];
        /// <summary>
        /// Горизонтальное смещение начала следующего глифа
        /// </summary>
        private readonly int horiAdvance;
        /// <summary>
        /// Вертикальное смещение начала следующего глифа 
        /// </summary>
        private readonly int vertAdvance;
        /// <summary>
        /// Растояние между буквами в пикселях
        /// </summary>
        private readonly byte stepFont;
        /// <summary>
        /// Буфер сетки данного шрифта
        /// </summary>
        private readonly ListFlout buffer = new ListFlout();
        /// <summary>
        /// Сетка шрифта
        /// </summary>
        private Mesh mesh;
        /// <summary>
        /// Горизонтальное смещение начала следующего глифа с учётом размера интерфейса
        /// </summary>
        private int hori;
        /// <summary>
        /// Вертикальное смещение начала следующего глифа с учётом размера интерфейса 
        /// </summary>
        private int vert;
        /// <summary>
        /// Размера интерфейса
        /// </summary>
        private int si;
        /// <summary>
        /// Стаил шрифта
        /// </summary>
        private readonly FontStyle style = new FontStyle();
        /// <summary>
        /// Цвет по умолчанию
        /// </summary>
        private Vector3 colorText = Gi.ColorText;
        /// <summary>
        /// Эффекты к шрифту
        /// </summary>
        private EnumFontFX fontFX = EnumFontFX.None;
        /// <summary>
        /// Объект рендера
        /// </summary>
        private readonly RenderMain render;
        /// <summary>
        /// Индест текстурки
        /// </summary>
        private readonly int texture;

        /// <summary>
        /// Класс шрифта
        /// </summary>
        /// <param name="textureFont">Объект изображения</param>
        /// <param name="stepFont">растояние между буквами в пикселях</param>
        /// <param name="render">объект рендера, только для основного потока</param>
        /// <param name="texture">индест текстурки</param>
        public FontBase(BufferedImage textureFont, byte stepFont, RenderMain render, int texture)
        {
            this.render = render;
            this.texture = texture;
            horiAdvance = textureFont.width >> 4;
            vertAdvance = textureFont.height >> 4;
            this.stepFont = stepFont;
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
                items[Convert(key)] = symbol;

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
        public void CreateMesh(GL gl) => mesh = new MeshGuiColor(gl);

        /// <summary>
        /// Обновить размер инерфейса
        /// </summary>
        public void UpdateSizeInterface()
        {
            si = Gi.Si;
            hori = horiAdvance * si;
            vert = vertAdvance * si;
        }

        /// <summary>
        /// Забиндить текстуру шрифта
        /// </summary>
        public void BindTexture() => render.BindTexture(texture);

        /// <summary>
        /// Получить объект символа
        /// </summary>
        public Symbol Get(char key)
        {
            try
            {
                return items[Convert(key)];
                //if (key < 32) return items[0];
                //return items[key - (key > 1000 ? 929 : 32)];
            }
            catch
            {
                return items[0];
            }
        }

        /// <summary>
        /// Проверить, присутствует ли такой символ
        /// </summary>
        public bool IsPresent(char key) => !Get(key).Equals(items[0]);

        #region Width

        /// <summary>
        /// Получить ширину символа
        /// </summary>
        private byte GetWidth(BufferedImage bi, int index)
        {
            int x0 = (index & 15) * horiAdvance;
            int y0 = (index >> 4) * horiAdvance;
            int x1 = x0 + horiAdvance - 1;
            int y1 = y0 + horiAdvance;

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
        private int GetStepFont() => style.IsBolb() ? stepFont + 1 : stepFont;

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
                    style.SetSymbol(symbol);
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
            Transfer.Run(text, width - width3dot, si);
            string[] strs = Transfer.OutText.Split(new string[] { Ce.Br, ChatStyle.Br }, StringSplitOptions.None);
            if (strs.Length > 0) text = strs[0];
            if (ellipsis && strs.Length > 1) text += Ce.Ellipsis;
            return text;
        }

        /// <summary>
        /// Перенести текст согласно ширине контрола
        /// </summary>
        public string TransferWidth(string text, int width)
        {
            Transfer.Run(text, width, si);
            return Transfer.OutText;
        }

        #endregion

        #region FX

        /// <summary>
        /// Дополнение к эффектам, делается в конце по итогу заполнения буфера
        /// </summary>
        public void RenderFX()
        {
            if (fontFX == EnumFontFX.Outline)
            {
                BufferOutline();
            }
            else if (fontFX == EnumFontFX.Shadow)
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
            int count = buffer.Count;
            // Делаем копию
            buffer.AddCopy(0, count);
            // Делаем смещение в сторону, и центральный последний меняем цвет
            for (int i = 0; i < count; i += 7)
            {
                buffer[i] = buffer[i] + si;
                buffer[i + 1] = buffer[i + 1] + si;
                buffer[i + 4] = buffer[i + count + 4] / 4f;
                buffer[i + 5] = buffer[i + count + 5] / 4f;
                buffer[i + 6] = buffer[i + count + 6] / 4f;
            }
        }
        /// <summary>
        /// Корректируем буфер с котуром
        /// </summary>
        private void BufferOutline()
        {
            // Текст готов, пробую сделать фон на основании текущего буффера
            int count = buffer.Count;
            int count2 = count * 2;
            int count3 = count * 3;
            int count4 = count * 4;

            // Делаем финишную копию
            buffer.AddCopy(0, count, count4);
            // Красим первый контур в затемнёный цвет
            for (int i = 0; i < count; i += 7)
            {
                buffer[i + 4] = buffer[i + count4 + 4] / 4f;
                buffer[i + 5] = buffer[i + count4 + 5] / 4f;
                buffer[i + 6] = buffer[i + count4 + 6] / 4f;
            }
            // Делаем ещё 3 копии контура
            buffer.AddCopy(0, count, count);
            buffer.AddCopy(0, count, count2);
            buffer.AddCopy(0, count, count3);

            // Делаем смещение в 4 стороны
            for (int i = 0; i < count; i += 7)
            {
                buffer[i + count] = buffer[i] + si;
                buffer[i + count2] = buffer[i] - si;
                buffer[i + count3 + 1] = buffer[i + 1] + si;
                buffer[i + 1] = buffer[i + 1] - si;
            }
        }

        #endregion

        /// <summary>
        /// Шаг смещения вертикали с учётом интерфейса
        /// </summary>
        public int GetVertStep() => vert + 4 * si;

        /// <summary>
        /// Вертикальное смещение начала следующего глифа с учётом размера интерфейса 
        /// </summary>
        public int GetVert() => vert;

        #region Render

        /// <summary>
        /// Прорисовка текста
        /// </summary>
        public void RenderText(int x, int y, string text)
        {
            string[] stringSeparators = new string[] { Ce.Br, ChatStyle.Br };
            string[] strs = text.Split(stringSeparators, StringSplitOptions.None);

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
            int y2 = y + vert;
            for (int i = 0; i < count; i++)
            {
                symbol = Get(vc[i]);
                if (symbol.IsAmpersand() && i < check)
                {
                    // Этап определения стиля шрифта
                    symbol = Get(vc[++i]);
                    style.SetSymbol(symbol);
                }
                else
                {
                    GenBuffer(symbol, x, y, x + hori, y2, colorText.X, colorText.Y, colorText.Z);
                    if (symbol.Width > 0) x += (symbol.Width + GetStepFont()) * si;
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
            if (style.IsColor())
            {
                int index = style.GetColor();
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
            
            buffer.AddRange(Rectangle(x1, y1, x2, y2, v1, u1, v2, u2, r, g, b));
            if (style.IsBolb())
            {
                buffer.AddRange(Rectangle(x1 + si, y1, x2 + si, y2, v1, u1, v2, u2, r, g, b));
            }
            if (style.IsUnderline())
            {
                buffer.AddRange(Rectangle(x1 - si, y2 - si, x1 + (symbol.Width + 1) * si, y2, 0, 0, .0625f, .0625f, r, g, b));
            }
            if (style.IsStrikethrough())
            {
                y2 -= (y2 - y1) / 2;
                buffer.AddRange(Rectangle(x1 - si, y2 - si, x1 + (symbol.Width + 1) * si, y2, 0, 0, .0625f, .0625f, r, g, b));
            }
        }

        /// <summary>
        /// Нарисовать прямоугольник в 2д, с цветом [2, 2, 3]
        /// </summary>
        private float[] Rectangle(int x1, int y1, int x2, int y2, float v1, float u1, float v2, float u2,
            float r, float g, float b)
        {
            if (style.IsItalic())
            {
                return new float[]
                {
                    x1 + 1.8f, y1, v1, u1, r, g, b,
                    x1 - 1.8f, y2, v1, u2, r, g, b,
                    x2 + 1.8f, y1, v2, u1, r, g, b,
                    x2 - 1.8f, y2, v2, u2, r, g, b,
                };
            }
            return new float[]
            {
                x1, y1, v1, u1, r, g, b,
                x1, y2, v1, u2, r, g, b,
                x2, y1, v2, u1, r, g, b,
                x2, y2, v2, u2, r, g, b,
            };
        }

        #endregion

        #region Reload

        /// <summary>
        /// Перезалить буфер и прорисовыать
        /// </summary>
        public void ReloadDraw()
        {
            if (mesh != null)
            {
                mesh.Reload(buffer.GetBufferAll(), buffer.Count);
                mesh.Draw();
            }
        }

        /// <summary>
        /// Перезалить в сторонюю сетку
        /// </summary>
        public void Reload(Mesh mesh)
        {
            if (mesh != null)
            {
                mesh.Reload(buffer.GetBufferAll(), buffer.Count);
            }
        }

        #endregion

        #region Buffer

        /// <summary>
        /// Очистить буфер сетки и прочие настройки
        /// </summary>
        public void Clear(bool isColorDefault = true)
        {
            buffer.Clear();
            style.Reset();
            if (isColorDefault)
            {
                colorText = Gi.ColorText;
            }
        }

        /// <summary>
        /// Получить сетку буфера
        /// </summary>
        public float[] ToBuffer() => buffer.ToArray();
        /// <summary>
        /// Сколько полигон
        /// </summary>
        public int ToPoligons() => buffer.Count / 14;

        #endregion

        #region Style

        /// <summary>
        /// Сбросить стиль
        /// </summary>
        public void StyleReset() => style.Reset();
        /// <summary>
        /// Задать цвет по умолчпнию, если не будет выбран стилем
        /// </summary>
        public FontBase SetColor(Vector3 colorText)
        {
            this.colorText = colorText;
            return this;
        }
        /// <summary>
        /// Указать эффект к шрифту
        /// </summary>
        public FontBase SetFontFX(EnumFontFX fontFX)
        {
            this.fontFX = fontFX;
            return this;
        }

        #endregion
    }
}
