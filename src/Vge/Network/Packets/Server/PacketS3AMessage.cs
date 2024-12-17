namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Пакет получения сообщения с сервера
    /// </summary>
    public struct PacketS3AMessage : IPacket
    {
        public byte Id => 0x3A;

        /// <summary>
        /// Сообщение
        /// </summary>
        public string Message { get; private set; }

        public PacketS3AMessage(string message)
        {
            Message = message;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Message = stream.String();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.String(Message);
        }
    }
}
