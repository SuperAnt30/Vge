using System;
using WinGL.Util;

namespace Vge.Util
{
    /// <summary>
    /// Структура для сравнении дистанции
    /// </summary>
    public struct ComparisonDistance : IComparable
    {
        private readonly int _x;
        private readonly int _y;
        private readonly int _z;
        private readonly float _distance;
        private readonly bool _body;

        public ComparisonDistance(int x, int y, float distance)
        {
            _x = x;
            _y = y;
            _z = 0;
            _distance = distance;
            _body = true;
        }
        public ComparisonDistance(int x, int y, int z, float distance)
        {
            _x = x;
            _y = y;
            _z = z;
            _distance = distance;
            _body = true;
        }

        /// <summary>
        /// Дистанция
        /// </summary>
        public float Distance() => _distance;
        /// <summary>
        /// Пустой ли объект
        /// </summary>
        public bool IsEmpty() => !_body;
        /// <summary>
        /// Получить вектор xy и z это дистанция
        /// </summary>
        public Vector3i GetPosition() => new Vector3i(_x, _y, (int)_distance);
        /// <summary>
        /// Получить вектор 3d
        /// </summary>
        public Vector3i GetPosition3d() => new Vector3i(_x, _y, _z);

        /// <summary>
        /// Метод для сортировки
        /// </summary>
        public int CompareTo(object obj)
        {
            if (obj is ComparisonDistance v)
            {
                return _distance.CompareTo(v.Distance());
            }
            else
            {
                throw new Exception(Sr.ItIsImpossibleToCompareTwoObjects);
            }
        }

        public override string ToString()
            => string.Format("({0}; {1}) {2:0.00}", _x, _y, _distance);
    }
}
