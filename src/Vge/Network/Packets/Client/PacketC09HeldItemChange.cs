namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Отправляем на сервер выбранный слот
    /// </summary>
    public struct PacketC09HeldItemChange : IPacket
    {
        public byte Id => 0x09;

        /// <summary>
        /// Id выбранного слота
        /// </summary>
        public byte SlotId { get; private set; }

        public PacketC09HeldItemChange(byte slotId) => SlotId = slotId;

        public void ReadPacket(ReadPacket stream) => SlotId = stream.Byte();
        public void WritePacket(WritePacket stream) => stream.Byte(SlotId);
    }
}
