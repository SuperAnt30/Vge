using System;
using Vge.Renderer.World.Entity;
using Vge.World;

namespace Vge.Entity
{
    /// <summary>
    /// Различные массивы моделей сущности
    /// </summary>
    public sealed class ModelEntityArrays
    {
        /// <summary>
        /// Псевдоним игрока
        /// </summary>
        public const string AliasPlayer = "Player";
        /// <summary>
        /// Массив названий сущностей
        /// </summary>
        public readonly string[] ModelEntitiesAlias;
        /// <summary>
        /// Массив объектов сущностей
        /// </summary>
        private readonly ModelEntity[] _modelEntitiesObjects;
        /// <summary>
        /// Количество всех сущностей
        /// </summary>
        public readonly int Count;
        /// <summary>
        /// Индекс игрока, в целом должен быть 0, так-как первый по списку
        /// </summary>
        public readonly ushort IndexPlayer = 0;

        public ModelEntityArrays()
        {
            Count = ModelEntitiesReg.Table.Count;
            ModelEntitiesAlias = new string[Count];
            _modelEntitiesObjects = new ModelEntity[Count];

            ModelEntity modelEntity;
            for (ushort id = 0; id < Count; id++)
            {
                ModelEntitiesAlias[id] = ModelEntitiesReg.Table.GetAlias(id);
                if (ModelEntitiesAlias[id] == AliasPlayer)
                {
                    IndexPlayer = id;
                }
                modelEntity = ModelEntitiesReg.Table[id];
                modelEntity.SetIndex(id);
                _modelEntitiesObjects[id] = modelEntity;
            }
        }

        /// <summary>
        /// Получить модель сущности по индексу из таблицы сервера.
        /// </summary>
        public ModelEntity GetModelEntity(ushort index)
        {
            if (index < Count)
            {
                return _modelEntitiesObjects[index];
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
                EntityBase entity = Activator.CreateInstance(_modelEntitiesObjects[index].EntityType) as EntityBase;
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
                EntityBase entity = Activator.CreateInstance(_modelEntitiesObjects[index].EntityType) as EntityBase;
                entity.InitServer(index, collision);
                return entity;
            }
            throw new Exception(Sr.GetString(Sr.IndexOutsideEntityType, index, Count));
        }

    }
}
