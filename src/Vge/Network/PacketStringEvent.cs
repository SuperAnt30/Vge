namespace Vge.Network
{
    /// <summary>
    /// Создания делегата для PacketBuffer
    /// </summary>
    public delegate void PacketStringEventHandler(object sender, PacketStringEventArgs e);

    /// <summary>
    /// Объект для события строки от сетевого пакета
    /// </summary>
    public class PacketStringEventArgs
    {
        /// <summary>
        /// Причина
        /// </summary>
        public string Cause { get; private set; }
        public SocketSide Side { get; private set; }

        public PacketStringEventArgs(SocketSide socketSide, string cause = "")
        {
            Cause = cause;
            Side = socketSide;
        }
   }
}
