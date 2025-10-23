using System;
using Vge.Realms;

namespace Vge.Renderer.Font
{
    /// <summary>
    /// Класс разбития длинный строки на переносы
    /// </summary>
    public class TransferText
    {
        public static string[] StringSeparators = new string[] { Ce.Br, ChatStyle.Br };

        /// <summary>
        /// Объект шрифта
        /// </summary>
        private readonly FontBase _font;
        /// <summary>
        /// Размер интерфеса
        /// </summary>
        private int _si;
        /// <summary>
        /// Задаваемая ширина
        /// </summary>
        private int _inWidth;
        /// <summary>
        /// Задаваемый текст
        /// </summary>
        private string _inText;

        /// <summary>
        /// Результат максимальной ширины строки
        /// </summary>
        public int OutWidth { get; private set; }
        /// <summary>
        /// Результат текста
        /// </summary>
        public string OutText { get; private set; }
        /// <summary>
        /// Количество строк
        /// </summary>
        public int NumberLines { get; private set; }

        public TransferText(FontBase font) => _font = font;

        public void Run(string text, int width, int si)
        {
            _inText = text;
            _si = si;
            _inWidth = width * si;
            Run();
        }

        private void Run()
        {
            OutWidth = 0;
            NumberLines = 1;
            string[] strs = _inText.Split(StringSeparators, StringSplitOptions.None);

            _font.Clear(false);
            // Ширина пробела
            int wspase = _font.WidthString(" ") * _si;
            int w;
            string text = "";
            string[] symbols;
            bool first = true;
            foreach (string str in strs)
            {
                symbols = str.Split(' ');
                w = 0;
                if (!first) text += Ce.Br; else first = false;

                foreach (string symbol in symbols)
                {
                    int ws = _font.WidthString(symbol) * _si;
                    if (w + ws > _inWidth)
                    {
                        if (w > OutWidth) OutWidth = w;
                        NumberLines++;
                        w = ws;
                        text += Ce.Br + symbol;
                    }
                    else
                    {
                        if (w > 0) text += " ";
                        text += symbol;
                        w += wspase + ws;
                    }
                }
                if (w > OutWidth) OutWidth = w;
            }
            OutText = text;
            OutWidth /= _si;
        }

        /// <summary>
        /// Сгенерировать строки по OutText
        /// </summary>
        public string[] GetStrings()
            => OutText.Split(StringSeparators, StringSplitOptions.None);

        /// <summary>
        /// Сгенерировать количество строк по OutText
        /// </summary>
        public int GetStringsCount() => GetStrings().Length;
    }
}
