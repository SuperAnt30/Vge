using WinGL.Util;

namespace Vge.Item
{
    /// <summary>
    /// Структура возможности установки предмета, и его AABB сетка
    /// </summary>
    public struct YouCanPlaceIt
    {
        /// <summary>
        /// Массив линий
        /// </summary>
        public readonly Vector3[] Lines;
        /// <summary>
        /// Наличие не пустой структуры
        /// </summary>
        public readonly bool Body;
        /// <summary>
        /// Можно установить
        /// </summary>
        public readonly bool CanPlace;

        public YouCanPlaceIt(bool canPlace, Vector3[] lines)
        {
            Lines = lines;
            CanPlace = canPlace;
            Body = true;
        }
    }
}
