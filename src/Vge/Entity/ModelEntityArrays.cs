using System;

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
        public readonly ModelEntity[] ModelEntitiesObjects;
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
            ModelEntitiesObjects = new ModelEntity[Count];

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
                ModelEntitiesObjects[id] = modelEntity;
            }
        }
    }
}
