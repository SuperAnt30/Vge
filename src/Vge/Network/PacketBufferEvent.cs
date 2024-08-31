namespace Vge.Network
{
    /// <summary>
    /// Создания делегата для PacketBuffer
    /// </summary>
    public delegate void PacketBufferEventHandler(object sender, PacketBufferEventArgs e);

    /// <summary>
    /// Объект для события PacketBuffer
    /// </summary>
    public class PacketBufferEventArgs
    {
        public PacketBuffer Buffer { get; private set; }
        public SocketSide Side { get; private set; }

        public PacketBufferEventArgs(PacketBuffer packetBuffer, SocketSide socketSide)
        {
            Buffer = packetBuffer;
            Side = socketSide;
        }
   }
}
