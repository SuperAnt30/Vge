﻿namespace Vge.World.Chunk
{
    /// <summary>
    /// Опции чанка, по высотам
    /// </summary>
    public class ChunkSettings
    {
        /// <summary>
        /// Количество секций в чанке  (old COUNT_HEIGHT)
        /// </summary>
        public byte NumberSections { get; private set; }
        /// <summary>
        /// Количество секций в чанке меньше. NumberChunkSections - 1 (old COUNT_HEIGHT15)
        /// </summary>
        public byte NumberSectionsLess { get; private set; }
        /// <summary>
        /// Количество блоков в чанке. NumberChunkSections * 16 - 1 (old COUNT_HEIGHT_BLOCK)
        /// </summary>
        public ushort NumberBlocks { get; private set; }
        /// <summary>
        /// Верхний блок который можно установить. NumberBlocks - 1 (old MAX_HEIGHT_BLOCK)
        /// </summary>
        public ushort NumberMaxBlock { get; private set; }

        /// <summary>
        /// Задать высоту чанков
        /// </summary>
        public void SetHeightChunks(byte height)
        {
            NumberSections = height;
            NumberSectionsLess = (byte)(height - 1);
            NumberBlocks = (ushort)(height * 16 - 1);
            NumberMaxBlock = (ushort)(height * 16 - 2);
        }
    }
}
