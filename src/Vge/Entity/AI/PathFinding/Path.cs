using System;
using System.Runtime.CompilerServices;

namespace Vge.Entity.AI.PathFinding
{
    public class Path
    {
        /// <summary>
        /// Содержит точки на этом пути
        /// </summary>
        private PathPoint[] _pathPoints = new PathPoint[PathConst.PathMaximumLength]; // 1024

        /// <summary>
        /// Количество точек на этом пути
        /// </summary>
        private int _count;

        /// <summary>
        /// Добавляет точку на путь
        /// </summary>
        public PathPoint AddPoint(PathPoint point)
        {
            if (point.Index >= 0)
            {
                // Индекс тут должен быть = -1
                throw new Exception(Sr.ThereShouldBeAnIndexHere);
            }
            else
            {
                if (_count == _pathPoints.Length)
                {
                    PathPoint[] points = new PathPoint[_count << 1];
                    Array.Copy(_pathPoints, 0, points, 0, _count);
                    _pathPoints = points;
                }

                _pathPoints[_count] = point;
                point.Index = _count;
                _SortBack(_count++);
                return point;
            }
        }

        /// <summary>
        /// Очищает путь
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearPath() => _count = 0;

        /// <summary>
        /// Возвращает и удаляет первую точку пути
        /// </summary>
        public PathPoint Dequeue()
        {
            PathPoint points = _pathPoints[0];
            _pathPoints[0] = _pathPoints[--_count];
            _pathPoints[_count] = null;
            if (_count > 0) _SortForward(0);
            points.Index = -1;
            return points;
        }

        /// <summary>
        /// Изменяет расстояние указанной точки до цели
        /// </summary>
        /// <param name="point">Точка пути</param>
        /// <param name="distance">новое расстояние</param>
        public void ChangeDistance(PathPoint point, float distance)
        {
            float distanceToTarget = point.DistanceToTarget;
            point.DistanceToTarget = distance;
            if (distance < distanceToTarget)
            {
                _SortBack(point.Index);
            }
            else
            {
                _SortForward(point.Index);
            }
        }

        /// <summary>
        /// Сортирует точку с конца
        /// </summary>
        private void _SortBack(int index)
        {
            PathPoint point = _pathPoints[index];
            int indexNew;
            for (float distance = point.DistanceToTarget; index > 0; index = indexNew)
            {
                indexNew = index - 1 >> 1;
                PathPoint point2 = _pathPoints[indexNew];

                if (distance >= point2.DistanceToTarget) break;

                _pathPoints[index] = point2;
                point2.Index = index;
            }
            _pathPoints[index] = point;
            point.Index = index;
        }

        /// <summary>
        /// Сортирует точку с начала
        /// </summary>
        private void _SortForward(int index)
        {
            PathPoint point = _pathPoints[index];
            float distance = point.DistanceToTarget;

            int index2, index3;
            float distance2, distance3;
            PathPoint point2, point3;

            while (true)
            {
                index2 = 1 + (index << 1);
                index3 = index2 + 1;

                if (index2 >= _count) break;

                point2 = _pathPoints[index2];
                distance2 = point2.DistanceToTarget;

                if (index3 >= _count)
                {
                    point3 = null;
                    distance3 = float.PositiveInfinity;
                }
                else
                {
                    point3 = _pathPoints[index3];
                    distance3 = point3.DistanceToTarget;
                }

                if (distance2 < distance3)
                {
                    if (distance2 >= distance) break;
                    _pathPoints[index] = point2;
                    point2.Index = index;
                    index = index2;
                }
                else
                {
                    if (distance3 >= distance) break;
                    _pathPoints[index] = point3;
                    point3.Index = index;
                    index = index3;
                }
            }

            _pathPoints[index] = point;
            point.Index = index;
        }

        /// <summary>
        /// Возвращает true, если этот путь не содержит точек
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPathEmpty() => _count == 0;
    }
}
