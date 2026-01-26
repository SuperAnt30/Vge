using Mvk2.World.Biome;
using System;
using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World.Block;
using Vge.World.BlockEntity;
using Vge.World.Gen;

namespace Mvk2.World.Gen
{
    /// <summary>
    /// Подготовительный чанк, для генерации мира остров
    /// x << 12 | z << 8 | y;
    /// </summary>
    public class ChunkPrimerMvk : IChunkPrimer
    {
        /// <summary>
        /// Данные блока
        /// 12 bit Id блока и 4 bit параметр блока
        /// y << 8 | z << 4 | x;
        /// 65536 = 16 * 16 * 256
        /// 32768 = 16 * 16 * 128
        /// </summary>
        public readonly int[] Id;
        public readonly int[] Met;
        /// <summary>
        /// Массив флагов, 
        /// 1 = меняем если не воздух
        /// </summary>
        public readonly byte[] Flag;
        /// <summary>
        /// Массив блоков которые светятся
        /// Координаты в битах 0000 0000 0000 000y  yyyy yyyy zzzz xxxx
        /// </summary>
        public readonly ArrayFast<uint> ArrayLightBlocks;
        /// <summary>
        /// Массив блоков которые тикают
        /// </summary>
        public readonly uint[] Tick;
        /// <summary>
        /// Биомы
        /// z << 4 | x;
        /// </summary>
        public readonly EnumBiomeIsland[] Biome = new EnumBiomeIsland[256];
        /// <summary>
        /// Карта высот
        /// z << 4 | x;
        /// </summary>
        public readonly int[] HeightMap = new int[256];
        /// <summary>
        /// Массив объектов блоков сущностей, пример Дерево.
        /// </summary>
        public readonly ListFast<BlockEntityBase> ListBlockEntity = new ListFast<BlockEntityBase>(10);

        /// <summary>
        /// Количество возможных блоков в чанке
        /// </summary>
        private readonly int _count;

        public ChunkPrimerMvk(int numberChunkSections)
        {
            _count = 4096 * numberChunkSections;
            Id = new int[_count];
            Met = new int[_count];
            Flag = new byte[_count];
            ArrayLightBlocks = new ArrayFast<uint>(_count);
            Tick = new uint[_count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlockState(int xz, int y, int id)
        {
            int index = y << 8 | xz;
            Id[index] = id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlockState(int xz, int y, int id, int met)
        {
            int index = y << 8 | xz;
            Id[index] = id;
            Met[index] = met;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlockIdFlag(int xz, int y, int id, byte flag)
        {
            int index = y << 8 | xz;
            Id[index] = id;
            Flag[index] = flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlockState(int x, int y, int z, int id, int met = 0)
        {
            int index = y << 8 | z << 4 | x;
            Id[index] = id;
            if (met != 0) Met[index] = met;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlockStateTick(int xz, int y, int id, int met, uint tick)
        {
            int index = y << 8 | xz;
            Id[index] = id;
            if (met != 0) Met[index] = met;
            Tick[index] = tick;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBlockId(int xz, int y) => Id[y << 8 | xz];

        /// <summary>
        /// Очистить
        /// </summary>
        public void Clear()
        {
            Array.Clear(Id, 0, _count);
            Array.Clear(Met, 0, _count);
            Array.Clear(Flag, 0, _count);
            for (int i = 0; i < 256; i++)
            {
                Biome[i] = EnumBiomeIsland.Plain;
                HeightMap[i] = 0;
            }
            ArrayLightBlocks.Clear();
            Array.Clear(Tick, 0, _count);
            ListBlockEntity.Clear();
        }

        /// <summary>
        /// Задать блочную структура к конкретному блоку, пример дерево
        /// </summary>
        public void SetBlockEntity(BlockEntityBase blockEntity)
        {
            ListBlockEntity.Add(blockEntity);
        }
    }
}
