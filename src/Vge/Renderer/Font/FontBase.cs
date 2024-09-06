using System;
using System.Numerics;
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
        /// Массив символов
        /// </summary>
        private readonly Symbol[] items = new Symbol[177];
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
        private readonly ListFast<float> buffer = new ListFast<float>();
        /// <summary>
        /// Сетка шрифта
        /// </summary>
        private readonly Mesh mesh;
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
        /// Класс шрифта
        /// </summary>
        /// <param name="textureFont">Объект изображения</param>
        /// <param name="stepFont">растояние между буквами в пикселях</param>
        /// <param name="isMesh">нужен ли меш</param>
        public FontBase(GL gl, BufferedImage textureFont, byte stepFont, bool isMesh = true)
        {
            if (isMesh)
            {
                mesh = new Mesh2d(gl);
            }

            horiAdvance = textureFont.width >> 4;
            vertAdvance = textureFont.height >> 4;
            this.stepFont = stepFont;

            string keys = " !\"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~ АБВГДЕЖЗИКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдежзиклмнопрстуфхцчшщъыьэюяЁёЙй";
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
                    throw new Exception(SR.GetString(SR.TheSymbolIsNotInTheList, vc[i]));
                }
                width = GetWidth(textureFont, index);
                Symbol symbol = new Symbol(symb, index, width);
                key = symb;
                items[key - (key > 1000 ? 929 : 32)] = symbol;
            }
            UpdateSizeInterface();
        }

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
        /// Получить объект символа
        /// </summary>
        public Symbol Get(char key)
        {
            try
            {
                return items[key - (key > 1000 ? 929 : 32)];
            }
            catch
            {
                return items[0];
            }
        }

        /// <summary>
        /// Узнать ширину символа
        /// </summary>
        public int WidthChar(char letter) => Get(letter).Width;

        #region Render

        /// <summary>
        /// Прорисовка текста
        /// </summary>
        public void RenderText(int x, int y, string text, Vector3 color)
        {
            string[] stringSeparators = new string[] { "\r\n" };
            string[] strs = text.Split(stringSeparators, StringSplitOptions.None);

            Symbol symbol;
            char[] vc;
            int x0 = x;
            foreach (string str in strs)
            {
                vc = str.ToCharArray();
                x = x0;
                for (int i = 0; i < vc.Length; i++)
                {
                    symbol = Get(vc[i]);
                    buffer.AddRange(Rectangle2d(x, y, x + hori, y + vert,
                        symbol.U1, symbol.V1, symbol.U2, symbol.V2, color.X, color.Y, color.Z));
                    if (symbol.Width > 0) x += (symbol.Width + stepFont) * si;
                }
                y += vert + 4;
            }
        }

        /// <summary>
        /// Прорисовка строки, возращает ширину строки
        /// </summary>
        public int RenderString(int x, int y, string text, Vector3 color)
        {
            char[] vc = text.ToCharArray();

            Symbol symbol;
            int x0 = x;
            for (int i = 0; i < vc.Length; i++)
            {
                symbol = Get(vc[i]);
                buffer.AddRange(Rectangle2d(x, y, x + hori, y + vert,
                    symbol.U1, symbol.V1, symbol.U2, symbol.V2, color.X, color.Y, color.Z));
                if (symbol.Width > 0) x += (symbol.Width + stepFont) * si;
            }
            return x - x0;
        }

        /// <summary>
        /// Нарисовать прямоугольник в 2д, с цветом [2, 2, 3]
        /// </summary>
        private float[] Rectangle2d(float x1, float y1, float x2, float y2, float v1, float u1, float v2, float u2,
            float r, float g, float b)
        {
            return new float[]
            {
                x1, y1, v1, u1, r, g, b,
                x1, y2, v1, u2, r, g, b,
                x2, y1, v2, u1, r, g, b,

              //  x1, y2, v1, u2, r, g, b,
                x2, y2, v2, u2, r, g, b,
               // x2, y1, v2, u1, r, g, b
            };
        }

        /// <summary>
        /// Перезалить буфер и прорисовыать
        /// </summary>
        public void ReloadDraw()
        {
            if (mesh != null)
            {
                mesh.Reload(buffer.GetBufferAll(), buffer.Count);
                //mesh.Reload(buffer.ToArray());
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

        /// <summary>
        /// Узнать ширину текста без размера интерфейса
        /// </summary>
        public int WidthString(string text)
        {
            char[] vc = text.ToCharArray();
            int w = 0;
            for (int i = 0; i < vc.Length; i++)
            {
                int w0 = WidthChar(vc[i]);
                if (w0 > 0) w += w0 + stepFont;
            }
            return w;
        }

        #region Buffer

        /// <summary>
        /// Очистить буфер сетки
        /// </summary>
        public void BufferClear() => buffer.Clear();
        /// <summary>
        /// Получить сетку буфера
        /// </summary>
        public float[] ToBuffer() => buffer.ToArray();

        #endregion
    }
}
