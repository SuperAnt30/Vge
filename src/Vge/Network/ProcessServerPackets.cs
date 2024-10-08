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
        private readonly GameServer _server;
        /// <summary>
        /// Массив очередей пакетов
        /// </summary>
        private readonly DoubleList<SocketBuffer> _packets = new DoubleList<SocketBuffer>();
        /// <summary>
        /// Зафиксированное время для проверки пинга, у игроках
        /// </summary>
        private long _lastPingTime;
        /// <summary>
        /// Зафиксированное урезанное время для проверки пинга, у игроках
        /// </summary>
        private uint _pingKeySend;

        public ProcessServerPackets(GameServer server) => _server = server;

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
                        _Handle00Ping(socketSide, (Packet00PingPong)readPacket.Receive(buffer, new Packet00PingPong()));
                        break;
                    case 0x01:
                        _Handle01KeepAlive(socketSide, (Packet01KeepAlive)readPacket.Receive(buffer, new Packet01KeepAlive()));
                        break;
                    default:
                        // Мир есть, заносим в пакет с двойным буфером, для обработки в такте
                        _packets.Add(new SocketBuffer(socketSide, buffer));
                        break;
                }
            }
            else
            {
                // Мир есть, заносим в пакет с двойным буфером, для обработки в такте
                _packets.Add(new SocketBuffer(socketSide, buffer));
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
                        _Handle02LoginStart(sb.Side, (PacketC02LoginStart)readPacket.Receive(sb.Buffer, new PacketC02LoginStart()));
                        break;
                    case 0x04:
                        _Handle04PlayerPosition(sb.Side, (PacketC04PlayerPosition)readPacket.Receive(sb.Buffer, new PacketC04PlayerPosition()));
                        break;
                    case 0x15:
                        _Handle15PlayerSetting(sb.Side, (PacketC15PlayerSetting)readPacket.Receive(sb.Buffer, new PacketC15PlayerSetting()));
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
            if (_server.TickCounter % 150 == 0)
            {
                // Раз в 5 секунд
                _lastPingTime = _server.Time();
                _pingKeySend = (uint)_lastPingTime;
                _server.Players.SendToAll(new Packet01KeepAlive(_pingKeySend));
            }
            if (!_packets.Empty())
            {
                _packets.Step();
                int count = _packets.CountBackward;
                for (int i = 0; i < count; i++)
                {
                    UpdateReceivePacket(_packets.GetNext());
                }
            }
        }

        /// <summary>
        /// Очистить пакеты в двойной буферизации
        /// </summary>
        public void Clear() => _packets.Clear();

        /// <summary>
        /// Ping-pong
        /// </summary>
        private void _Handle00Ping(SocketSide socketSide, Packet00PingPong packet)
            => _server.ResponsePacket(socketSide, packet);

        /// <summary>
        /// Сохранить жизнь
        /// </summary>
        private void _Handle01KeepAlive(SocketSide socketSide, Packet01KeepAlive packet)
        {
            if (_pingKeySend == packet.GetTime())
            {
                PlayerServer playerServer = _server.Players.FindPlayerBySocket(socketSide);
                if (playerServer != null)
                {
                    playerServer.SetPing(_lastPingTime);
                }
            }
        }

        /// <summary>
        /// Пакет проверки логина
        /// </summary>
        private void _Handle02LoginStart(SocketSide socketSide, PacketC02LoginStart packet)
            => _server.Players.LoginStart(socketSide, packet);

        /// <summary>
        /// Пакет позиции игрока
        /// </summary>
        private void _Handle04PlayerPosition(SocketSide socketSide, PacketC04PlayerPosition packet)
        {
            PlayerServer playerServer = _server.Players.FindPlayerBySocket(socketSide);
            if (playerServer != null)
            {
                playerServer.chPos = new WinGL.Util.Vector2i((int)packet.GetPos().X, (int)packet.GetPos().Y);
                playerServer.isPos = true;
              //  _server.Worlds.GetWorld(0).Fragment.AddWorldAnchorChunk(playerServer.chPos.X + 25, playerServer.chPos.Y);
            }

            //EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayerSocket(socket);
            //if (entityPlayer != null)
            //{
            //    entityPlayer.SetSneakingSprinting(packet.IsSneaking(), packet.IsSprinting());
            //    entityPlayer.SetPosition(packet.GetPos());
            //    entityPlayer.SetOnGroundServer(packet.OnGround());
            //    entityPlayer.MarkPlayerActive();
            //}
        }

        /// <summary>
        /// Пакет настроек клиента
        /// </summary>
        private void _Handle15PlayerSetting(SocketSide socketSide, PacketC15PlayerSetting packet)
        {
            PlayerServer playerServer = _server.Players.FindPlayerBySocket(socketSide);
            if (playerServer != null)
            {
                playerServer.SetOverviewChunk(packet.GetOverviewChunk());
                playerServer.isPos = true;
            }
        }
           // => ServerMain.World.Players.ClientSetting(socket, packet);//, ServerMain.World.Players.);
    }
}
