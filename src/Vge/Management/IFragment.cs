namespace Vge.Management
{
    /// <summary>
    /// Интерфейс фрагмента, в каком стиле и алгоритме загружать чанки вокруг якорей
    /// </summary>
    public interface IFragment
    {
        void OverviewChunkAddServerCircle(IAnchor anchor,
            int chx, int chz, int radius,
            int chx2, int chz2, int radius2);
    }
}
