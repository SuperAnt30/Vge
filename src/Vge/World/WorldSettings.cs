namespace Vge.World
{
    /// <summary>
    /// Опции мира
    /// </summary>
    public class WorldSettings
    {
        /// <summary>
        /// Не имеет неба, true
        /// </summary>
        public bool HasNoSky { get; protected set; } = false;
        /// <summary>
        /// Активный радиус обзора для сервера, нужен для спавна и тиков блоков
        /// </summary>
        public byte ActiveRadius { get; protected set; } = 8;
        /// <summary>
        /// Количество секций в чанке. Максимально 32
        /// </summary>
        public byte NumberChunkSections { get; protected set; } = 8;

    }
}
