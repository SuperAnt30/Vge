using System.Runtime.CompilerServices;
using WinGL.Util;

namespace Vge.Util
{
    /// <summary>
    /// SAT (теорема о разделяющей оси)
    /// https://habr.com/ru/articles/509568/
    /// https://textbooks.cs.ksu.edu/cis580/04-collisions/04-separating-axis-theorem/
    /// </summary>
    public class Sat3d
    {
        /// <summary>
        /// Проверить есть ли столкновение, вернёт true если есть
        /// Кубы массив, следующий где Y вверх
        ///   5 +-----+ 6
        ///    /:    /|
        /// 4 +-1---+7+ 2
        ///   |     |/
        /// 0 +-----+ 3
        /// </summary>
        public static bool Collides(Vector3[] cube1, Vector3[] cube2)
        {
            Vector3[] axes = new Vector3[]
            {
                Glm.Normalize(cube1[0] - cube1[1]),
                Glm.Normalize(cube1[1] - cube1[2]),
                Glm.Normalize(cube1[0] - cube1[4]),
                Glm.Normalize(cube2[0] - cube2[1]),
                Glm.Normalize(cube2[1] - cube2[2]),
                Glm.Normalize(cube2[0] - cube2[4])
            };

            for (int j = 0; j < axes.Length; j++)
            {
                // проекции куба 1
                _ProjAxis(out float min1, out float max1, cube1, axes[j]);

                // проекции куба 2
                _ProjAxis(out float min2, out float max2, cube2, axes[j]);

                if (max1 < min2 || max2 < min1)
                {
                    // объекты не пересекаются
                    return false;
                }
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void _ProjAxis(out float min, out float max, Vector3[] points, Vector3 axis)
        {
            min = max = Glm.Dot(points[0], axis);
            float tmp;
            for (int i = 1; i < points.Length; i++)
            {
                tmp = Glm.Dot(points[i], axis);
                if (tmp > max) max = tmp;
                if (tmp < min) min = tmp;
            }
        }
    }
}
