
using Vge.Games;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.Util;

namespace Vge.Network
{
    /// <summary>
    /// Обработка клиентсиких пакетов для сервером
    /// </summary>
    public class ProcessClientPackets : ProcessPackets
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public GameBase Game { get; private set; }
        /// <summary>
        /// Массив очередей пакетов
        /// </summary>
        private DoubleList<IPacket> packets = new DoubleList<IPacket>();

        public ProcessClientPackets(GameBase client) : base(true) => Game = client;

        /// <summary>
        /// Передача данных для клиента
        /// </summary>
        public void ReceiveBuffer(byte[] buffer) => ReceivePacket(null, buffer);

        protected override void ReceivePacketClient(IPacket packet, int light)
        {
            //Debug.Traffic += light;
            byte id = GetId(packet);
            //if (id == 0xF0)
            //{
            //    // Мира ещё нет, он в стадии создании, первый старт первого игрока
            //    HandleF0Connection((PacketSF0Connection)packet);
            //}
            //else 
            //if (ClientMain.World != null)
            {
                if (id == 0x00)
                {
                    Handle00Pong((PacketS00Pong)packet);
                }
                else if (id == 0x01)
                {
                    Handle01KeepAlive((PacketS01KeepAlive)packet);
                }
                //else if (id == 0x02)
                //{
                //    // Хоть мир уже и есть, но для первого игрока, но он ещё не запустил игровой такт
                //    Handle02JoinGame((PacketS02JoinGame)packet);
                //}
                else
                {
                    // Мир есть, заносим в пакет с двойным буфером, для обработки в такте
                    packets.Add(packet);
                }
            }
        }

        private void UpdateReceivePacketClient(IPacket packet)
        {
            byte id = GetId(packet);
            switch (id)
            {
                //case 0x03: Handle03TimeUpdate((PacketS03TimeUpdate)packet); break;

                //case 0xF1: HandleF1Disconnect((PacketSF1Disconnect)packet); break;
            }
        }

        public void Clear() => packets.Clear();

        /// <summary>
        /// Игровой такт клиента
        /// </summary>
        public void Update()
        {
            packets.Step();
            int count = packets.CountBackward;
            for (int i = 0; i < count; i++)
            {
                UpdateReceivePacketClient(packets.GetNext());
            }
        }

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
            //=> Game.TrancivePacket(new PacketC01KeepAlive(packet.GetTime()));

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
