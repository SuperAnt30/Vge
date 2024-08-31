using Vge.Games;
using Vge.Network.Packets.Client;
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
        /// Объект который из буфера данных склеивает пакеты
        /// </summary>
        private readonly ReadPacket readPacket = new ReadPacket();
        /// <summary>
        /// Объект который из буфера данных склеивает пакеты в Update
        /// </summary>
        private readonly ReadPacket readPacketUp = new ReadPacket();
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
            Traffic += buffer.Length;
            switch (buffer[0])
            {
                case 0x00: 
                    Handle00Pong((PacketS00Pong)readPacket.Receive(buffer, new PacketS00Pong()));
                    break;
                case 0x01:
                    Handle01KeepAlive((PacketS01KeepAlive)readPacket.Receive(buffer, new PacketS01KeepAlive()));
                    break;
                default:
                    // Мир есть, заносим в пакет с двойным буфером, для обработки в такте
                    packets.Add(buffer);
                    break;
            }
        }

        /// <summary>
        /// Передача данных для клиента в последовотельности игрового такта
        /// </summary>
        private void UpdateReceivePacket(byte[] buffer)
        {
            switch (buffer[0])
            {
                case 0x03:
                    Handle03TimeUpdate((PacketS03TimeUpdate)readPacketUp.Receive(buffer, new PacketS03TimeUpdate()));
                    break;
            }
        }

        /// <summary>
        /// Игровой такт клиента
        /// </summary>
        public void Update()
        {
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
        /// Пакет связи
        /// </summary>
        private void Handle00Pong(PacketS00Pong packet) 
            => Game.SetPing(packet.GetClientTime());

        /// <summary>
        /// KeepAlive
        /// </summary>
        private void Handle01KeepAlive(PacketS01KeepAlive packet)
        {
            Game.TrancivePacket(new PacketC01KeepAlive(packet.GetTime()));
            Game.OnTick2();
        }

        /// <summary>
        /// Пакет синхронизации времени с сервером
        /// </summary>
        private void Handle03TimeUpdate(PacketS03TimeUpdate packet)
        {
            Game.SetTickCounter(packet.GetTime());
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
