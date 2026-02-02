using System;
using Vge.Entity;

namespace Vge.World.BlockEntity
{
    /// <summary>
    /// Различные массивы блоков сущностей
    /// </summary>
    public sealed class BlockEntityArrays
    {
        /// <summary>
        /// Массив названий блоков сущностей
        /// </summary>
        public readonly string[] BlockEntityAlias;
        /// <summary>
        /// Массив объектов блоков сущностей
        /// </summary>
        private readonly ResourcesEntityBase[] _entitiesObjects;
        /// <summary>
        /// Количество всех блоков
        /// </summary>
        public readonly int Count;

        public BlockEntityArrays()
        {
            Count = BlocksEntityReg.Table.Count;
            BlockEntityAlias = new string[Count];
            _entitiesObjects = new ResourcesEntityBase[Count];

            ResourcesEntityBase resourcesEntity;
            for (ushort id = 0; id < Count; id++)
            {
                BlockEntityAlias[id] = BlocksEntityReg.Table.GetAlias(id);
                resourcesEntity = BlocksEntityReg.Table[id];
                resourcesEntity?.SetIndex(id);
                _entitiesObjects[id] = resourcesEntity;
            }
        }

        /// <summary>
        /// Создать сущность для клиента по индексу из таблицы сервера.
        /// Регистрацию индексов сущностей можно заполнить в AllWorlds.Init()
        /// </summary>
        //public BlockEntityBase CreateEntityClient(ushort index, EntitiesRenderer entitiesRenderer)
        //{
        //    if (index < Count)
        //    {
        //        EntityBase entity = Activator.CreateInstance(_entitiesObjects[index].EntityType) as EntityBase;
        //        entity.InitRender(index, entitiesRenderer);
        //        return entity;
        //    }
        //    throw new Exception(Sr.GetString(Sr.IndexOutsideEntityType, index, Count));
        //}

        /// <summary>
        /// Создать сущность для сервера по индексу из таблицы сервера.
        /// Регистрацию индексов сущностей можно заполнить в GameModClient.InitAfterStartGame() 
        /// </summary>
        public BlockEntityBase CreateEntityServer(short index, WorldServer worldServer)
        {
            if (index < Count)
            {
                BlockEntityBase entity = Activator.CreateInstance(_entitiesObjects[index].EntityType) as BlockEntityBase;
                entity.InitServer(index, worldServer);
                return entity;
            }
            throw new Exception(Sr.GetString(Sr.IndexOutsideEntityType, index, Count));
        }
    }
}
