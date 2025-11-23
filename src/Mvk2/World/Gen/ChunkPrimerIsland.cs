using Mvk2.World.Biome;
using System;
using System.Runtime.CompilerServices;
using Vge.Util;
using WinGL.Util;

namespace Mvk2.World.Gen
{
    /// <summary>
    /// Подготовительный чанк, для генерации мира остров
    /// x << 12 | z << 8 | y;
    /// </summary>
    public class ChunkPrimerIsland
    {
        /// <summary>
        /// Данные блока
        /// 12 bit Id блока и 4 bit параметр блока
        /// y << 8 | z << 4 | x;
        /// 65536 = 16 * 16 * 256
        /// 32768 = 16 * 16 * 128
        /// </summary>
        public readonly ushort[] Id;
        public readonly uint[] Met;
        /// <summary>
        /// Массив блоков которые светятся
        /// Координаты в битах 0000 0000 0000 000y  yyyy yyyy zzzz xxxx
        /// </summary>
        public readonly ArrayFast<uint> ArrayLightBlocks;
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
        /// Количество возможных блоков в чанке
        /// </summary>
        private readonly int _count;

        public ChunkPrimerIsland(int numberChunkSections)
        {
            _count = 4096 * numberChunkSections;
            Id = new ushort[_count];
            Met = new uint[_count];
            ArrayLightBlocks = new ArrayFast<uint>(_count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlockState(int xz, int y, ushort id, uint met = 0)
        {
            int index = y << 8 | xz;
            Id[index] = id;
            if (met != 0) Met[index] = met;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlockState(int x, int y, int z, ushort id, uint met = 0)
        {
            int index = y << 8 | z << 4 | x;
            Id[index] = id;
            if (met != 0) Met[index] = met;
        }

        /// <summary>
        /// Очистить
        /// </summary>
        public void Clear()
        {
            Array.Clear(Id, 0, _count);
            Array.Clear(Met, 0, _count);
            for (int i = 0; i < 256; i++)
            {
                Biome[i] = EnumBiomeIsland.Plain;
                HeightMap[i] = 0;
            }
            ArrayLightBlocks.Clear();
        }
    }
}
