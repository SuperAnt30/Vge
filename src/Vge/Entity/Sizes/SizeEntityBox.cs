using System.Runtime.CompilerServices;
using Vge.Util;
using WinGL.Util;

namespace Vge.Entity.Sizes
{
    /// <summary>
    /// Размер, вес сущностей которая работает с физикой
    /// </summary>
    public readonly struct SizeEntityBox : ISizeEntityBox
    {
        /// <summary>
        /// Сущность к которой прекреплена физика
        /// </summary>
        public readonly EntityBase Entity;

        /// <summary>
        /// Пол ширина сущности
        /// </summary>
        private readonly float _width;
        /// <summary>
        /// Высота сущности
        /// </summary>
        private readonly float _height;
        /// <summary>
        /// Вес сущности в килограммах
        /// </summary>
        private readonly int _weight;

        public SizeEntityBox(EntityBase entity, float width, float height, int weight)
        {
            Entity = entity;
            _width = width;
            _height = height;
            _weight = weight;
        }

        /// <summary>
        /// Высота сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetHeight() => _height;

        /// <summary>
        /// Пол ширины сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetWidth() => _width;

        /// <summary>
        /// Вес сущности для определения импулса между сущностями,
        /// У кого больше вес тот больше толкает или меньше потдаётся импульсу.
        /// В килограммах.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetWeight() => _weight;

        /// <summary>
        /// Возвращает, пересекается ли данная ограничивающая рамка с этой
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IntersectsWith(AxisAlignedBB other)
            => GetBoundingBox().IntersectsWith(other);

        /// <summary>
        /// Возвращает, пересекается ли данная ограничивающая рамка с блоком
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IntersectsWithBlock(int x, int y, int z)
            => GetBoundingBox().IntersectsWith(x, y, z);

        /// <summary>
        /// Получить ограничительную рамку сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBB GetBoundingBox()
            => new AxisAlignedBB(Entity.PosX - _width, Entity.PosY, Entity.PosZ - _width,
                Entity.PosX + _width, Entity.PosY + _height, Entity.PosZ + _width);

        /// <summary>
        /// Получить ограничительную рамку сущности со смещением
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AxisAlignedBB GetBoundingBoxOffset(float x, float y, float z)
            => new AxisAlignedBB(Entity.PosX - _width + x, Entity.PosY + y, Entity.PosZ - _width + z,
                Entity.PosX + _width + x, Entity.PosY + _height + y, Entity.PosZ + _width + z);

        /// <summary>
        /// Рассчитать точку пересечения Hitbox и отрезка, в виде вектора от pos1 до pos2
        /// </summary>
        public PointIntersection CalculateIntercept(Vector3 pos1, Vector3 pos2)
            => GetBoundingBox().CalculateIntercept(pos1, pos2);

        /// <summary>
        /// Если экземпляр и ограничивающие рамки аргумента перекрываются в измерениях Y и Z, 
        /// вычислите смещение между ними в измерении X. вернуть offset, если ограничивающие 
        /// рамки не перекрываются или если offset ближе к 0, чем вычисленное смещение. 
        /// В противном случае вернуть рассчитанное смещение.
        /// </summary>
        /// <param name="offset">смещение</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float CalculateXOffset(AxisAlignedBB other, float offset)
            => GetBoundingBox().CalculateXOffset(other, offset);

        /// <summary>
        /// Если экземпляр и ограничивающие рамки аргумента перекрываются в измерениях X и Z, 
        /// вычислите смещение между ними в измерении Y. вернуть offset, если ограничивающие 
        /// рамки не перекрываются или если offset ближе к 0, чем вычисленное смещение. 
        /// В противном случае вернуть рассчитанное смещение.
        /// </summary>
        /// <param name="offset">смещение</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float CalculateYOffset(AxisAlignedBB other, float offset)
            => GetBoundingBox().CalculateYOffset(other, offset);

        /// <summary>
        /// Если экземпляр и ограничивающие рамки аргумента перекрываются в измерениях Y и X, 
        /// вычислите смещение между ними в измерении Z. вернуть offset, если ограничивающие 
        /// рамки не перекрываются или если offset ближе к 0, чем вычисленное смещение. 
        /// В противном случае вернуть рассчитанное смещение.
        /// </summary>
        /// <param name="offset">смещение</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float CalculateZOffset(AxisAlignedBB other, float offset)
            => GetBoundingBox().CalculateZOffset(other, offset);
    }
}
