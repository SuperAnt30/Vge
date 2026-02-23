using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.NBT;
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
        /// 12 bit Id блока и 19 bit параметр блока
        /// где 28 - 30 bit отвечает за дополнительную жидкость
        /// 31 bit (отвечает за знак) не используется для простоты, чтоб не менять типы к uint 
        /// 
        /// NLLL mmmm MMMM MMMM MMMM BBBB BBBB BBBB
        /// N - не используется
        /// L - id блока жидкости через доп массив индексов
        /// m - met данные жидкости
        /// M - met данные блока
        /// B - id блока
        /// </summary>
        public int[] Data;
        /// <summary>
        /// Освещение блочное и небесное, по 4 bit
        /// Block << 4 | Sky & 0xF
        /// </summary>
        public byte[] Light;
        /// <summary>
        /// Количество блоков не воздуха
        /// </summary>
        public int CountBlock { get; private set; }
        /// <summary>
        /// Карта всех разрушающих блоков
        /// </summary>
        public Dictionary<int, byte> Destroy = new Dictionary<int, byte>();

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
            Light = new byte[4096];
        }

        /// <summary>
        /// Пустой, все блоки воздуха
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearNotLight()
        {
            Data = null;
            CountBlock = 0;
            _countTickBlock = 0;
            Destroy.Clear();
        }

        /// <summary>
        /// Получить блок данных, XYZ 0..15 
        /// </summary>
        public BlockState GetBlockState(int x, int y, int z)
        {
            int index = y << 8 | z << 4 | x;
            try
            {
                return new BlockState(Data[index], Light[index]);
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
        /// Получить блок данных, XYZ 0..15 без света
        /// </summary>
        public BlockState GetBlockStateNotLight(int xz, int y)
        {
            int index = y << 8 | xz;
            try
            {
                return new BlockState(Data[index]);
            }
            catch (Exception ex)
            {
                Logger.Crash("ChunkStorage.GetBlockStateNotLight countBlock {0} countTickBlock {1} index {2} data {3} null\r\n{4}",
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
        public void SetData(int index, int id, int met = 0)
        {
            if (id == 0)
            {
                // воздух, проверка на чистку
                if (CountBlock > 0 && (Data[index] & 0xFFF) != 0)
                {
                    CountBlock--;
                    if (Ce.Blocks.BlocksRandomTick[Data[index] & 0xFFF]) _countTickBlock--;
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
                    Data = new int[4096];
                    _countTickBlock = 0;
                }
                if ((Data[index] & 0xFFF) == 0) CountBlock++;
                bool rold = Ce.Blocks.BlocksRandomTick[Data[index] & 0xFFF];
                bool rnew = Ce.Blocks.BlocksRandomTick[id];
                if (!rold && rnew) _countTickBlock++;
                else if (rold && !rnew) _countTickBlock--;
                Data[index] = id & 0xFFF | met << 12;
            }
            Destroy.Remove(index);
        }

        /// <summary>
        /// Заменить только мет данные блока
        /// </summary>
        /// <param name="index">y << 8 | z << 4 | x</param>
        public void NewMetBlock(int index, int met)
        {
            if (CountBlock > 0)
            {
                int id = Data[index] & 0xFFF;
                Data[index] = id & 0xFFF | met << 12;
            }
        }

        /// <summary>
        /// Имеются ли блоки которым нужен случайный тик
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetNeedsRandomTick() => _countTickBlock > 0;

        #region NBT

        public void WriteDataToNBT(TagList nbt)
        {
            int i, count;
            // нету блоков
            bool emptyB = IsEmptyData();
            // нету блоков освещения блока
            bool emptyLB = true;
            // нету блоков освещения неба
            bool emptyLS = true;
            for (i = 0; i < 4096; i++)
            {
                if ((Light[i] >> 4) > 0)
                {
                    emptyLB = false;
                    break;
                }
            }
            for (i = 0; i < 4096; i++)
            {
                if ((Light[i] & 15) < 15)
                {
                    emptyLS = false;
                    break;
                }
            }

            byte[] buffer;
            TagCompound tagCompound = new TagCompound();
            
            if (!emptyB)
            {
                tagCompound.SetIntArray("BlockStates", Data);
            }
            if (!emptyLB)
            {
                buffer = new byte[2048];
                // яркость от блоков
                for (i = 0; i < 2048; i++)
                {
                    count = i * 2;
                    buffer[i] = (byte)(((Light[count] >> 4) & 0xF) | ((Light[count + 1] >> 4) << 4));
                }
                tagCompound.SetByteArray("BlockLight", buffer);
            }
            if (!emptyLS)
            {
                buffer = new byte[2048];
                // яркость от неба
                for (i = 0; i < 2048; i++)
                {
                    count = i * 2;
                    buffer[i] = (byte)((Light[count] & 0xF) | ((Light[count + 1] & 0xF) << 4));
                }
                tagCompound.SetByteArray("SkyLight", buffer);
            }

            if (!emptyB || !emptyLB || !emptyLS)
            {
                tagCompound.SetByte("Y", (byte)(YBase >> 4));
                nbt.AppendTag(tagCompound);
            }
        }

        public void ReadDataFromNBT(TagCompound nbt)
        {
            int count;
            int i;
            int[] data = nbt.GetIntArray("BlockStates");
            // нету блоков
            if (data.Length == 4096)
            {
                int idMet;
                for (i = 0; i < 4096; i++)
                {
                    idMet = data[i];
                    SetData(i, idMet & 0xFFF, idMet >> 12);
                }
            }

            Array.Clear(Light, 0, Light.Length);
            byte[] buffer = nbt.GetByteArray("BlockLight");
            // нету блоков освещения блока
            if (buffer.Length == 2048)
            {
                for (i = 0; i < 2048; i++)
                {
                    count = i * 2;
                    Light[count] = (byte)((buffer[i] & 0xF) << 4);
                    Light[count + 1] = (byte)((buffer[i] >> 4) << 4);
                }
            }
            buffer = nbt.GetByteArray("SkyLight");
            // нету блоков освещения неба
            if (buffer.Length == 2048)
            {
                for (i = 0; i < 2048; i++)
                {
                    count = i * 2;
                    Light[count] |= (byte)(buffer[i] & 0xF);
                    Light[count + 1] |= (byte)(buffer[i] >> 4);
                }
            }
            else
            {
                for (i = 0; i < 4096; i++) Light[i] |= 15;
            }
            return;
        }

        #endregion

        /// <summary>
        /// Вернуть количество блоков не воздуха, количество тикающих блоков и разрушающих блоков
        /// </summary>
        public string ToStringCount() => "СhunkCount block:" + CountBlock + " tick:" + _countTickBlock + " destroy:" + Destroy.Count;

        public override string ToString() => " yB:" + YBase + " body:" + CountBlock + " ";
    }
}
