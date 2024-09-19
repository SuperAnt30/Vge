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
        public string Text { get; private set; }
        public SocketSide Side { get; private set; }

        public PacketStringEventArgs(SocketSide socketSide, string text = "")
        {
            Text = text;
            Side = socketSide;
        }
   }
}
