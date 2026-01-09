namespace Vge.World.Block
{
    /// <summary>
    /// Структура мгновенно тикающего блока
    /// </summary>
    public struct BlockTick
    {
        /// <summary>
        /// Локальная позиция блока в чанке X
        /// </summary>
        public byte X;
        /// <summary>
        /// Позиция блока в чанке Y
        /// </summary>
        public ushort Y;
        /// <summary>
        /// Локальная позици в чанке Z
        /// </summary>
        public byte Z;
        /// <summary>
        /// Отдельный тик для дополнительного блока жидкости
        /// </summary>
        public bool Liquid;
        /// <summary>
        /// Игровое время когда этот блок должен сработать
        /// </summary>
        public uint ScheduledTick;
        /// <summary>
        /// Приоритет
        /// </summary>
        public bool Priority;
        /// <summary>
        /// Индекс для оптимизации, используется только в Upload для кеша
        /// </summary>
        public int Index;

        public BlockTick(int x, int y, int z, bool liquid, uint scheduledTick, bool priority)
        {
            X = (byte)x;
            Y = (ushort)y;
            Z = (byte)z;
            Liquid = liquid;
            ScheduledTick = scheduledTick;
            Priority = priority;
            Index = 0;
        }

        public override string ToString()
            => string.Format("{0},{1},{2}{3} = {4}", X, Y, Z, Liquid ? "L" : "", ScheduledTick);

        public void Set(uint scheduledTick, bool priority)
        {
            ScheduledTick = scheduledTick;
            Priority = priority;
        }
    }
}
