namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Подтверждение фрагментов
    /// </summary>
    public struct PacketC20AcknowledgeChunks : IPacket
    {
        public byte GetId() => 0x20;

        /// <summary>
        /// Количество чанков отправлено
        /// </summary>
        public byte Quantity { get; private set; }
        /// <summary>
        /// Время в мс
        /// </summary>
        public int Time { get; private set; }

        public PacketC20AcknowledgeChunks(int time, byte quantity)
        {
            Time = time;
            Quantity = quantity;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Time = stream.Int();
            Quantity = stream.Byte();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(Time);
            stream.Byte(Quantity);
        }
    }
}
