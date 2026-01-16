namespace Vge.World.Gen
{
    /// <summary>
    /// Индерфейс подготовительного чанка для генерации
    /// </summary>
    public interface IChunkPrimer
    {
        void SetBlockState(int xz, int y, int id);

        void SetBlockState(int xz, int y, int id, int met);

        void SetBlockIdFlag(int xz, int y, int id, byte flag);

        int GetBlockId(int xz, int y);
    }
}
