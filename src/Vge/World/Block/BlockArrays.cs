namespace Vge.World.Block
{
    /// <summary>
    /// Различные массивы блоков
    /// </summary>
    public sealed class BlockArrays
    {
        /*
        BlocksLightOpacity[0] = LightOpacity << 4 | LightValue
        LightOpacity = BlocksLightOpacity[0] >> 4
        LightValue = BlocksLightOpacity[0] & 0xF
        */

        /// <summary>
        /// Массив названий блоков
        /// </summary>
        public readonly string[] BlockAlias;
        /// <summary>
        /// Массив объектов блоков
        /// </summary>
        public readonly BlockBase[] BlockObjects;
        /// <summary>
        /// Массив прозрачности и излучаемости освещения, для ускорения алгоритмов освещения
        /// </summary>
        public readonly byte[] BlocksLightOpacity;
        /// <summary>
        /// Массив нужности случайного тика для блока
        /// </summary>
        public readonly bool[] BlocksRandomTick;
        /// <summary>
        /// Массив с флагом имеется ли у блока metadata
        /// </summary>
        public readonly bool[] BlocksMetadata;
        /// <summary>
        /// Количество всех блоков
        /// </summary>
        public readonly int Count;

        public BlockArrays()
        {
            Count = BlocksReg.Table.Count;
            BlockAlias = new string[Count];
            BlockObjects = new BlockBase[Count];
            BlocksLightOpacity = new byte[Count];
            BlocksRandomTick = new bool[Count];
            BlocksMetadata = new bool[Count];

            ushort id;
            BlockBase block;
            for (id = 0; id < Count; id++)
            {
                BlockAlias[id] = BlocksReg.Table.GetAlias(id);
                BlockObjects[id] = block = BlocksReg.Table[id];
                BlocksLightOpacity[id] = (byte)(block.LightOpacity << 4 | block.LightValue);
                BlocksRandomTick[id] = block.NeedsRandomTick;
                BlocksMetadata[id] = block.IsMetadata;
            }

            for (id = 0; id < Count; id++)
            {
                BlockObjects[id].InitIdN2(id);
            }
        }

        /// <summary>
        /// Дополнительная инициализация блоков после инициализации предметов
        /// </summary>
        public void InitializationAfterItemsN3()
        {
            for (ushort id = 0; id < Count; id++)
            {
                BlockObjects[id].InitializationAfterItemsN3();
            }
        }
    }
}
