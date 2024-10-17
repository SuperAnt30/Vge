﻿using System;
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
        private readonly float _distance;
        private readonly bool _body;

        public ComparisonDistance(int x, int y, float distance)
        {
            _x = x;
            _y = y;
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
        /// Получить вектор
        /// </summary>
        public Vector2i GetPosition() => new Vector2i(_x, _y);

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