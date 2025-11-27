namespace Vge.World.Gen
{
    /// <summary>
    /// Индерфейс подготовительного чанка для генерации
    /// </summary>
    public interface IChunkPrimer
    {
        void SetBlockStateFlag(int xz, int y, ushort id, byte flag);
    }
}
