using System.Runtime.CompilerServices;
using Vge.Entity.Particle;
using Vge.World;
using WinGL.Util;

namespace Vge.Entity.AI.PathFinding
{
    /// <summary>
    /// Путь сущности
    /// </summary>
    public class PathEntity
    {
        /// <summary>
        /// Фактические точки пути
        /// </summary>
        private readonly PathPoint[] _points;

        /// <summary>
        /// Является ли пункт назначения одинаковым
        /// </summary>
        private readonly bool _isDestinationSame;

        /// <summary>
        /// Индекс массива, на который в данный момент нацелен объект
        /// </summary>
        private int _currentPathIndex = 0;

        /// <summary>
        /// Общая длина пути
        /// </summary>
        private int _pathLength;

        public PathEntity(PathPoint[] points, bool isDestinationSame)
        {
            _isDestinationSame = isDestinationSame;
            _points = points;
            _pathLength = points.Length;
        }

        /// <summary>
        /// Направляет этот путь к следующей точке в своем массиве
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementPathIndex() => ++_currentPathIndex;

        /// <summary>
        /// Возвращает true, если этот путь достиг конца
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsFinished() => _currentPathIndex >= _pathLength;

        /// <summary>
        /// Возвращает последний объект массива
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PathPoint GetFinalPathPoint() => _pathLength > 0 ? _points[_pathLength - 1] : null;

        /// <summary>
        /// Вернуть объект по интексу расположения
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PathPoint GetPathPointFromIndex(int index) => _points[index];

        /// <summary>
        /// Вернуть общую длинну
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCurrentPathLength() => _pathLength;

        /// <summary>
        /// Задать общую длинну
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCurrentPathLength(int length) => _pathLength = length;

        /// <summary>
        /// Вернуть индекс текущего элемента
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCurrentPathIndex() => _currentPathIndex;

        /// <summary>
        /// Задать индекс текущего элемента
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCurrentPathIndex(int index) => _currentPathIndex = index;

        /// <summary>
        /// Получает вектор объекта, связанный с данным индексом.
        /// </summary>
        public Vector3 GetVectorFromIndex(EntityBase entity, int index)
        {
            float w = entity.Size.GetWidth();
            float w2 = w * 2f;
            w = (Mth.Ceiling(w2) - w2) / 2f + w;
            return new Vector3(_points[index].CoordX + w, _points[index].CoordY, _points[index].CoordZ + w);

            //return new vec3(points[index].xCoord + .5f, points[index].yCoord, points[index].zCoord + .5f);
        }

        /// <summary>
        /// Возвращает текущий целевой узел объекта как vec3
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetPosition(EntityBase entity) => GetVectorFromIndex(entity, _currentPathIndex);

        /// <summary>
        /// Возвращает true, если EntityPath совпадают. Равенства, не связанные с экземпляром
        /// </summary>
        public bool IsSamePath(PathEntity pathEntity)
        {
            if (pathEntity == null) return false;
            if (pathEntity._points.Length != _points.Length) return false;

            for (int i = 0; i < _points.Length; i++)
            {
                if (_points[i].CoordX != pathEntity._points[i].CoordX 
                    || _points[i].CoordY != pathEntity._points[i].CoordY
                    || _points[i].CoordZ != pathEntity._points[i].CoordZ) return false;
            }
            return true;
        }

        /// <summary>
        /// Является ли пункт назначения одинаковым
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDestinationSame()
        {
            PathPoint pathPoint = GetFinalPathPoint();
            return pathPoint == null ? false : _isDestinationSame;
        }

        /// <summary>
        /// Отладка, визуализация перемещения PathNavigate.SetPath
        /// </summary>
        public void DebugPath(WorldServer world)
        {
            for (int i = 0; i < _points.Length; i++)
            {
                world.SpawnParticle(EntitiesFXReg.CubeId, 1, 
                    new Vector3(_points[i].CoordX + .5f, _points[i].CoordY, _points[i].CoordZ + .5f), 
                    new Vector3(0), 0, 0x4A006);
            }
        }

        /// <summary>
        /// Отладка, последняя точка куда идти PathFinder.CreateEntityPath
        /// </summary>
        public static void DebugEnd(WorldBase world, PathPoint pointEnd)
            => world.SpawnParticle(EntitiesFXReg.CubeId, 1, 
                new Vector3(pointEnd.CoordX + .5f, pointEnd.CoordY, pointEnd.CoordZ + .5f), 
                new Vector3(0), 0, 0x4F06C);
    }
}
