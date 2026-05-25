using System.Runtime.CompilerServices;
using WinGL.Util;

namespace Vge.Entity.AI.PathFinding
{
    /// <summary>
    /// Точки пути
    /// </summary>
    public class PathPoint
    {
        public readonly int CoordX;
        public readonly int CoordY;
        public readonly int CoordZ;

        /// <summary>
        /// Индекс этой точки в назначенном ей пути
        /// </summary>
        public int Index = -1;
        /// <summary>
        /// Расстояние до цели
        /// </summary>
        public float DistanceToTarget;
        /// <summary>
        /// Расстояние по пути до этой точки
        /// </summary>
        public float TotalPathDistance;
        /// <summary>
        /// Линейное расстояние до следующей точки
        /// </summary>
        public float DistanceToNext;
        /// <summary>
        /// Предшествующая этому точка на заданном пути
        /// </summary>
        public PathPoint Previous;
        /// <summary>
        /// Истинно, если навигатор уже посетил эту точку
        /// </summary>
        public bool Visited;

        /// <summary>
        /// Хэш координат, используемый для идентификации этой точки
        /// </summary>
        private readonly int _hash;

        public PathPoint(int x, int y, int z)
        {
            CoordX = x;
            CoordY = y;
            CoordZ = z;
            _hash = MakeHash(x, y, z);
        }

        /// <summary>
        /// Возвращает линейное расстояние до другой точки пути
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float DistanceTo(PathPoint point) => Mth.Sqrt(DistanceToSquared(point));

        /// <summary>
        /// Возвращает линейное расстояние до другой точки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float DistanceTo(float x, float y, float z)
        {
            float vx = x - CoordX - .5f;
            float vy = y - CoordY - .5f;
            float vz = z - CoordZ - .5f;
            return Mth.Sqrt(vx * vx + vy * vy + vz * vz);
        }

        /// <summary>
        /// Возвращает квадрат расстояния до другой точки пути
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float DistanceToSquared(PathPoint point)
        {
            float x = point.CoordX - CoordX;
            float y = point.CoordY - CoordY;
            float z = point.CoordZ - CoordZ;
            return x * x + y * y + z * z;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(PathPoint))
            {
                var point = (PathPoint)obj;
                if (_hash == point._hash && CoordX == point.CoordX 
                    && CoordY == point.CoordY && CoordZ == point.CoordZ)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Возвращает true, если эта точка уже была назначена пути
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAssigned() => Index >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => _hash;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MakeHash(int x, int y, int z)
            => y & 255 | (x & 32767) << 8 | (z & 32767) << 24 | (x < 0 ? int.MinValue : 0) | (z < 0 ? 32768 : 0);

        public override string ToString() => CoordX + "," + CoordY + "," + CoordZ + " = " + DistanceToTarget;
    }
}
