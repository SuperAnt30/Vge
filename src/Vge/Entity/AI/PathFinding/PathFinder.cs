using Vge.World;
using Vge.World.Block;

namespace Vge.Entity.AI.PathFinding
{
    public class PathFinder
    {
        /// <summary>
        /// Объект мира
        /// </summary>
        private readonly WorldServer _world;
        /// <summary>
        /// Процесс узла перемещения
        /// </summary>
        private readonly NodeProcessor _nodeProcessor;
        /// <summary>
        /// Создаваемый путь
        /// </summary>
        private readonly Path _path = new Path();
        /// <summary>
        /// Выбор точек пути для добавления к пути
        /// </summary>
        private readonly PathPoint[] _pathOptions = new PathPoint[8]; // 32

        /// <summary>
        /// Остановка при соприкосновении коллизии
        /// true - соприкосновении коллизии, false - центр
        /// </summary>
        private bool _stopOnOverlap;
        /// <summary>
        /// Приемочный радиус
        /// </summary>
        private float _acceptanceRadius = 0;

        public PathFinder(WorldServer world, NodeProcessor nodeProcessor)
        {
            _world = world;
            _nodeProcessor = nodeProcessor;
        }

        #region Опции

        /// <summary>
        /// Задать опции
        /// </summary>
        /// <param name="stopOnOverlap">Задать остановку при соприкосновении коллизии, true - соприкосновении коллизии, false - центр</param>
        /// <param name="acceptanceRadius">Задать расстояние до точки, которое будет считаться выполненым, 0 - расчёта нет</param>
        public void SetOptions(bool stopOnOverlap, float acceptanceRadius)
        {
            _stopOnOverlap = stopOnOverlap;
            _acceptanceRadius = acceptanceRadius;
        }
        /// <summary>
        /// Остановка при соприкосновении коллизии
        /// true - соприкосновении коллизии, false - центр
        /// </summary>
        public bool GetStopOnOverlap() => _stopOnOverlap;
        /// <summary>
        /// Получить расстояние до точки, которое будет считаться выполненым,
        /// 0 - расчёта нет
        /// </summary>
        public float GetAcceptanceRadius() => _acceptanceRadius;

        #endregion

        /// <summary>
        /// Создает путь от одного объекта к другому на минимальном расстоянии
        /// </summary>
        public PathEntity CreateEntityPathTo(WorldServer world, EntityMob entity,
            EntityLiving entityGoal, float distance)
        {
            float y = entityGoal.PosY;
            // TODO:: 2026-05-25 AI Path
            //if (entity.IsFlying && _nodeProcessor is SwimOrFlyNodeProcessor)
            //{
            //    y += entityGoal.GetEyeHeight();
            //}
            return CreateEntityPathTo(world, entity, entityGoal.PosX, y, entityGoal.PosZ, entityGoal.Size.GetWidth(), distance);
        }

        /// <summary>
        /// Создает путь от объекта к указанному местоположению на минимальном расстоянии
        /// </summary>
        public PathEntity CreateEntityPathTo(WorldServer world, EntityMob entity,
            BlockPos blockPos, float distance) => CreateEntityPathTo(world, entity,
                blockPos.X + .5f, blockPos.Y + .5f, blockPos.Z + .5f, 0, distance);

        /// <summary>
        /// Создает путь от объекта к указанному местоположению на минимальном расстоянии
        /// </summary>
        /// <param name="entityGoalWidth">Ширина сущности к которой идём</param>
        private PathEntity CreateEntityPathTo(WorldServer world, EntityMob entity,
            float x, float y, float z, float entityGoalWidth, float distance)
        {
            _path.ClearPath();
            _nodeProcessor.InitProcessor(world, entity);
            PathPoint pointBegin = _nodeProcessor.GetPathPointTo(entity);
            PathPoint pointEnd = _nodeProcessor.GetPathPointToCoords(entity, x, y, z);
            if (pointEnd != null)
            {
                PathEntity points = InitEntityPath(entity, pointBegin, pointEnd, x, y, z, entityGoalWidth, distance);
                _nodeProcessor.PostProcess();
                return points;
            }
            return null;
        }

        /// <summary>
        /// Инициализировать путь объекта
        /// </summary>
        /// <param name="entityGoalWidth">Ширина сущности к которой идём</param>
        private PathEntity InitEntityPath(EntityMob entity, PathPoint pointBegin, PathPoint pointEnd, float x, float y, float z, float entityGoalWidth, float distance)
        {
            pointBegin.TotalPathDistance = 0f;
            pointBegin.DistanceToNext = pointBegin.DistanceToSquared(pointEnd);
            pointBegin.DistanceToTarget = pointBegin.DistanceToNext;
            _path.ClearPath();
            _path.AddPoint(pointBegin);
            PathPoint point = pointBegin;
            int count = 0;
            bool isAcceptanceRadius = _stopOnOverlap ? entityGoalWidth != 0 : false;
            float acceptanceRadius = isAcceptanceRadius ? entityGoalWidth + entity.Size.GetWidth() + .5f : .5f;
            if (_acceptanceRadius > 0)
            {
                isAcceptanceRadius = true;
                acceptanceRadius += _acceptanceRadius;
            }
            ///acceptanceRadius">Приемочный радиус
            while (!_path.IsPathEmpty())
            {
                PathPoint point2 = _path.Dequeue();

                if (point2.Equals(pointEnd) || (isAcceptanceRadius && point2.DistanceTo(x, y, z) < acceptanceRadius))
                {
                    //world.Log.Log("Pathfind.CountEnd {0}", count);
                    return CreateEntityPath(pointBegin, point2, true);
                }

                if (point2.DistanceToSquared(pointEnd) < point.DistanceToSquared(pointEnd))
                {
                    point = point2;
                }

                point2.Visited = true;
                int countSide = _nodeProcessor.FindPathOptions(_pathOptions, entity, point2, pointEnd, distance);

                for (int i = 0; i < countSide; ++i)
                {
                    PathPoint point3 = _pathOptions[i];
                    float distance2 = point2.TotalPathDistance + point2.DistanceToSquared(point3);

                    if (distance2 < distance * 2f && (!point3.IsAssigned() || distance2 < point3.TotalPathDistance))
                    {
                        point3.Previous = point2;
                        point3.TotalPathDistance = distance2;
                        point3.DistanceToNext = point3.DistanceToSquared(pointEnd);

                        if (point3.IsAssigned())
                        {
                            _path.ChangeDistance(point3, point3.TotalPathDistance + point3.DistanceToNext);
                        }
                        else
                        {
                            point3.DistanceToTarget = point3.TotalPathDistance + point3.DistanceToNext;
                            _path.AddPoint(point3);
                        }
                    }
                }
                if (++count >= PathConst.PathMaximumLength) break;
            }
            //world.Log.Log("Pathfind.Count {0}", count);
            return point == pointBegin ? null : CreateEntityPath(pointBegin, point, point.Equals(pointEnd));
        }

        /// <summary>
        /// Возвращает новый путь сущности для данной начальной и конечной точки
        /// </summary>
        /// <param name="pointBegin">начальная точка</param>
        /// <param name="pointEnd">конечная точка</param>
        /// <param name="isDestinationSame">Является ли пункт назначения одинаковым</param>
        private PathEntity CreateEntityPath(PathPoint pointBegin, PathPoint pointEnd, bool isDestinationSame)
        {
            // Отладка, последняя точка куда идти
            PathEntity.DebugEnd(_world, pointEnd);
            // TODO:: 2026-05-25 AI Path

            int step = 1;
            PathPoint point;

            for (point = pointEnd; point.Previous != null; point = point.Previous)
            {
                ++step;
            }

            if (step < 2) return null;

            PathPoint[] points = new PathPoint[step - 1];
            point = pointEnd;
            step -= 2;

            for (points[step] = pointEnd; point.Previous != null && step > 0; points[step] = point)
            {
                point = point.Previous;
                --step;
            }

            return new PathEntity(points, isDestinationSame);
        }
    }
}
