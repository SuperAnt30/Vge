namespace Vge.World
{
    /// <summary>
    /// Структура спрайта для генерации атласа блоков и предметов
    /// </summary>
    public struct SpriteData
    {
        /// <summary>
        /// Индекс расположение спрайта
        /// </summary>
        public int Index;
        /// <summary>
        /// Занимаемое количество спрайтов в ширину
        /// </summary>
        public int CountWidth;
        /// <summary>
        /// Занимаемое количество спрайтов в высоту
        /// </summary>
        public int CountHeight;

        public SpriteData(int index, int countWidth, int countHeight)
        {
            Index = index;
            CountWidth = countWidth;
            CountHeight = countHeight;
        }
        public SpriteData(int index)
        {
            Index = index;
            CountWidth = 1;
            CountHeight = 1;
        }
    }
}
