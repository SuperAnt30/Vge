using System.Runtime.CompilerServices;
using Vge.Util;
using WinGL.Util;

namespace Vge.Entity.Sizes
{
    /// <summary>
    /// Размер отсутствует это точка, вес сущностей которая работает с физикой
    /// </summary>
    public readonly struct SizeEntityPoint : ISizeEntity
    {
        /// <summary>
        /// Сущность к которой прекреплена физика
        /// </summary>
        public readonly EntityBase Entity;

        /// <summary>
        /// Вес сущности
        /// </summary>
        private readonly int _weight;

        public SizeEntityPoint(EntityBase entity, int weight)
        {
            Entity = entity;
            _weight = weight;
        }

        /// <summary>
        /// Высота сущности. Делаем размер 1/16 блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetHeight() => .0625f;

        /// <summary>
        /// Пол ширины сущности. Делаем размер 1/16 блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetWidth() => .03125f;

        /// <summary>
        /// Вес сущности для определения импулса между сущностями,
        /// У кого больше вес тот больше толкает или меньше потдаётся импульсу.
        /// В килограммах.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetWeight() => _weight;

        /// <summary>
        /// Возвращает, пересекается ли данная сущность с other
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IntersectsWith(AxisAlignedBB other)
            => other.IsPointInside(Entity.PosX, Entity.PosY, Entity.PosZ);

        /// <summary>
        /// Возвращает, пересекается ли данная сущность с блоком
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IntersectsWithBlock(int x, int y, int z)
            => x + 1 <= Entity.PosX && x >= Entity.PosX
                && y + 1 <= Entity.PosY && y >= Entity.PosY
                && z + 1 <= Entity.PosZ && z >= Entity.PosZ;

        /// <summary>
        /// Рассчитать точку пересечения Hitbox и отрезка, в виде вектора от pos1 до pos2
        /// [Игнорируем]
        /// </summary>
        public PointIntersection CalculateIntercept(Vector3 pos1, Vector3 pos2) => new PointIntersection();

        /// <summary>
        /// Игнорируем если это точка, только точка может взаимодействовать с сущностью!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float CalculateXOffset(AxisAlignedBB other, float offset) => offset;

        /// <summary>
        /// Игнорируем если это точка, только точка может взаимодействовать с сущностью!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float CalculateYOffset(AxisAlignedBB other, float offset) => offset;

        /// <summary>
        /// Игнорируем если это точка, только точка может взаимодействовать с сущностью!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float CalculateZOffset(AxisAlignedBB other, float offset) => offset;
    }
}
