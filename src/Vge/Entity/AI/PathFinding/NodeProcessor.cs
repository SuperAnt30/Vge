using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.World;
using WinGL.Util;

namespace Vge.Entity.AI.PathFinding
{
    /// <summary>
    /// Процесс узла перемещения
    /// </summary>
    public abstract class NodeProcessor
    {
        protected WorldServer _world;
        protected Dictionary<int, PathPoint> _map = new Dictionary<int, PathPoint>();
        protected int _sizeXZ;
        protected int _sizeY;
        protected PathPoint _pathPointEnd;

        /// <summary>
        /// Инициализация
        /// </summary>
        public virtual void InitProcessor(WorldServer world, EntityMob entity)
        {
            _world = world;
            _map.Clear();
            _sizeXZ = Mth.Floor(entity.Size.GetWidth() * 2 + 1f);
            _sizeY = Mth.Floor(entity.Size.GetHeight() + 1);
        }

        /// <summary>
        /// Этот метод вызывается после обработки всех узлов и создания PathEntity
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void PostProcess() { }

        /// <summary>
        /// Возвращает сопоставленную точку или создает и добавляет ее  
        /// </summary>
        protected PathPoint OpenPoint(int x, int y, int z)
        {
            int hash = PathPoint.MakeHash(x, y, z);

            if (!_map.ContainsKey(hash))
            {
                PathPoint point = new PathPoint(x, y, z);
                _map.Add(hash, point);
                return point;
            }
            else if (_pathPointEnd != null && _pathPointEnd.GetHashCode() == hash)
            {
                return _pathPointEnd;
            }

            return null;
        }

        /// <summary>
        /// Возвращает точку пути от куда стартуем
        /// </summary>
        public abstract PathPoint GetPathPointTo(EntityMob entity);

        /// <summary>
        /// Возвращает точку пути куда надо придти
        /// </summary>
        public abstract PathPoint GetPathPointToCoords(EntityMob entity, float x, float y, float z);

        /// <summary>
        /// Найти параметры пути
        /// </summary>
        public abstract int FindPathOptions(PathPoint[] points, EntityMob entity, PathPoint pointBegin, PathPoint pointEnd, float distance);
    }
}
