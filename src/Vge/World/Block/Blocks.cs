namespace Vge.World.Block
{
    /// <summary>
    /// Различные массивы блоков
    /// </summary>
    public sealed class Blocks
    {
        /*
        BlocksLightOpacity[0] = LightOpacity << 4 | LightValue
        LightOpacity = BlocksLightOpacity[0] >> 4
        LightValue = BlocksLightOpacity[0] & 0xF
        */

        /// <summary>
        /// Массив названий блоков
        /// </summary>
        public readonly static string[] BlockAlias;
        /// <summary>
        /// Массив объектов блоков
        /// </summary>
        public readonly static BlockBase[] BlockObjects;
        /// <summary>
        /// Массив прозрачности и излучаемости освещения, для ускорения алгоритмов освещения
        /// </summary>
        public readonly static byte[] BlocksLightOpacity;
        /// <summary>
        /// Массив нужности случайного тика для блока
        /// </summary>
        public readonly static bool[] BlocksRandomTick;
        /// <summary>
        /// Массив с дополнительными metdata свыше 4 бита
        /// </summary>
        public readonly static bool[] BlocksAddMet;
        /// <summary>
        /// Количество всех блоков
        /// </summary>
        public readonly static int Count;

        static Blocks()
        {
            Count = BlocksReg.Table.Count;
            BlockAlias = new string[Count];
            BlockObjects = new BlockBase[Count];
            BlocksLightOpacity = new byte[Count];
            BlocksRandomTick = new bool[Count];
            BlocksAddMet = new bool[Count];

            ushort id;
            BlockBase block;
            for (id = 0; id < Count; id++)
            {
                BlockAlias[id] = BlocksReg.Table.GetAlias(id);
                BlockObjects[id] = block = BlocksReg.Table[id];
                BlocksLightOpacity[id] = (byte)(block.LightOpacity << 4 | block.LightValue);
                BlocksRandomTick[id] = block.NeedsRandomTick;
                BlocksAddMet[id] = block.IsAddMet;
            }

            for (id = 0; id < Count; id++)
            {
                BlockObjects[id].Initialization();
            }
        }

        /// <summary>
        /// Дополнительная инициализация блоков после инициализации предметов
        /// </summary>
        public static void InitializationAfterItems()
        {
            for (int id = 0; id < Count; id++)
            {
                BlockObjects[id].InitializationAfterItems();
            }
        }
    }
}
