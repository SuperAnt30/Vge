namespace Vge.Entity
{
    /// <summary>
    /// Различные массивы моделей сущности
    /// </summary>
    public sealed class ModelEntityArrays
    {
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

        public ModelEntityArrays()
        {
            Count = ModelEntitiesReg.Table.Count;
            ModelEntitiesAlias = new string[Count];
            ModelEntitiesObjects = new ModelEntity[Count];

            ModelEntity modelEntity;
            for (ushort id = 0; id < Count; id++)
            {
                ModelEntitiesAlias[id] = ModelEntitiesReg.Table.GetAlias(id);
                ModelEntitiesObjects[id] = modelEntity = ModelEntitiesReg.Table[id];
            }
        }
    }
}
