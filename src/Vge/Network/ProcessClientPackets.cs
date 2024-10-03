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
        private readonly DoubleList<byte[]> packets = new DoubleList<byte[]>();

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
                        Handle00Pong((Packet00PingPong)readPacket.Receive(buffer, new Packet00PingPong()));
                        break;
                    case 0x01:
                        Handle01KeepAlive((Packet01KeepAlive)readPacket.Receive(buffer, new Packet01KeepAlive()));
                        break;
                    case 0x02:
                        Handle02LoadingGame((PacketS02LoadingGame)readPacket.Receive(buffer, new PacketS02LoadingGame()));
                        break;
                }
            }
            else
            {
                // Мир есть, заносим в пакет с двойным буфером, для обработки в такте
                packets.Add(buffer);
            }
        }

        /// <summary>
        /// Передача данных для клиента в последовотельности игрового такта
        /// </summary>
        private void UpdateReceivePacket(byte[] buffer)
        {
            // Объект который из буфера данных склеивает пакеты
            ReadPacket readPacket = new ReadPacket();
            switch (buffer[0])
            {
                case 0x03:
                    Handle03JoinGame((PacketS03JoinGame)readPacket.Receive(buffer, new PacketS03JoinGame()));
                    break;
                case 0x04:
                    Handle04TimeUpdate((PacketS04TimeUpdate)readPacket.Receive(buffer, new PacketS04TimeUpdate()));
                    break;
                case 0x08:
                    Handle08PlayerPosLook((PacketS08PlayerPosLook)readPacket.Receive(buffer, new PacketS08PlayerPosLook()));
                    break;
                    
            }
        }

        /// <summary>
        /// Игровой такт клиента
        /// </summary>
        public void Update()
        {
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
        /// Пакет связи
        /// </summary>
        private void Handle00Pong(Packet00PingPong packet) 
            => Game.Player.SetPing(packet.GetClientTime());

        /// <summary>
        /// KeepAlive
        /// </summary>
        private void Handle01KeepAlive(Packet01KeepAlive packet)
            => Game.TrancivePacket(packet);

        /// <summary>
        /// Загрузка игры
        /// </summary>
        private void Handle02LoadingGame(PacketS02LoadingGame packet)
            => Game.PacketLoadingGame(packet);

        /// <summary>
        /// Пакет соединения игрока с сервером
        /// </summary>
        private void Handle03JoinGame(PacketS03JoinGame packet)
            => Game.PlayerOnTheServer(packet.GetIndex(), packet.GetUuid());
        
        /// <summary>
        /// Пакет синхронизации времени с сервером
        /// </summary>
        private void Handle04TimeUpdate(PacketS04TimeUpdate packet)
        {
            Game.SetTickCounter(packet.GetTime());
        }

        /// <summary>
        /// Пакет расположения игрока, при старте, телепорт, рестарте и тп
        /// </summary>
        private void Handle08PlayerPosLook(PacketS08PlayerPosLook packet)
        {
            Game.Player.chPos = new WinGL.Util.Vector2i((int)packet.GetPos().X, (int)packet.GetPos().Y);
            Debug.player = Game.Player.chPos;
        }

        #region ConnectionDisconnect

        ///// <summary>
        ///// Пакет соединения
        ///// </summary>
        //private void HandleF0Connection(PacketSF0Connection packet)
        //{
        //    if (packet.IsBegin())
        //    {
        //        ClientMain.BeginWorldConnect();
        //    }
        //    else if (packet.IsConnect())
        //    {
        //        // connect
        //        ClientMain.TrancivePacket(new PacketC02LoginStart(ClientMain.ToNikname(), ClientMain.IndexVersion, true));
        //    }
        //    else
        //    {
        //        // disconnect с причиной
        //        ClientMain.ExitingWorld(packet.GetCause());
        //    }

        //}


        ///// <summary>
        ///// Дисконект игрока
        ///// </summary>
        //private void HandleF1Disconnect(PacketSF1Disconnect packet)
        //{
        //    ClientMain.World.PlayerRemove(packet.GetId());
        //    ClientMain.World.RemoveEntityFromWorld(packet.GetId());
        //}

        #endregion
    }
}
