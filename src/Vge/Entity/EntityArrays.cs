using System;
using System.Runtime.CompilerServices;
using Vge.Renderer.World.Entity;
using Vge.World;

namespace Vge.Entity
{
    /// <summary>
    /// Различные массивы сущности
    /// </summary>
    public sealed class EntityArrays
    {
        /// <summary>
        /// Псевдоним игрока
        /// </summary>
        public const string AliasPlayer = "Player";
        /// <summary>
        /// Массив названий сущностей
        /// </summary>
        public readonly string[] EntitiesAlias;
        /// <summary>
        /// Массив объектов сущностей
        /// </summary>
        private readonly ResourcesEntityBase[] _entitiesObjects;
        /// <summary>
        /// Количество всех различных сущностей
        /// </summary>
        public readonly int Count;
        /// <summary>
        /// Индекс игрока, в целом должен быть 0, так-как первый по списку
        /// </summary>
        public readonly ushort IndexPlayer = 0;

        public EntityArrays()
        {
            Count = EntitiesReg.Table.Count;
            EntitiesAlias = new string[Count];
            _entitiesObjects = new ResourcesEntityBase[Count];

            ResourcesEntityBase resourcesEntity;
            for (ushort id = 0; id < Count; id++)
            {
                EntitiesAlias[id] = EntitiesReg.Table.GetAlias(id);
                if (EntitiesAlias[id] == AliasPlayer)
                {
                    IndexPlayer = id;
                }
                resourcesEntity = EntitiesReg.Table[id];
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
        public EntityBase CreateEntityClient(ushort index, EntitiesRenderer entitiesRenderer)
        {
            if (index < Count)
            {
                EntityBase entity = Activator.CreateInstance(_entitiesObjects[index].EntityType) as EntityBase;
                entity.InitRender(index, entitiesRenderer);
                return entity;
            }
            throw new Exception(Sr.GetString(Sr.IndexOutsideEntityType, index, Count));
        }

        /// <summary>
        /// Создать сущность для сервера по индексу из таблицы сервера.
        /// Регистрацию индексов сущностей можно заполнить в GameModClient.InitAfterStartGame() 
        /// </summary>
        public EntityBase CreateEntityServer(ushort index, CollisionBase collision)
        {
            if (index < Count)
            {
                EntityBase entity = Activator.CreateInstance(_entitiesObjects[index].EntityType) as EntityBase;
                entity.InitServer(index, collision);
                return entity;
            }
            throw new Exception(Sr.GetString(Sr.IndexOutsideEntityType, index, Count));
        }

    }
}
