using Vge.Games;
using Vge.Network.Packets;
using Vge.Network.Packets.Server;
using Vge.Util;

namespace Vge.Network
{
    /// <summary>
    /// Обработка клиентсиких пакетов для сервером
    /// </summary>
    public class ProcessClientPackets
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public GameBase Game { get; private set; }

        /// <summary>
        /// Трафик в байтах
        /// </summary>
        public long Traffic { get; private set; } = 0;

        /// <summary>
        /// Массив очередей пакетов
        /// </summary>
        private readonly DoubleList<byte[]> _packets = new DoubleList<byte[]>();

        public ProcessClientPackets(GameBase client) => Game = client;

        /// <summary>
        /// Передача данных для клиента
        /// </summary>
        public void ReceiveBuffer(byte[] buffer)
        {
            Traffic += buffer.Length + Ce.SizeHeaderTCP;
            if (buffer[0] <= 2)
            {
                // Объект который из буфера данных склеивает пакеты
                ReadPacket readPacket = new ReadPacket();
                switch (buffer[0])
                {
                    case 0x00:
                        _Handle00Pong((Packet00PingPong)readPacket.Receive(buffer, new Packet00PingPong()));
                        break;
                    case 0x01:
                        _Handle01KeepAlive((Packet01KeepAlive)readPacket.Receive(buffer, new Packet01KeepAlive()));
                        break;
                    case 0x02:
                        _Handle02LoadingGame((PacketS02LoadingGame)readPacket.Receive(buffer, new PacketS02LoadingGame()));
                        break;
                }
            }
            else
            {
                // Мир есть, заносим в пакет с двойным буфером, для обработки в такте
                _packets.Add(buffer);
            }
        }

        /// <summary>
        /// Передача данных для клиента в последовотельности игрового такта
        /// </summary>
        private void _UpdateReceivePacket(byte[] buffer)
        {
            // Объект который из буфера данных склеивает пакеты
            ReadPacket readPacket = new ReadPacket();
            switch (buffer[0])
            {
                case 0x03:
                    _Handle03JoinGame((PacketS03JoinGame)readPacket.Receive(buffer, new PacketS03JoinGame()));
                    break;
                case 0x04:
                    _Handle04TimeUpdate((PacketS04TimeUpdate)readPacket.Receive(buffer, new PacketS04TimeUpdate()));
                    break;
                case 0x08:
                    _Handle08PlayerPosLook((PacketS08PlayerPosLook)readPacket.Receive(buffer, new PacketS08PlayerPosLook()));
                    break;
                case 0x20:
                    _Handle20ChunkSend((PacketS20ChunkSend)readPacket.Receive(buffer, new PacketS20ChunkSend()));
                    break;
                case 0x21:
                    _Handle21ChunkData((PacketS21ChunkData)readPacket.Receive(buffer, new PacketS21ChunkData()));
                    break;
            }
        }

        /// <summary>
        /// Игровой такт клиента
        /// </summary>
        public void Update()
        {
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
        /// Пакет связи
        /// </summary>
        private void _Handle00Pong(Packet00PingPong packet) 
            => Game.Player.SetPing(packet.GetClientTime());

        /// <summary>
        /// KeepAlive
        /// </summary>
        private void _Handle01KeepAlive(Packet01KeepAlive packet)
            => Game.TrancivePacket(packet);

        /// <summary>
        /// Загрузка игры
        /// </summary>
        private void _Handle02LoadingGame(PacketS02LoadingGame packet)
            => Game.PacketLoadingGame(packet);

        /// <summary>
        /// Пакет соединения игрока с сервером
        /// </summary>
        private void _Handle03JoinGame(PacketS03JoinGame packet)
            => Game.PlayerOnTheServer(packet.GetIndex(), packet.GetUuid());
        
        /// <summary>
        /// Пакет синхронизации времени с сервером
        /// </summary>
        private void _Handle04TimeUpdate(PacketS04TimeUpdate packet)
        {
            Game.SetTickCounter(packet.GetTime());
        }

        /// <summary>
        /// Пакет расположения игрока, при старте, телепорт, рестарте и тп
        /// </summary>
        private void _Handle08PlayerPosLook(PacketS08PlayerPosLook packet)
        {
            Game.Player.chPos = new WinGL.Util.Vector2i((int)packet.GetPos().X, (int)packet.GetPos().Y);
            Debug.Player = Game.Player.chPos;
        }

        /// <summary>
        /// Замер скорости закачки чанков
        /// </summary>
        private void _Handle20ChunkSend(PacketS20ChunkSend packet)
            => Game.Player.PacketChunckSend(packet);

        /// <summary>
        /// Пакет изменённые псевдо чанки
        /// </summary>
        private void _Handle21ChunkData(PacketS21ChunkData packet)
            => Game.World.ChunkPrClient.PacketChunckData(packet);

        #endregion
    }
}
