namespace Vge.World.Chunk
{
    /// <summary>
    /// Псевдо чанк с данными вокселей
    /// 16 * 16 * 16
    /// y << 8 | z << 4 | x
    /// </summary>
    public class ChunkStorage
    {
        /// <summary>
        /// Уровень псевдочанка, нижнего блока, т.е. кратно 16. Глобальная координата Y, не чанка
        /// </summary>
        public readonly int YBase;
        
        /// <summary>
        /// Данные блока
        /// 12 bit Id блока и 4 bit параметр блока
        /// </summary>
        public ushort[] Data;
        /// <summary>
        /// Освещение блочное, 4 bit используется
        /// </summary>
        public byte[] LightBlock;
        /// <summary>
        /// Освещение небесное, 4 bit используется
        /// </summary>
        public byte[] LightSky;
        /// <summary>
        /// Количество блоков не воздуха
        /// </summary>
        public int CountBlock { get; private set; }

        /// <summary>
        /// Количество блоков которым нужен тик
        /// </summary>
        private int _countTickBlock;

        public ChunkStorage(int y)
        {
            YBase = y;
            Data = null;
            CountBlock = 0;
            _countTickBlock = 0;
            LightBlock = new byte[4096];
            LightSky = new byte[4096];
        }

        /// <summary>
        /// Пустой, все блоки воздуха
        /// </summary>
        public bool IsEmptyData() => CountBlock == 0;

        /// <summary>
        /// Очистить
        /// </summary>
        public void Clear()
        {
            Data = null;
            CountBlock = 0;
            _countTickBlock = 0;
        }

        /// <summary>
        /// Имеются ли блоки которым нужен случайный тик
        /// </summary>
        public bool GetNeedsRandomTick() => _countTickBlock > 0;

        /// <summary>
        /// Вернуть количество блоков не воздуха и количество тикающих блоков
        /// </summary>
        public string ToStringCount() => CountBlock + "|" + _countTickBlock;

        public override string ToString() => "yB:" + YBase + " body:" + CountBlock + " ";
    }
}
