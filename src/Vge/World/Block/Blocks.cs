namespace Vge.World.Block
{
    /// <summary>
    /// Различные массивы блоков
    /// </summary>
    public sealed class Blocks
    {
        /*
        BlocksLightOpacity[0] = LightOpacity << 4 | LightValue
        LightOpacity = BlocksLightOpacity[0] >> 4
        LightValue = BlocksLightOpacity[0] & 0xF
        */

        /// <summary>
        /// Массив прозрачности и излучаемости освещения, для ускорения алгоритмов освещения
        /// </summary>
        public static byte[] BlocksLightOpacity;
        /// <summary>
        /// Массив всех кэш блоков
        /// </summary>
        public static BlockBase[] BlocksInt;
        /// <summary>
        /// Массив нужности случайного тика для блока
        /// </summary>
        public static bool[] BlocksRandomTick;
        /// <summary>
        /// Массив с дополнительными metdata свыше 4 бита
        /// </summary>
        public static bool[] BlocksAddMet;
    }
}
