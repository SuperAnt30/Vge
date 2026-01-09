using System.Runtime.CompilerServices;
using Vge.Network;
namespace Vge.World.Block
{
    /// <summary>
    /// Бинарные данные блока
    /// </summary>
    public struct BlockState
    {
        /// <summary>
        /// ID блока, используется только 12 bit = 0 - 4095
        /// </summary>
        public int Id;
        /// <summary>
        /// Дополнительные параметры блока 19 bit
        /// LLL mmmm MMMM MMMM MMMM
        /// L - id блока жидкости через доп массив индексов
        /// m - met данные жидкости
        /// M - met данные блока
        /// </summary>
        public int Met;
        /// <summary>
        /// Освещение блочное, 4 bit используется
        /// </summary>
        public byte LightBlock;
        /// <summary>
        /// Освещение небесное, 4 bit используется
        /// </summary>
        public byte LightSky;

        public BlockState(int idMet)
        {
            Id = idMet & 0xFFF;
            Met = idMet >> 12;
            LightBlock = 0;
            LightSky = 0;
        }

        public BlockState(int idMet, byte light)
        {
            Id = idMet & 0xFFF;
            Met = idMet >> 12;
            LightBlock = (byte)(light >> 4);
            LightSky = (byte)(light & 15);
        }

        /// <summary>
        /// Блок воздуха
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public bool IsAir() => Id == 0;
        /// <summary>
        /// Пустой ли объект
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty() => Id == 0 && Met == 1;

        /// <summary>
        /// Получить кэш блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockBase GetBlock() => Ce.Blocks.BlockObjects[Id];

        /// <summary>
        /// Пометить пустой блок
        /// </summary>
        public BlockState Empty()
        {
            // id воздуха, но мет данные 1
            Id = 0;
            Met = 1;
            LightBlock = 0;
            LightSky = 15;
            return this;
        }

        /// <summary>
        /// Веррнуть новый BlockState с новыйм мет данные
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockState NewMet(int met) => new BlockState()
        {
            Id = Id, Met = met, LightBlock = LightBlock, LightSky = LightSky
        };

        /// <summary>
        /// Записать блок в буффер пакета
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteStream(WritePacket stream)
        {
            stream.Int(Id | Met << 12);
            stream.Byte((byte)(LightBlock << 4 | LightSky & 0xF));
        }

        /// <summary>
        /// Прочесть блок из буффер пакета и занести в чанк
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadStream(ReadPacket stream)
        {
            int value = stream.Int();
            Id = value & 0xFFF;
            Met = value >> 12;
            value = stream.Byte();
            LightBlock = (byte)(value >> 4);
            LightSky = (byte)(value & 0xF);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(BlockState))
            {
                var vec = (BlockState)obj;
                if (Id == vec.Id && Met == vec.Met 
                    && LightBlock == vec.LightBlock && LightSky == vec.LightSky) return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Id ^ Met.GetHashCode() ^ LightBlock ^ LightSky;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToInfo() => GetBlock().ToInfo(Met);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => "#" + Id + " M:" + Met;
    }
}
