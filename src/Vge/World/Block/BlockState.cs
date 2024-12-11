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
        /// ID блока
        /// </summary>
        public ushort Id;
        /// <summary>
        /// Дополнительные параметры блока если имеются (32 бита)
        /// </summary>
        public uint Met;
        /// <summary>
        /// Освещение блочное, 4 bit используется
        /// </summary>
        public byte LightBlock;
        /// <summary>
        /// Освещение небесное, 4 bit используется
        /// </summary>
        public byte LightSky;

        public BlockState(ushort id, uint met = 0, byte lightBlock = 0, byte lightSky = 0)
        {
            Id = id;
            Met = met;
            LightBlock = lightBlock;
            LightSky = lightSky;
        }

        /// <summary>
        /// Блок воздуха
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAir() => Id == 0;
        /// <summary>
        /// Пустой ли объект
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty() => Id == 0 && Met == 1;

        /// <summary>
        /// Получить кэш блока
        /// </summary>
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
        public BlockState NewMet(ushort met) => new BlockState(Id, met, LightBlock, LightSky);

        /// <summary>
        /// Записать блок в буффер пакета
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteStream(WritePacket stream)
        {
            stream.UShort(Id);
            stream.UInt(Met);
            stream.Byte((byte)(LightBlock << 4 | LightSky & 0xF));
        }

        /// <summary>
        /// Прочесть блок из буффер пакета и занести в чанк
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadStream(ReadPacket stream)
        {
            Id = stream.UShort();
            Met = stream.UInt();
            byte light = stream.Byte();
            LightBlock = (byte)(light >> 4);
            LightSky = (byte)(light & 0xF);
        }

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

        public override int GetHashCode() => Id ^ Met.GetHashCode() ^ LightBlock ^ LightSky;

        public string ToInfo() => Id + " " + Ce.Blocks.BlockAlias[Id] + " M:" + Met;

        public override string ToString() => "#" + Id + " M:" + Met;
    }
}
