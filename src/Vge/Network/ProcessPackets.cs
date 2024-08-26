using System;
using System.IO;
using System.Net.Sockets;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;

namespace Vge.Network
{
    /// <summary>
    /// Обработка сетевых пакетов
    /// </summary>
    public abstract class ProcessPackets
    {
        private readonly bool isClient;

        protected ProcessPackets(bool client) => isClient = client;

        /// <summary>
        /// Объявление всех объектов пакетов
        /// </summary>
        /// <param name="id">Ключ пакета</param>
        /// <param name="isClient">Пакет от клиента</param>
        protected IPacket Init(byte id)
        {
            if (!isClient)
            {
                // Пакеты от клиента
                switch (id)
                {
                    case 0x00: return new PacketC00Ping();
                    case 0x01: return new PacketC01KeepAlive();

                   
                }
            }
            else
            {
                // Пакеты от сервера
                switch (id)
                {
                    case 0x00: return new PacketS00Pong();
                    case 0x01: return new PacketS01KeepAlive();

                    
                }
            }

            return null;
        }

        /// <summary>
        /// Получить id по имени объекта
        /// </summary>
        protected bool IsKey(IPacket packet, string check)
        {
           // try
            {
                string hex = packet.ToString().Substring(packet.ToString().Substring(26, 1) == "P" ? 32 : 39, 1);
                return hex == check;
            }
            //catch (Exception ex)
            //{
            //    Logger.Crach(ex);
            //    throw;
            //}
        }

        /// <summary>
        /// Получить id по имени объекта
        /// </summary>
        public static byte GetId(IPacket packet)
        {
            string hex = packet.GetType().Name.Substring(7, 2);

            //try
            {
            //    string hex = packet.ToString().Substring(packet.ToString().Substring(26, 1) == "P" ? 33 : 40, 2);
                return Convert.ToByte(hex, 16);
            }
            //catch (Exception ex)
            //{
            //    Logger.Crach(ex);
            //    throw;
            //}
        }

        /// <summary>
        /// Передача данных для клиента
        /// </summary>
        //public void ReceiveBufferClient(byte[] buffer) => ReceivePacket(null, buffer);
        ///// <summary>
        ///// Передача данных для сервера
        ///// </summary>
        //public void ReceiveBufferServer(Socket socket, byte[] buffer) => ReceivePacket(socket, buffer);

        protected void ReceivePacket(Socket socket, byte[] buffer)
        {
            IPacket packet = Init(buffer[0]);
            if (packet == null) return;
            try
            {
                using (MemoryStream readStream = new MemoryStream(buffer, 1, buffer.Length - 1))
                {
                    using (StreamBase stream = new StreamBase(readStream))
                    {
                        packet.ReadPacket(stream);
                        if (isClient)
                        {
                            ReceivePacketClient(packet, buffer.Length);
                        }
                        else
                        {
                            ReceivePacketServer(socket, packet);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Generic Exception Handler: {e}");
            }
        }

        /// <summary>
        /// Пакет получения для сервера
        /// </summary>
        /// <param name="socket">клиент</param>
        /// <param name="packet">данные пакета</param>
        protected virtual void ReceivePacketServer(Socket socket, IPacket packet) { }
        /// <summary>
        /// Пакет получения для клиента
        /// </summary>
        /// <param name="packet">данные пакета</param>
        /// <param name="light">длинна пакета</param>
        protected virtual void ReceivePacketClient(IPacket packet, int light) { }
    }
}
