namespace Vge.Util
{
    /// <summary>
    /// Объект всякого разного
    /// </summary>
    public sealed class Sundry
    {
        /// <summary>
        /// Рекомендуемое количество для партии
        /// </summary>
        /// <param name="time">Время в мс затраченное</param>
        /// <param name="quantity">Количество сделанного</param>
        /// <param name="previous">Предыдущее количество</param>
        public static byte RecommendedQuantityBatch(int time, int quantity, byte previous)
        {
            int dbs = previous;
            if (time == 0)
            {
                if (quantity < Ce.MaxDesiredBatchSize)
                {
                    dbs = quantity * 2;
                    if (dbs > Ce.MaxDesiredBatchSize)
                    {
                        // Максималка!
                        dbs = Ce.MaxDesiredBatchSize;
                    }
                }
                else if (dbs != Ce.MaxDesiredBatchSize)
                {
                    dbs = Ce.MaxDesiredBatchSize;
                }
            }
            else
            {
                dbs = Ce.MaxBatchChunksTime * quantity / time;
                if (dbs > Ce.MaxDesiredBatchSize) dbs = Ce.MaxDesiredBatchSize;
                if (dbs < Ce.MinDesiredBatchSize) dbs = Ce.MinDesiredBatchSize;
            }
            if (dbs != previous)
            {
                return (byte)((previous * 7 + dbs) / 8);
            }
            return previous;
        }

    }
}
