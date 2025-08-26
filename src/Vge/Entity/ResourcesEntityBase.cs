using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Shape;

namespace Vge.Entity
{
    /// <summary>
    /// Базовые ресурсы сущности, нужны везде, без формы
    /// </summary>
    public class ResourcesEntityBase
    {
        /// <summary>
        /// Индекс сущности из таблицы
        /// </summary>
        public ushort IndexEntity { get; private set; }
        /// <summary>
        /// Название сущности
        /// </summary>
        public readonly string Alias;
        /// <summary>
        /// Тип объекта сущности
        /// </summary>
        public readonly Type EntityType;
        
        public ResourcesEntityBase(string alias, Type entityType)
        {
            Alias = alias;
            EntityType = entityType;
        }

        /// <summary>
        /// Задать индекс сущности, из таблицы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetIndex(ushort id) => IndexEntity = id;

        /// <summary>
        /// Заменить буфер из-за смены размера текстуры
        /// </summary>
        public virtual void SetBufferMeshBecauseSizeTexture(ShapeEntity shapeEntity) { }

        public override string ToString() => Alias + " Index:" + IndexEntity;
    }
}
