using System.Runtime.CompilerServices;

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
        /// Массив нужности случайного тика для блока
        /// </summary>
        public readonly bool[] BlocksRandomTick;
        /// <summary>
        /// Количество всех блоков
        /// </summary>
        public readonly int Count;

        /// <summary>
        /// Массив прозрачности и излучаемости освещения, для ускорения алгоритмов освещения
        /// </summary>
        private readonly byte[] _blocksLightOpacity;
        /// <summary>
        /// Массив прозрачности зависящий от Metdata блока
        /// </summary>
        private readonly bool[] _blocksOpacityMet;
        /// <summary>
        /// Массив излучаемости освещения зависящий от Metdata блока
        /// </summary>
        private readonly bool[] _blocksLightMet;
        /// <summary>
        /// Получить массив индексов блоков жидкости, посути их менять нельзя они храняться в Met
        /// </summary>
        private readonly int[] _blockIndexLiquid;

        public BlockArrays()
        {
            Count = BlocksReg.Table.Count;
            BlockAlias = new string[Count];
            BlockObjects = new BlockBase[Count];
            _blocksLightOpacity = new byte[Count];
            _blocksOpacityMet = new bool[Count];
            _blocksLightMet = new bool[Count];
            BlocksRandomTick = new bool[Count];
            _blockIndexLiquid = new int[8];

            BlockBase block;
            byte indexLiquid = 0;
            for (ushort id = 0; id < Count; id++)
            {
                BlockAlias[id] = BlocksReg.Table.GetAlias(id);
                block = BlocksReg.Table[id];
                block.SetIndex(id);
                BlockObjects[id] = block;
                _blocksLightOpacity[id] = (byte)(block.LightOpacity << 4 | block.LightValue);
                BlocksRandomTick[id] = block.NeedsRandomTick;
                if (block.Liquid && indexLiquid < 8)
                {
                    _blockIndexLiquid[++indexLiquid] = block.IndexBlock;
                    block.SetIndexLiquid(indexLiquid);
                }
            }
        }

        /// <summary>
        /// Дополнительная инициализация блоков после инициализации предметов
        /// </summary>
        public void InitializationAfterItemsN3()
        {
            for (ushort id = 0; id < Count; id++)
            {
                BlockObjects[id].InitAfterItemsN3();
            }
        }

        /// <summary>
        /// Получить дополнительный блок жидкости
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockBase GetAddLiquid(int met) 
            => Ce.Blocks.BlockObjects[_blockIndexLiquid[met >> 16]];

        /// <summary>
        /// Получить индекс дополнительного блока жидкости
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAddLiquidIndex(int met) => _blockIndexLiquid[met >> 16];

        #region Light

        /// <summary>
        /// Получить прозрачности и излучаемости освещения блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetLightOpacity(int index)
        {
            if (_blocksOpacityMet[index & 0xFFF] || _blocksLightMet[index & 0xFFF])
            {
                // Это временно, потом через метданные запустим
                return (byte)(BlockObjects[index & 0xFFF].GetLightOpacity(index >> 12) 
                    | BlockObjects[index & 0xFFF].GetLightValue(index >> 12));
            }
            return _blocksLightOpacity[index & 0xFFF];
        }

        /// <summary>
        /// Полностью прозрачный ли блок, LightOpacity = 0 = Air
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTransparent(int index)
        {
            if (_blocksOpacityMet[index & 0xFFF])
            {
                // Это временно, потом через метданные запустим
                return BlockObjects[index & 0xFFF].GetLightOpacity(index >> 12) == 0;
            }
            return (_blocksLightOpacity[index & 0xFFF] >> 4) == 0;
        }

        /// <summary>
        /// Получить прозрачность блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOpacity(int index)
        {
            if (_blocksOpacityMet[index & 0xFFF])
            {
                // Это временно, потом через метданные запустим
                return BlockObjects[index & 0xFFF].GetLightOpacity(index >> 12);
            }
            return _blocksLightOpacity[index & 0xFFF] >> 4;
        }

        /// <summary>
        /// Получить излучаемости освещения блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetLightValue(int index)
        {
            if (_blocksLightMet[index & 0xFFF])
            {
                return BlockObjects[index & 0xFFF].GetLightValue(index >> 12);
            }
            return _blocksLightOpacity[index & 0xFFF] & 0xF;
        }

        #endregion
    }
}
