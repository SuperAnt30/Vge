using System.Net.Sockets;
using Vge.Games;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.Util;

namespace Vge.Network
{
    /// <summary>
    /// Обработка серверных пакетов для клиента
    /// </summary>
    public class ProcessServerPackets : ProcessPackets
    {
        /// <summary>
        /// Основной сервер
        /// </summary>
        public Server ServerMain { get; protected set; }

        private long networkTickCount;
        private long lastPingTime;
        private long lastSentPingPacket;
        private uint pingKeySend;
        /// <summary>
        /// Массив очередей пакетов
        /// </summary>
        private DoubleList<SocketPacket> packets = new DoubleList<SocketPacket>();
        /// <summary>
        /// Класс для очередей пакетов
        /// </summary>
        private struct SocketPacket
        {
            public Socket socket;
            public IPacket packet;
        }

        public ProcessServerPackets(Server server) : base(false) => ServerMain = server;

        /// <summary>
        /// Передача данных для сервера
        /// </summary>
        public void ReceiveBuffer(Socket socket, byte[] buffer) => ReceivePacket(socket, buffer);

        protected override void ReceivePacketServer(Socket socket, IPacket packet)
        {
            switch (GetId(packet))
            {
                case 0x00: Handle00Ping(socket, (PacketC00Ping)packet); break;
                case 0x01: Handle01KeepAlive(socket, (PacketC01KeepAlive)packet); break;
                // Мира ещё нет, он в стадии создании, первый старт первого игрока
               // case 0x02: Handle02LoginStart(socket, (PacketC02LoginStart)packet); break;
                default:
                    // Мир есть, заносим в пакет с двойным буфером, для обработки в такте
                    packets.Add(new SocketPacket() { socket = socket, packet = packet });
                    break;

            }
        }

        private void UpdateReceivePacketServer(Socket socket, IPacket packet)
        {
            switch (GetId(packet))
            {
                //case 0x03: Handle03UseEntity(socket, (PacketC03UseEntity)packet); break;
            }
        }

        public void Clear() => packets.Clear();

        /// <summary>
        /// Такт сервера
        /// </summary>
        public void Update()
        {
            networkTickCount++;
            if (networkTickCount - lastSentPingPacket > 40)
            {
                lastSentPingPacket = networkTickCount;
                lastPingTime = ServerMain.Time();
                pingKeySend = (uint)lastPingTime;
                ServerMain.ResponsePacketAll(new PacketS01KeepAlive(pingKeySend));
            }
            packets.Step();
            SocketPacket socketPacket;
            int count = packets.CountBackward;
            for (int i = 0; i < count; i++)
            {
                socketPacket = packets.GetNext();
                if (socketPacket.socket.Connected)
                {
                    UpdateReceivePacketServer(socketPacket.socket, socketPacket.packet);
                }
            }
        }

        /// <summary>
        /// Ping-pong
        /// </summary>
        private void Handle00Ping(Socket socket, PacketC00Ping packet)
            => ServerMain.ResponsePacket(socket, new PacketS00Pong(packet.GetClientTime()));

        /// <summary>
        /// KeepAlive
        /// </summary>
        private void Handle01KeepAlive(Socket socket, PacketC01KeepAlive packet)
        {
            //EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayerSocket(socket);
            //if (packet.GetTime() == pingKeySend && entityPlayer != null)
            //{
            //    entityPlayer.SetPing(lastPingTime);
            //}
        }
    }
}
