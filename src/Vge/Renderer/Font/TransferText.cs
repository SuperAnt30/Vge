using System;
using Vge.Realms;

namespace Vge.Renderer.Font
{
    /// <summary>
    /// Класс разбития длинный строки на переносы
    /// </summary>
    public class TransferText
    {
        /// <summary>
        /// Объект шрифта
        /// </summary>
        private readonly FontBase font;
        /// <summary>
        /// Размер интерфеса
        /// </summary>
        private int si;
        /// <summary>
        /// Задаваемая ширина
        /// </summary>
        private int inWidth;
        /// <summary>
        /// Задаваемый текст
        /// </summary>
        private string inText;

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

        public TransferText(FontBase font) => this.font = font;

        public void Run(string text, int width, int si)
        {
            inText = text;
            this.si = si;
            inWidth = width * si;
            Run();
        }

        private void Run()
        {
            OutWidth = 0;
            NumberLines = 1;
            string[] strs = inText.Split(new string[] { Ce.Br, ChatStyle.Br }, StringSplitOptions.None);

            font.Clear();
            // Ширина пробела
            int wspase = font.WidthString(" ") * si;
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
                    int ws = font.WidthString(symbol) * si;
                    if (w + wspase + ws > inWidth)
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
            OutWidth /= si;
        }
    }
}
