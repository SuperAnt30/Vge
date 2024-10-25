using System.Collections.Generic;
using WinGL.Util;

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

        /// <summary>
        /// Сгенерировать кольца для конкретного обзора
        /// </summary>
        public static Vector2i[] GenOverviewCircles(int overview)
        {
            ComparisonDistance comparison;
            List<ComparisonDistance> r = new List<ComparisonDistance>();
            for (int x = -overview; x <= overview; x++)
            {
                for (int y = -overview; y <= overview; y++)
                {
                    comparison = new ComparisonDistance(x, y, Mth.Sqrt(x * x + y * y));
                    if (comparison.Distance() - .3f <= overview)
                    {
                        r.Add(comparison);
                    }
                }
            }
            r.Sort();
            Vector2i[] overviewCircles = new Vector2i[r.Count];
            for (int i = 0; i < r.Count; i++)
            {
                overviewCircles[i] = r[i].GetPosition();
            }
            return overviewCircles;
        }
    }
}
