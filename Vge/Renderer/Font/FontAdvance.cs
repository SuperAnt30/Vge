using WinGL.Util;

namespace Vge.Renderer.Font
{
    /// <summary>
    /// Объект хранит все глифы
    /// </summary>
    public class FontAdvance
    {
        public static string Key = " !\"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~ АБВГДЕЖЗИКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдежзиклмнопрстуфхцчшщъыьэюяЁёЙй";
        /// <summary>
        /// Получить массив символов
        /// </summary>
        public static char[] ToArrayKey() => Key.ToCharArray();

        /// <summary>
        /// Массив символов
        /// </summary>
        private static readonly Symbol[,] items = new Symbol[3, 177];

        /// <summary>
        /// Горизонтальное смещение начала следующего глифа
        /// </summary>
        public static int[] HoriAdvance { get; private set; } = new int[] { 8, 12, 16 };
        /// <summary>
        /// Вертикальное смещение начала следующего глифа 
        /// </summary>
        public static int[] VertAdvance { get; private set; } = new int[] { 8, 12, 16 };

        /// <summary>
        /// Инициализировать шрифты
        /// </summary>
        public static void Initialize(BufferedImage textureFont8, BufferedImage textureFont12, BufferedImage textureFont16)
        {
            InitializeFontX(textureFont8, 0);
            InitializeFontX(textureFont12, 1);
            InitializeFontX(textureFont16, 2);
        }

        private static void InitializeFontX(BufferedImage textureFont, byte size)
        {
            HoriAdvance[size] = textureFont.width >> 4;
            VertAdvance[size] = textureFont.height >> 4;

            char[] vc = ToArrayKey();
            int key;
            for (int i = 0; i < vc.Length; i++)
            {
                Symbol symbol = new Symbol(vc[i], size, textureFont);
                key = vc[i];
                items[size, key - (key > 1000 ? 929 : 32)] = symbol;
            }

            items[size, 0] = new Symbol(' ', size, textureFont);
        }

        /// <summary>
        /// Получить объект символа
        /// </summary>
        public static Symbol Get(char key, int size)
        {
            try
            {
                return items[size, key - (key > 1000 ? 929 : 32)];
            }
            catch
            {
                return items[size, 0];
            }
        }
    }
}
