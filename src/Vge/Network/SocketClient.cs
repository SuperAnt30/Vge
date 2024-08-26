using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Vge.Network
{
    /// <summary>
    /// Объект клиента используя сокет
    /// </summary>
    public class SocketClient : SocketBase
    {
        /// <summary>
        /// IP адрес сервера
        /// </summary>
        public IPAddress Ip { get; private set; }

        /// <summary>
        /// Объект склейки
        /// </summary>
        private ReceivingBytes receivingBytes;

        public SocketClient(IPAddress ip, int port) : base(port) => Ip = ip;

        #region Runing

        /// <summary>
        /// Соединяемся к серверу
        /// </summary>
        public bool Connect()
        {
            if (!IsConnected)
            {
                try
                {
                    WorkSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    WorkSocket.Connect(Ip, Port);

                    // Соединились
                    ServerPacket sp = new ServerPacket(WorkSocket, StatusNet.Connect);
                    OnReceive(new ServerPacketEventArgs(sp));

                    receivingBytes = new ReceivingBytes(WorkSocket);
                    receivingBytes.Receive += RbReceive;

                    ReceiveBuff();

                    return true;
                }
                catch (SocketException e)
                {
                    OnError(new ErrorEventArgs(e));
                }
            }
            return false;
        }

        /// <summary>
        /// Разрываем соединение с сервером
        /// </summary>
        public void Disconnect()
        {
            if (!IsConnected)
            {
                return;
            }
            try
            {
                // Разорвали связь
                ServerPacket sp = new ServerPacket(WorkSocket, StatusNet.Disconnect);
                OnReceive(new ServerPacketEventArgs(sp));

                WorkSocket.Shutdown(SocketShutdown.Both);
                WorkSocket.Close();
                WorkSocket = null;
                receivingBytes = null;
            }
            catch (SocketException e)
            {
                OnError(new ErrorEventArgs(e));
            }
        }

        #endregion

        /// <summary>
        /// Ответ готовности сообщения
        /// </summary>
        protected override void RbReceive(object sender, ServerPacketEventArgs e)
        {
            if (e.Packet.status == StatusNet.Disconnecting)
            {
                Disconnect();
            }
            base.RbReceive(sender, e);
        }

        /// <summary>
        /// Отправить пакет
        /// </summary>
        public void SendPacket(byte[] bytes) => SenderOld(WorkSocket, bytes);

        /// <summary>
        /// Получить буфер по сети
        /// </summary>
        private void ReceiveBuff()
        {
            // Чтение данных из клиентского сокета. 
            int bytesRead = 0;
            try
            {
                bytesRead = WorkSocket.Receive(buff);
            }
            catch
            {
                // Затычка, если сеть будет разорвана
            }

            try
            {
                if (bytesRead > 0)
                {
                    // Если длинны данный больше 0, то обрабатываем данные
                    receivingBytes.Receiving(ReceivingBytes.DivisionAr(buff, 0, bytesRead));

                    // Запуск ожидание следующего ответа от клиента
                    if (WorkSocket != null && WorkSocket.Connected)
                    {
                        ReceiveBuff();
                    }
                }
                else
                {
                    // Если данные отсутствуют, то разрываем связь
                    Disconnect();
                }
            }
            catch (Exception e)
            {
                // исключение намекает на разрыв соединения
                Disconnect();
                // Возвращаем ошибку
                OnError(new ErrorEventArgs(e));
            }
        }
    }
}
