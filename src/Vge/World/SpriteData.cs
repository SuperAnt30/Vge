using System.Runtime.CompilerServices;

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

        private readonly bool _animation;

        public SpriteData(int index, int countWidth, int countHeight, bool animation)
        {
            Index = index;
            CountWidth = countWidth;
            CountHeight = countHeight;
            _animation = animation;
        }
        public SpriteData(int index)
        {
            Index = index;
            CountWidth = 1;
            CountHeight = 1;
            _animation = false;
        }

        /// <summary>
        /// Имеется ли анимация
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAnimation() => _animation;
            //CountHeight > 1 && CountHeight != CountWidth;

        public override string ToString()
            => Index + " [" + CountWidth + ":" + CountHeight + "]" + (_animation ? " A" : "");
    }
}
