namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Подтверждение фрагментов
    /// </summary>
    public struct PacketC20AcknowledgeChunks : IPacket
    {
        public byte Id => 0x20;

        /// <summary>
        /// Количество чанков отправлено
        /// </summary>
        public byte Quantity { get; private set; }
        /// <summary>
        /// Время в мс
        /// </summary>
        public int Time { get; private set; }
        /// <summary>
        /// Загрузка ли, false - распаковка
        /// </summary>
        public bool IsLoad { get; private set; }

        public PacketC20AcknowledgeChunks(int time, byte quantity, bool isLoad)
        {
            Time = time;
            Quantity = quantity;
            IsLoad = isLoad;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Time = stream.Int();
            Quantity = stream.Byte();
            IsLoad = stream.Bool();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(Time);
            stream.Byte(Quantity);
            stream.Bool(IsLoad);
        }
    }
}
