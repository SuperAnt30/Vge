using Mvk2.World.Biome;
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
        /// x << 12 | z << 8 | y;
        /// 65536 = 16 * 16 * 256
        /// 32768 = 16 * 16 * 128
        /// </summary>
        public readonly ushort[] Id;
        /// <summary>
        /// Массив для списка блоков с освещённости
        /// </summary>
        public readonly ArrayFast<Vector3i> ArrayLightBlocks;
        /// <summary>
        /// Биомы
        /// x << 4 | z;
        /// </summary>
        public readonly EnumBiomeIsland[] Biome = new EnumBiomeIsland[256];
        /// <summary>
        /// Карта высот
        /// x << 4 | z;
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
            ArrayLightBlocks = new ArrayFast<Vector3i>(_count);
        }

        /// <summary>
        /// Очистить
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _count; i++) Id[i] = 0;
            for (int i = 0; i < 256; i++)
            {
                Biome[i] = EnumBiomeIsland.Plain;
                HeightMap[i] = 0;
            }
            ArrayLightBlocks.Clear();
        }
    }
}
