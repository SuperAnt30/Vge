using System.Runtime.CompilerServices;
using Vge.World;
using Vge.World.Block;
using WinGL.Util;

namespace Vge.Entity.AI.PathFinding
{
    /// <summary>
    /// Абстрактный класс навигации моба
    /// </summary>
    public abstract class PathNavigate
    {
        protected readonly EntityMob _entity;
        protected readonly WorldServer _world;

        /// <summary>
        /// Отслеживаемый объект
        /// </summary>
        public PathEntity CurrentPath { get; protected set; }
        /// <summary>
        /// Скорость перемещения
        /// </summary>
        protected float _speed;
        /// <summary>
        /// Время в тиках по текущему пути
        /// </summary>
        private int _totalTicks;
        /// <summary>
        /// Время последней проверки позиции (для обнаружения успешного движения)
        /// </summary>
        private int _ticksAtLastPos;
        /// <summary>
        /// Координаты положения объекта в последний раз, когда выполнялась проверка (часть мониторинга «зависания»)
        /// </summary>
        private Vector3 _lastPosCheck = new Vector3(0);

        protected readonly PathFinder _pathFinder;

        public PathNavigate(EntityMob entity, WorldServer world)
        {
            _entity = entity;
            _world = world;
            _pathFinder = _GetPathFinder();
        }

        /// <summary>
        /// Получить объект PathFinder
        /// </summary>
        protected abstract PathFinder _GetPathFinder();
        /// <summary>
        /// Может ли перемещаться
        /// </summary>
        protected abstract bool _CanNavigate();
        /// <summary>
        /// Тикущая позиция сущности
        /// </summary>
        protected abstract Vector3 _GetEntityPosition();
        /// <summary>
        /// Возвращает true, если объект указанного размера может безопасно пройти по прямой линии между двумя точками
        /// </summary>
        protected abstract bool _IsDirectPathBetweenPoints(Vector3 pos1, Vector3 pos2, 
            int sizeX, int sizeY, int sizeZ);

        /// <summary>
        /// Обрезает данные пути от конца до первого блока, покрытого солнцем
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void _RemoveSunnyPath() { }

        /// <summary>
        /// Возвращает true, если объект находится в воде, лаве или нефте, иначе false
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool _IsInLiquid() => _entity.PresenceBlocks.IsInLiquid;

        /// <summary>
        /// Возвращает путь к заданным координатам
        /// </summary>
        public virtual PathEntity GetPathToXYZ(BlockPos blockPos)
        {
            if (!_CanNavigate()) return null;
            return _pathFinder.CreateEntityPathTo(_world, _entity, blockPos, PathConst.PathSearchRange);
        }

        /// <summary>
        /// Возвращает путь к заданному EntityLiving
        /// </summary>
        public virtual PathEntity GetPathToEntityLiving(EntityLiving entity)
        {
            if (!_CanNavigate()) return null;
            return _pathFinder.CreateEntityPathTo(_world, _entity, entity, PathConst.PathSearchRange);
        }

        /// <summary>
        /// Попробуйте найти и указать путь к XYZ. Возвращает true в случае успеха
        /// </summary>
        /// <param name="allowPartialPath">Разрешить частичный путь</param>
        /// <param name="stopOnOverlap">Задать остановку при соприкосновении коллизии, true - соприкосновении коллизии, false - центр</param>
        /// <param name="acceptanceRadius">Задать расстояние до точки, которое будет считаться выполненым, 0 - расчёта нет</param>
        public bool TryMoveToXYZ(float x, float y, float z, float speed, bool allowPartialPath = true, bool stopOnOverlap = false, float acceptanceRadius = 0f)
        {
            _pathFinder.SetOptions(stopOnOverlap, acceptanceRadius);
            BlockPos blockPos = new BlockPos(x, y, z);
            PathEntity path = GetPathToXYZ(blockPos);
            // Проверка на доступ чтоб сущность могла дойти до конечной точки
            if (!allowPartialPath && path != null && !path.IsDestinationSame()) return false;
            return SetPath(path, speed);
        }

        /// <summary>
        /// Попробуйте найти и указать путь к EntityLiving. Возвращает true в случае успеха
        /// </summary>
        /// <param name="allowPartialPath">Разрешить частичный путь </param>
        /// <param name="stopOnOverlap">Задать остановку при соприкосновении коллизии, true - соприкосновении коллизии, false - центр</param>
        /// <param name="acceptanceRadius">Задать расстояние до точки, которое будет считаться выполненым, 0 - расчёта нет</param>
        public virtual bool TryMoveToEntityLiving(EntityLiving entity, float speed, bool allowPartialPath = true, bool stopOnOverlap = false, float acceptanceRadius = 0f)
        {
            _pathFinder.SetOptions(stopOnOverlap, acceptanceRadius);
            PathEntity path = GetPathToEntityLiving(entity);
            // Проверка на доступ чтоб сущность могла дойти до конечной точки
            if (!allowPartialPath && path != null && !path.IsDestinationSame()) return false;
            return SetPath(path, speed);
        }

        /// <summary>
        /// Задает новый путь. Если он отличается от старого пути. 
        /// Проверяет корректировку пути для избегания солнца и сохраняет начальные координаты.
        /// </summary>
        public bool SetPath(PathEntity path, float speed)
        {
            if (path == null)
            {
                CurrentPath = null;
                return false;
            }
            if (!path.IsSamePath(CurrentPath))
            {
                CurrentPath = path;
            }

            // Отладка, визуализация перемещения
            //path.DebugPath(_world);

            _RemoveSunnyPath();

            if (CurrentPath.GetCurrentPathLength() == 0)
            {
                return false;
            }

            _speed = speed;
            _ticksAtLastPos = _totalTicks;
            _lastPosCheck = _GetEntityPosition();
            return true;
        }

        /// <summary>
        /// Если путь нулевой или достигнут конец
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NoPath() => CurrentPath == null || CurrentPath.IsFinished();

        /// <summary>
        /// Устанавливает для активного PathEntity значение null
        /// Нельзя очищать навигациию в ResetTask!!! Так-как новая задача по навигации может быть очищена прошлой задачей
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearPathEntity() => CurrentPath = null;

        public virtual void OnUpdateNavigation()
        {
            ++_totalTicks;
            if (!NoPath())
            {
                Vector3 pos;

                if (_CanNavigate())
                {
                    _PathFollow();
                }
                else if (CurrentPath != null && CurrentPath.GetCurrentPathIndex() < CurrentPath.GetCurrentPathLength())
                {
                    pos = _GetEntityPosition();
                    Vector3 pos2 = CurrentPath.GetVectorFromIndex(_entity, CurrentPath.GetCurrentPathIndex());

                    if (pos.Y > pos2.Y && !_entity.OnGround && Mth.Floor(pos.X) == Mth.Floor(pos2.X) 
                        && Mth.Floor(pos.Z) == Mth.Floor(pos2.Z))
                    {
                        CurrentPath.SetCurrentPathIndex(CurrentPath.GetCurrentPathIndex() + 1);
                    }
                }

                if (!NoPath())
                {
                    _entity.MoveHelper.SetMoveTo(CurrentPath.GetPosition(_entity), _speed);
                }
            }
        }

        /// <summary>
        /// Следовать пути
        /// </summary>
        protected virtual void _PathFollow()
        {
            Vector3 pos = _GetEntityPosition();
            int leght = CurrentPath.GetCurrentPathLength();
            int i;

            for (i = CurrentPath.GetCurrentPathIndex(); i < CurrentPath.GetCurrentPathLength(); ++i)
            {
                if (CurrentPath.GetPathPointFromIndex(i).CoordY != (int)pos.Y)
                {
                    leght = i;
                    break;
                }
            }

            float squareDistance = _pathFinder.GetStopOnOverlap()
                ? _entity.Size.GetWidth() * _entity.Size.GetWidth() * 4 : .0625f;

            for (i = CurrentPath.GetCurrentPathIndex(); i < leght; ++i)
            {
                Vector3 vec = CurrentPath.GetVectorFromIndex(_entity, i);

                if (Glm.SquareDistanceTo(pos, vec) < squareDistance)
                {
                    CurrentPath.SetCurrentPathIndex(i + 1);
                }
            }

            i = Mth.Ceiling(_entity.Size.GetWidth() * 2f);
            int height = (int)_entity.Size.GetHeight() + 1;

            for (int j = leght - 1; j >= CurrentPath.GetCurrentPathIndex(); --j)
            {
                if (_IsDirectPathBetweenPoints(pos, CurrentPath.GetVectorFromIndex(_entity, j), i, height, i))
                {
                    CurrentPath.SetCurrentPathIndex(j);
                    break;
                }
            }

            _CheckForStuck(pos);
        }

        /// <summary>
        /// Проверяет, не был ли объект перемещен при последней проверке, и если да, очищает текущий PathEntity
        /// </summary>
        protected void _CheckForStuck(Vector3 pos)
        {
            if (_totalTicks - _ticksAtLastPos > PathConst.CountTickNotPath)
            {
                if (Glm.SquareDistanceTo(pos, _lastPosCheck) < 2.25f)
                {
                    ClearPathEntity();
                }
                _ticksAtLastPos = _totalTicks;
                _lastPosCheck = pos;
            }
        }
    }
}
