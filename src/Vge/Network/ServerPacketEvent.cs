using System.Net.Sockets;

namespace Vge.Network
{
    /// <summary>
    /// Создания делегата для ServerPacket
    /// </summary>
    public delegate void ServerPacketEventHandler(object sender, ServerPacketEventArgs e);

    /// <summary>
    /// Объект для события серверпакета
    /// </summary>
    public class ServerPacketEventArgs
    {
        /// <summary>
        /// Получить объект сервера пинг
        /// </summary>
        public ServerPacket Packet { get; private set; }

        public ServerPacketEventArgs(ServerPacket sp) => Packet = sp;
   }
}
