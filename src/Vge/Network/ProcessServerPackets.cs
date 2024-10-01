using Vge.Games;
using Vge.Management;
using Vge.Network.Packets;
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
            if (buffer[0] < 2)
            {
                // Объект который из буфера данных склеивает пакеты
                ReadPacket readPacket = new ReadPacket();
                switch (buffer[0])
                {
                    case 0x00:
                        Handle00Ping(socketSide, (Packet00PingPong)readPacket.Receive(buffer, new Packet00PingPong()));
                        break;
                    case 0x01:
                        Handle01KeepAlive(socketSide, (Packet01KeepAlive)readPacket.Receive(buffer, new Packet01KeepAlive()));
                        break;
                    default:
                        // Мир есть, заносим в пакет с двойным буфером, для обработки в такте
                        packets.Add(new SocketBuffer(socketSide, buffer));
                        break;
                }
            }
            else
            {
                // Мир есть, заносим в пакет с двойным буфером, для обработки в такте
                packets.Add(new SocketBuffer(socketSide, buffer));
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
                // Объект который из буфера данных склеивает пакеты
                ReadPacket readPacket = new ReadPacket();
                switch (sb.Buffer[0])
                {
                    case 0x02:
                        Handle02LoginStart(sb.Side, (PacketC02LoginStart)readPacket.Receive(sb.Buffer, new PacketC02LoginStart()));
                        break;
                    case 0x04:
                        Handle04PlayerPosition(sb.Side, (PacketC04PlayerPosition)readPacket.Receive(sb.Buffer, new PacketC04PlayerPosition()));
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
                server.Players.SendToAll(new Packet01KeepAlive(pingKeySend));
            }
            if (!packets.Empty())
            {
                packets.Step();
                int count = packets.CountBackward;
                for (int i = 0; i < count; i++)
                {
                    UpdateReceivePacket(packets.GetNext());
                }
            }
        }

        /// <summary>
        /// Очистить пакеты в двойной буферизации
        /// </summary>
        public void Clear() => packets.Clear();

        /// <summary>
        /// Ping-pong
        /// </summary>
        private void Handle00Ping(SocketSide socketSide, Packet00PingPong packet)
            => server.ResponsePacket(socketSide, packet);// new PacketS00Pong(packet.GetClientTime()));

        /// <summary>
        /// Сохранить жизнь
        /// </summary>
        private void Handle01KeepAlive(SocketSide socketSide, Packet01KeepAlive packet)
        {
            if (pingKeySend == packet.GetTime())
            {
                PlayerServer playerServer = server.Players.FindPlayerBySocket(socketSide);
                if (playerServer != null)
                {
                    playerServer.SetPing(lastPingTime);
                }
            }
        }

        /// <summary>
        /// Пакет проверки логина
        /// </summary>
        private void Handle02LoginStart(SocketSide socketSide, PacketC02LoginStart packet)
            => server.Players.LoginStart(socketSide, packet);

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
