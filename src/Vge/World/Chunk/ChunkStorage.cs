using System;
using System.Collections.Generic;
using Vge.Util;
using Vge.World.Block;

namespace Vge.World.Chunk
{
    /// <summary>
    /// Псевдо чанк с данными вокселей
    /// 16 * 16 * 16
    /// y << 8 | z << 4 | x
    /// </summary>
    public class ChunkStorage
    {
        /// <summary>
        /// Уровень псевдочанка, нижнего блока, т.е. кратно 16. Глобальная координата Y, не чанка
        /// </summary>
        public readonly int YBase;
        /// <summary>
        /// Высота чанка 0 - NumberSections
        /// </summary>
        public readonly int Index;
        /// <summary>
        /// Ключ кэш координат чанка (ulong)(uint)x  32) | ((uint)y
        /// </summary>
        public readonly ulong KeyCash;

        /// <summary>
        /// Данные блока
        /// 12 bit Id блока и 4 bit параметр блока
        /// </summary>
        public ushort[] Data;
        /// <summary>
        /// Освещение блочное, 4 bit используется
        /// </summary>
        public byte[] LightBlock;
        /// <summary>
        /// Освещение небесное, 4 bit используется
        /// </summary>
        public byte[] LightSky;
        /// <summary>
        /// Количество блоков не воздуха
        /// </summary>
        public int CountBlock { get; private set; }
        /// <summary>
        /// Дополнительные данные блока
        /// </summary>
        public readonly Dictionary<ushort, uint> Metadata = new Dictionary<ushort, uint>();

        /// <summary>
        /// Количество блоков которым нужен тик
        /// </summary>
        private int _countTickBlock;

        public ChunkStorage(ulong keyCash, int index)
        {
            //KeyCash = y + (x % 2 == 0 ? 16777216 : 0) + (z % 2 == 0 ? 33554432 : 0);
            KeyCash = keyCash;
            Index = index;
            YBase = index << 4;
            Data = null;
            CountBlock = 0;
            _countTickBlock = 0;
            LightBlock = new byte[4096];
            LightSky = new byte[4096];
        }

        /// <summary>
        /// Пустой, все блоки воздуха
        /// </summary>
        public bool IsEmptyData() => CountBlock == 0;

        /// <summary>
        /// Перепроверить количество блоков
        /// </summary>
        public void UpCountBlock()
        {
            CountBlock = 0;
            if (Data != null)
            {
                for (int i = 0; i < 4096; i++)
                {
                    if ((Data[i] & 0xFFF) != 0) CountBlock++;
                }
            }
        }

        /// <summary>
        /// Очистить без света
        /// </summary>
        public void ClearNotLight()
        {
            Data = null;
            //LightBlock = new byte[4096];
            //LightSky = new byte[4096];
            CountBlock = 0;
            _countTickBlock = 0;
        }

        /// <summary>
        /// Получить блок данных, XYZ 0..15 
        /// </summary>
        public BlockState GetBlockState(int x, int y, int z)
        {
            ushort index = (ushort)(y << 8 | z << 4 | x);
            try
            {
                ushort value = Data[index];
                ushort id = (ushort)(value & 0xFFF);
                return new BlockState(id,
                    Ce.Blocks.BlocksMetadata[id] && Metadata.ContainsKey(index) ? Metadata[index] : (uint)0,
                    LightBlock[index], LightSky[index]);
            }
            catch (Exception ex)
            {
                Logger.Crash("ChunkStorage.GetBlockState countBlock {0} countTickBlock {1} index {2} data {3} null\r\n{4}",
                    CountBlock,
                    _countTickBlock,
                    index,
                    Data == null ? "==" : "!=",
                    ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Задать данные блока, XYZ 0..15 
        /// index = y << 8 | z << 4 | x
        /// </summary>
        public void SetData(int index, ushort id, uint met = 0)
        {
            if (id == 0)
            {
                // воздух, проверка на чистку
                if (CountBlock > 0 && (Data[index] & 0xFFF) != 0)
                {
                    CountBlock--;
                    if (Ce.Blocks.BlocksRandomTick[Data[index] & 0xFFF]) _countTickBlock--;
                    Metadata.Remove((ushort)index);
                    if (CountBlock == 0)
                    {
                        Data = null;
                        _countTickBlock = 0;
                    }
                    else Data[index] = 0;
                }
            }
            else
            {
                if (CountBlock == 0)
                {
                    Data = new ushort[4096];
                    _countTickBlock = 0;
                }
                if ((Data[index] & 0xFFF) == 0) CountBlock++;
                bool rold = Ce.Blocks.BlocksRandomTick[Data[index] & 0xFFF];
                bool rnew = Ce.Blocks.BlocksRandomTick[id];
                if (!rold && rnew) _countTickBlock++;
                else if (rold && !rnew) _countTickBlock--;
                ushort key = (ushort)index;
                if (Ce.Blocks.BlocksMetadata[id])
                {
                    Data[index] = id;
                    if (Metadata.ContainsKey(key)) Metadata[key] = met;
                    else Metadata.Add(key, met);
                }
                else
                {
                    Data[index] = (ushort)(id & 0xFFF | met << 12);
                    Metadata.Remove(key);
                }
            }
        }

        /// <summary>
        /// Заменить только мет данные блока
        /// </summary>
        /// <param name="index">y << 8 | z << 4 | x</param>
        public void NewMetBlock(int index, ushort met)
        {
            if (CountBlock > 0)
            {
                int id = Data[index] & 0xFFF;
                ushort key = (ushort)index;
                if (Ce.Blocks.BlocksMetadata[id])
                {
                    if (Metadata.ContainsKey(key)) Metadata[key] = met;
                    else Metadata.Add(key, met);
                }
                else
                {
                    Data[index] = (ushort)(id & 0xFFF | met << 12);
                    Metadata.Remove(key);
                }
            }
        }

        /// <summary>
        /// Имеются ли блоки которым нужен случайный тик
        /// </summary>
        public bool GetNeedsRandomTick() => _countTickBlock > 0;

        /// <summary>
        /// Вернуть количество блоков не воздуха и количество тикающих блоков
        /// </summary>
        public string ToStringCount() => CountBlock + "|" + _countTickBlock;

        public override string ToString() => " yB:" + YBase + " body:" + CountBlock + " ";
    }
}
