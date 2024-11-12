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
        public static byte RecommendedQuantityBatch(int time, int quantity, byte previous,
            int maxBatch, int maxTime)
        {
            int dbs = previous;
            if (time == 0)
            {
                if (quantity < maxBatch)
                {
                    dbs = quantity * 2;
                    if (dbs > maxBatch)
                    {
                        // Максималка!
                        dbs = maxBatch;
                    }
                }
                else if (dbs != maxBatch)
                {
                    dbs = maxBatch;
                }
            }
            else
            {
                dbs = maxTime * quantity / time;
                if (dbs > maxBatch) dbs = maxBatch;
                if (dbs < Ce.MinDesiredBatchSize) dbs = Ce.MinDesiredBatchSize;
            }
            if (dbs != previous)
            {
                return (byte)((previous * 7 + dbs) / 8);
                //return (byte)((previous * 3 + dbs) / 4);
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

        /// <summary>
        /// Сгенерировать сферу для конкретного обзора без сортировки, столбами по Y
        /// </summary>
        public static Vector3i[] GenOverviewSphere(int overview)
        {
            ComparisonDistance comparison;
            List<ComparisonDistance> r = new List<ComparisonDistance>();
            for (int y = -overview; y <= overview; y++)
            {
                for (int x = -overview; x <= overview; x++)
                {
                    for (int z = -overview; z <= overview; z++)
                    {
                        comparison = new ComparisonDistance(x, y, z, Mth.Sqrt(x * x + y * y));
                        if (comparison.Distance() - .3f <= overview)
                        {
                            r.Add(comparison);
                        }
                    }
                }
            }
            Vector3i[] overviewSphere = new Vector3i[r.Count];
            for (int i = 0; i < r.Count; i++)
            {
                overviewSphere[i] = r[i].GetPosition3d();
            }
            return overviewSphere;
        }

        /// <summary>
        /// Определение вращения
        /// </summary>
        public static Vector2 MotionAngle(float strafe, float forward, float friction, float yaw)
        {
            Vector2 motion = new Vector2(0);
            float sf = strafe * strafe + forward * forward;
            if (sf >= 0.0001f)
            {
                sf = Mth.Sqrt(sf);
                if (sf < 1f) sf = 1f;
                sf = friction / sf;
                strafe *= sf;
                forward *= sf;
                float ysin = Glm.Sin(yaw);
                float ycos = Glm.Cos(yaw);
                motion.X += ycos * strafe - ysin * forward;
                motion.Y += ycos * forward + ysin * strafe;
            }
            return motion;
        }
    }
}
