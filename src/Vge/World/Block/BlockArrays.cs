using System.Runtime.CompilerServices;
using WinGL.Util;

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
        /// Массив нужности обязательного случайного блока для частичек
        /// </summary>
        public readonly bool[] BlocksRequiredRandomTick;
        /// <summary>
        /// Количество всех блоков
        /// </summary>
        public readonly int Count;

        /// <summary>
        /// Массив прозрачности и излучаемости освещения, для ускорения алгоритмов освещения
        /// </summary>
        private readonly byte[] _blocksLightOpacityValue;
        /// <summary>
        /// Получить массив индексов блоков жидкости, посути их менять нельзя они храняться в Met
        /// </summary>
        private readonly int[] _blockIndexLiquid;

        public BlockArrays()
        {
            Count = BlocksReg.Table.Count;
            BlockAlias = new string[Count];
            BlockObjects = new BlockBase[Count];
            _blocksLightOpacityValue = new byte[Count];
            BlocksRandomTick = new bool[Count];
            BlocksRequiredRandomTick = new bool[Count];
            _blockIndexLiquid = new int[8];

            int countShape = 0;
            BlockBase block;
            byte indexLiquid = 0;
            for (ushort id = 0; id < Count; id++)
            {
                BlockAlias[id] = BlocksReg.Table.GetAlias(id);
                block = BlocksReg.Table[id];
                block.SetIndex(id);
                BlockObjects[id] = block;
                _blocksLightOpacityValue[id] = block.LightOpacityValue;
                BlocksRandomTick[id] = block.NeedsRandomTick;
                BlocksRequiredRandomTick[id] = block.RequiredRandomTick;
                if (block.Liquid && indexLiquid < 8)
                {
                    _blockIndexLiquid[++indexLiquid] = block.IndexBlock;
                    block.SetIndexLiquid(indexLiquid);
                }
                countShape += block.CountShape();
            }

            return;
        }

        /// <summary>
        /// Дополнительная инициализация блоков после инициализации предметов и корректировки id блоков
        /// </summary>
        public void InitializationAfterItems()
        {
            for (ushort id = 0; id < Count; id++)
            {
                BlockObjects[id].InitAfterItems();
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
        public byte GetLightOpacityValue(int index)
        {
            if (LiquidMet.IsAddLiquid(index >> 12))
            {
                return _MaxLightOpacityValue(
                    _blocksLightOpacityValue[LiquidMet.GetAddLiquidIndex(index >> 12)],
                    _blocksLightOpacityValue[index & 0xFFF]
                );
            }
            return _blocksLightOpacityValue[index & 0xFFF];
        }

        /// <summary>
        /// Максимум по каждой половине: непрозрачность (старшие 4 бита) и свет (младшие 4 бита) отдельно
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte _MaxLightOpacityValue(byte a, byte b)
        {
            int opacity = Mth.Max(a & 0xF0, b & 0xF0); // старшие биты, остаются на своём месте
            int light = Mth.Max(a & 0x0F, b & 0x0F);   // младшие биты
            return (byte)(opacity | light);
        }

        /// <summary>
        /// Полностью прозрачный ли блок, LightOpacity = 0 = Air
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTransparent(int index) => GetLightOpacity(index) == 0;

        /// <summary>
        /// Получить прозрачность блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetLightOpacity(int index)
        {
            if (LiquidMet.IsAddLiquid(index >> 12))
            {
                return Mth.Max(
                    _blocksLightOpacityValue[LiquidMet.GetAddLiquidIndex(index >> 12)] >> 4,
                    _blocksLightOpacityValue[index & 0xFFF] >> 4
                );
            }
            return _blocksLightOpacityValue[index & 0xFFF] >> 4;
        }

        /// <summary>
        /// Получить излучаемости освещения блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetLightValue(int index)
        {
            if (LiquidMet.IsAddLiquid(index >> 12))
            {
                return Mth.Max(
                    _blocksLightOpacityValue[LiquidMet.GetAddLiquidIndex(index >> 12)] & 0xF,
                    _blocksLightOpacityValue[index & 0xFFF] & 0xF
                );
            }
            return _blocksLightOpacityValue[index & 0xFFF] & 0xF;
        }

        /// <summary>
        /// Сверить отличаются ли прозрачность у блоков
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EqualsLightOpacity(int indexOne, int indexTwo)
            => GetLightOpacity(indexOne) != GetLightOpacity(indexTwo);

        /// <summary>
        /// Сверить отличаются ли излучаемость освещения у блоков
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EqualsLightValue(int indexOne, int indexTwo)
            => GetLightValue(indexOne) != GetLightValue(indexTwo);

        #endregion
    }
}
