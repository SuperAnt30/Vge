namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Отправляем чанки, для проверки скорости
    /// </summary>
    public struct PacketS20ChunkSend : IPacket
    {
        public byte Id => 0x20;

        /// <summary>
        /// Начальный флаг, перед отправкой чанков, false по окончанию отправки
        /// </summary>
        public bool Start { get; private set; }
        /// <summary>
        /// Количество чанков отправлено
        /// </summary>
        public byte Quantity { get; private set; }

        /// <summary>
        /// Поставить проверку на загрузку начальная и финишная
        /// </summary>
        public PacketS20ChunkSend(byte quantity = 0)
        {
            Quantity = quantity;
            Start = quantity == 0;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Quantity = stream.Byte();
            Start = Quantity == 0;
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Byte(Quantity);
        }
    }
}
