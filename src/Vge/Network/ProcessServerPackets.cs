using Vge.Entity;
using Vge.Entity.Player;
using Vge.Games;
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
        /// Класс для очередей пакетов
        /// </summary>
        private struct SocketPacket
        {
            public SocketSide Side;
            public IPacket Packet;

            public SocketPacket(SocketSide side, IPacket packet)
            {
                Side = side;
                Packet = packet;
            }
        }

        /// <summary>
        /// Основной сервер
        /// </summary>
        private readonly GameServer _server;
        /// <summary>
        /// Массив очередей пакетов
        /// </summary>
        private readonly DoubleList<SocketPacket> _packets = new DoubleList<SocketPacket>();
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
            ReadPacket readPacket = new ReadPacket();
            readPacket.SetBuffer(buffer);
            _ReceivePacket(socketSide, readPacket.Receive(PacketsInit.InitClient(buffer[0])));
        }

        private void _ReceivePacket(SocketSide socketSide, IPacket packet)
        {
            byte index = packet.Id;
            if (index < 2)
            {
                if (index == 0)
                {
                    _Handle00Ping(socketSide, (Packet00PingPong)packet);
                }
                else if (index == 1)
                {
                    _Handle01KeepAlive(socketSide, (Packet01KeepAlive)packet);
                }
            }
            else
            {
                // Мир есть, заносим в пакет с двойным буфером, для обработки в такте
                _packets.Add(new SocketPacket(socketSide, packet));
            }
        }

        /// <summary>
        /// Передача данных для сервера в последовотельности игрового такта
        /// </summary>
        private void _UpdateReceivePacket(SocketPacket sp)
        {
            if (sp.Side != null && !sp.Side.IsConnect())
            {
                // Связь с сетевым игроком разорвана, не зачем обрабатывать пакет
                return;
            }
            try
            {
                // Объект который из буфера данных склеивает пакеты
                switch (sp.Packet.Id)
                {
                    case 0x02: _Handle02LoginStart(sp.Side, (PacketC02LoginStart)sp.Packet); break;
                    case 0x03: _Handle03UseEntity(sp.Side, (PacketC03UseEntity)sp.Packet); break;
                    case 0x04: _Handle04PlayerPosition(sp.Side, (PacketC04PlayerPosition)sp.Packet); break;
                    case 0x07: _Handle07PlayerDigging(sp.Side, (PacketC07PlayerDigging)sp.Packet); break;
                    case 0x08: _Handle08PlayerBlockPlacement(sp.Side, (PacketC08PlayerBlockPlacement)sp.Packet); break;
                    case 0x09: _Handle09HeldItemChange(sp.Side, (PacketC09HeldItemChange)sp.Packet); break;
                    case 0x0A: _Handle0APlayerAnimation(sp.Side, (PacketC0APlayerAnimation)sp.Packet); break;
                    case 0x0E: _Handle0EClickWindow(sp.Side, (PacketC0EClickWindow)sp.Packet); break;
                    case 0x14: _Handle14Message(sp.Side, (PacketC14Message)sp.Packet); break;
                    case 0x15: _Handle15PlayerSetting(sp.Side, (PacketC15PlayerSetting)sp.Packet); break;
                    case 0x20: _Handle20AcknowledgeChunks(sp.Side, (PacketC20AcknowledgeChunks)sp.Packet); break;
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
            if (_server.TickCounterGlobal % 150 == 0)
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
                    _UpdateReceivePacket(_packets.GetNext());
                }
            }
        }

        /// <summary>
        /// Очистить пакеты в двойной буферизации
        /// </summary>
        public void Clear() => _packets.Clear();

        #region Handles

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
            if (_pingKeySend == packet.Time)
            {
                PlayerServer playerServer = _server.Players.FindPlayerBySocket(socketSide);
                playerServer?.SetPing(_lastPingTime);
            }
        }

        /// <summary>
        /// Пакет проверки логина
        /// </summary>
        private void _Handle02LoginStart(SocketSide socketSide, PacketC02LoginStart packet)
            => _server.Players.LoginStart(socketSide, packet);

        /// <summary>
        /// Пакет взаимодействие с сущностью
        /// </summary>
        private void _Handle03UseEntity(SocketSide socketSide, PacketC03UseEntity packet)
        {
            PlayerServer playerServer = _server.Players.FindPlayerBySocket(socketSide);
            if (playerServer != null)
            {
                EntityBase entity = playerServer.GetWorld().LoadedEntityList.Get(packet.Index);
                if (entity != null)
                {
                    if (packet.Action == PacketC03UseEntity.EnumAction.Inpulse)
                    {
                        entity.SetPhysicsImpulse(packet.X, packet.Y, packet.Z);
                    }
                    else if (packet.Action == PacketC03UseEntity.EnumAction.Awaken)
                    {
                        entity.AwakenPhysicSleep();
                    }
                    else if (packet.Action == PacketC03UseEntity.EnumAction.Interact)
                    {
                        entity.OnInteract(playerServer);
                    }
                }
            }
        }
        
        /// <summary>
        /// Пакет позиции игрока
        /// </summary>
        private void _Handle04PlayerPosition(SocketSide socketSide, PacketC04PlayerPosition packet)
        {
            PlayerServer playerServer = _server.Players.FindPlayerBySocket(socketSide);
            playerServer?.PacketPlayerPosition(packet);
        }

        /// <summary>
        /// Пакет игрок копает / ломает
        /// </summary>
        private void _Handle07PlayerDigging(SocketSide socketSide, PacketC07PlayerDigging packet)
        {
            PlayerServer playerServer = _server.Players.FindPlayerBySocket(socketSide);
            playerServer?.PacketPlayerDigging(packet);
        }

        /// <summary>
        /// Пакет игрок устанавливает или взаимодействует с блоком
        /// </summary>
        private void _Handle08PlayerBlockPlacement(SocketSide socketSide, PacketC08PlayerBlockPlacement packet)
        {
            PlayerServer playerServer = _server.Players.FindPlayerBySocket(socketSide);
            playerServer?.PacketPlayerBlockPlacement(packet);
        }

        /// <summary>
        /// Пакет игрок отправляем на сервер выбранный слот
        /// </summary>
        private void _Handle09HeldItemChange(SocketSide socketSide, PacketC09HeldItemChange packet)
        {
            PlayerServer playerServer = _server.Players.FindPlayerBySocket(socketSide);
            playerServer?.PacketHeldItemChange(packet);
        }

        /// <summary>
        /// Пакет анимации игрока
        /// </summary>
        private void _Handle0APlayerAnimation(SocketSide socketSide, PacketC0APlayerAnimation packet)
        {
            PlayerServer playerServer = _server.Players.FindPlayerBySocket(socketSide);
            playerServer?.PacketPlayerAnimation(packet);
        }

        /// <summary>
        /// Пакет кликов по окну и контролам
        /// </summary>
        private void _Handle0EClickWindow(SocketSide socketSide, PacketC0EClickWindow packet)
        {
            PlayerServer playerServer = _server.Players.FindPlayerBySocket(socketSide);
            playerServer?.PacketClickWindow(packet);
        }

        /// <summary>
        /// Пакет передачии сообщения или команды
        /// </summary>
        private void _Handle14Message(SocketSide socketSide, PacketC14Message packet)
            => _server.Players.ClientMessage(_server.Players.FindPlayerBySocket(socketSide), packet);

        /// <summary>
        /// Пакет настроек клиента
        /// </summary>
        private void _Handle15PlayerSetting(SocketSide socketSide, PacketC15PlayerSetting packet)
        {
            PlayerServer playerServer = _server.Players.FindPlayerBySocket(socketSide);
            playerServer?.PacketPlayerSetting(packet);
        }

        /// <summary>
        /// Пакет подтверждение фрагментов
        /// </summary>
        private void _Handle20AcknowledgeChunks(SocketSide socketSide, PacketC20AcknowledgeChunks packet)
        {
            PlayerServer playerServer = _server.Players.FindPlayerBySocket(socketSide);
            playerServer?.PacketAcknowledgeChunks(packet);
        }

        #endregion
    }
}
