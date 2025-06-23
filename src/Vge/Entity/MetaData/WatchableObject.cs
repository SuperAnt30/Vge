namespace Vge.Entity.MetaData
{
    /// <summary>
    /// Структура наблюдаемого объекта
    /// </summary>
    public struct WatchableObject
    {
        /// <summary>
        /// Тип объекта 
        /// </summary>
        public readonly EnumTypeWatcher ObjectType;
        /// <summary>
        /// Index, хранения в массиве
        /// </summary>
        public readonly byte Index;
        /// <summary>
        /// Наблюдаемый объект
        /// </summary>
        public object WatchedObject;
        /// <summary>
        /// Меняется данный объект
        /// </summary>
        public bool Changing;

        public WatchableObject(EnumTypeWatcher type, byte index, object obj)
        {
            Index = index;
            WatchedObject = obj;
            ObjectType = type;
            Changing = true;
        }
    }
}
