using System;
using System.Runtime.CompilerServices;
using Vge.Renderer.World;
using Vge.World;

namespace Vge.Entity.Particle
{
    /// <summary>
    /// Различные массивы частичек
    /// </summary>
    public sealed class EntityFXArrays
    {
        /// <summary>
        /// Массив названий частичек
        /// </summary>
        public readonly string[] EntitiesFXAlias;
        /// <summary>
        /// Массив объектов сущностей
        /// </summary>
        private readonly ResourcesEntityBase[] _entitiesObjects;
        /// <summary>
        /// Количество всех различных сущностей
        /// </summary>
        public readonly int Count;

        public EntityFXArrays()
        {
            Count = EntitiesFXReg.Table.Count;
            EntitiesFXAlias = new string[Count];
            _entitiesObjects = new ResourcesEntityBase[Count];

            ResourcesEntityBase resourcesEntity;
            for (ushort id = 0; id < Count; id++)
            {
                EntitiesFXAlias[id] = EntitiesFXReg.Table.GetAlias(id);
                resourcesEntity = EntitiesFXReg.Table[id];
                resourcesEntity.SetIndex(id);
                _entitiesObjects[id] = resourcesEntity;
            }
        }

        /// <summary>
        /// Получить ресурсы сущности по индексу из таблицы сервера.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ResourcesEntityBase GetResourcesEntity(ushort index)
        {
            if (index < Count)
            {
                return _entitiesObjects[index];
            }
            throw new Exception(Sr.GetString(Sr.IndexOutsideEntityType, index, Count));
        }

        /// <summary>
        /// Создать сущность для клиента по индексу из таблицы сервера.
        /// Регистрацию индексов сущностей можно заполнить в AllWorlds.Init()
        /// </summary>
        public EntityFX CreateEntityClient(ushort index, WorldClient world, ParticlesRenderer renderer, int parameter)
        {
            if (index < Count)
            {
                EntityFX entity = Activator.CreateInstance(_entitiesObjects[index].EntityType) as EntityFX;
                entity.Init(index, world, renderer, parameter);
                return entity;
            }
            throw new Exception(Sr.GetString(Sr.IndexOutsideEntityType, index, Count));
        }
    }
}
