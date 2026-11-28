using System.Runtime.CompilerServices;

namespace Vge.World.Block
{
    /// <summary>
    /// Раскладка Met у блока с доп. жидкостью
    /// </summary>
    public static class LiquidMet
    {
        /// <summary>
        /// Имеется ли дополнительная жидкость в этом блоке
        /// NLLL mmmm MMMM MMMM MMMM //BBBB BBBB BBBB
        /// N - не используется
        /// L - id блока жидкости через доп массив индексов
        /// m - met данные жидкости
        /// M - met данные блока
        /// B - id блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAddLiquid(int met) => met >> 16 != 0;

        /// <summary>
        /// Получить Index дополнительный блока жидкости
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetAddLiquidIndex(int met) => Ce.Blocks.GetAddLiquidIndex(met);

        /// <summary>
        /// Получить дополнительный блок жидкости
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockBase GetAddLiquid(int met) => Ce.Blocks.GetAddLiquid(met);

        /// <summary>
        /// Получить мет данные дополнительной жидкости
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetAddLiquidMet(int met) => (met >> 12) & 0xF;

        /// <summary>
        /// Сгенерировать параметры блока, внося данные доп жидкости
        /// </summary>
        /// <param name="met"></param>
        //public int GenMetdataAddLiquid(int met)
        //{

        //}
    }
}
