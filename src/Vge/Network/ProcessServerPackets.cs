using Vge.Games;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.Util;

namespace Vge.Network
{
    /// <summary>
    /// Обработка серверных пакетов для клиента
    /// </summary>
    public class ProcessServerPackets
    {
        /// <summary>
        /// Структура сокета и буфера пакета
        /// </summary>
        private struct SocketBuffer
        {
            public readonly SocketSide Side;
            public readonly byte[] Buffer;

            public SocketBuffer(SocketSide side, byte[] buffer)
            {
                Side = side;
                Buffer = buffer;
            }
        }

        /// <summary>
        /// Основной сервер
        /// </summary>
        private readonly Server server;
        /// <summary>
        /// Объект который из буфера данных склеивает пакеты для локального игрока
        /// </summary>
        private readonly ReadPacket readPacketLocal = new ReadPacket();
        /// <summary>
        /// Объект который из буфера данных склеивает пакеты в Update
        /// </summary>
        private readonly ReadPacket readPacketUp = new ReadPacket();
        /// <summary>
        /// Массив очередей пакетов
        /// </summary>
        private readonly DoubleList<SocketBuffer> packets = new DoubleList<SocketBuffer>();
        /// <summary>
        /// Задержка времени для перепинговки всех игроков
        /// </summary>
        private long lastSentPingPacket;
        /// <summary>
        /// Зафиксированное время для проверки пинга, у игроках
        /// </summary>
        private long lastPingTime;
        /// <summary>
        /// Зафиксированное урезанное время для проверки пинга, у игроках
        /// </summary>
        private uint pingKeySend;

        public ProcessServerPackets(Server server) => this.server = server;

        /// <summary>
        /// Передача данных для сервера
        /// </summary>
        public void ReceiveBuffer(SocketSide socketSide, byte[] buffer)
        {
            ReadPacket readPacket = socketSide == null ? readPacketLocal : socketSide.Read;
            switch (buffer[0])
            {
                case 0x00:
                    Handle00Ping(socketSide, (PacketC00Ping)readPacket.Receive(buffer, new PacketC00Ping()));
                    break;
                case 0x01:
                    Handle01KeepAlive(socketSide, (PacketC01KeepAlive)readPacket.Receive(buffer, new PacketC01KeepAlive()));
                    break;
                default:
                    // Мир есть, заносим в пакет с двойным буфером, для обработки в такте
                    packets.Add(new SocketBuffer(socketSide, buffer));
                    break;
            }
        }

        /// <summary>
        /// Передача данных для сервера в последовотельности игрового такта
        /// </summary>
        private void UpdateReceivePacket(SocketBuffer sb)
        {
            if (sb.Side != null && !sb.Side.IsConnect())
            {
                // Связь с сетевым игроком разорвана, не зачем обрабатывать пакет
                return;
            }
            try
            {
                switch (sb.Buffer[0])
                {
                    case 0x04:
                        Handle04PlayerPosition(sb.Side, (PacketC04PlayerPosition)readPacketUp.Receive(sb.Buffer, new PacketC04PlayerPosition()));
                        break;
                }
            }
            catch
            {
                // сюда можем попасть если сокет закрылся в другом потоке
            }
        }

        /// <summary>
        /// Такт сервера
        /// </summary>
        public void Update()
        {
        //    if (server.TickCounter - lastSentPingPacket > 200)
            {
                // Раз в 10 секунд
                lastSentPingPacket = server.TickCounter;
                lastPingTime = server.Time();
                pingKeySend = (uint)lastPingTime;
                server.ResponsePacketAll(new PacketS01KeepAlive(pingKeySend));
            }
            packets.Step();
            int count = packets.CountBackward;
            for (int i = 0; i < count; i++)
            {
                UpdateReceivePacket(packets.GetNext());
            }
        }

        /// <summary>
        /// Очистить пакеты в двойной буферизации
        /// </summary>
        public void Clear() => packets.Clear();

        /// <summary>
        /// Ping-pong
        /// </summary>
        private void Handle00Ping(SocketSide socketSide, PacketC00Ping packet)
            => server.ResponsePacket(socketSide, new PacketS00Pong(packet.GetClientTime()));

        /// <summary>
        /// KeepAlive
        /// </summary>
        private void Handle01KeepAlive(SocketSide socketSide, PacketC01KeepAlive packet)
        {
            //EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayerSocket(socket);
            //if (packet.GetTime() == pingKeySend && entityPlayer != null)
            //{
            //    entityPlayer.SetPing(lastPingTime);
            //}
        }

        /// <summary>
        /// Пакет позиции игрока
        /// </summary>
        private void Handle04PlayerPosition(SocketSide socketSide, PacketC04PlayerPosition packet)
        {
            server.debugText = packet.GetKey();
            //EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayerSocket(socket);
            //if (entityPlayer != null)
            //{
            //    entityPlayer.SetSneakingSprinting(packet.IsSneaking(), packet.IsSprinting());
            //    entityPlayer.SetPosition(packet.GetPos());
            //    entityPlayer.SetOnGroundServer(packet.OnGround());
            //    entityPlayer.MarkPlayerActive();
            //}
        }
    }
}
