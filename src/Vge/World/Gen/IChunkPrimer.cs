namespace Vge.World.Gen
{
    /// <summary>
    /// Индерфейс подготовительного чанка для генерации
    /// </summary>
    public interface IChunkPrimer
    {
        void SetBlockState(int xz, int y, ushort id, uint met = 0);

        void SetBlockIdFlag(int xz, int y, ushort id, byte flag);

        ushort GetBlockId(int xz, int y);
    }
}
