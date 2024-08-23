using System;
using System.IO;
using System.Net.Sockets;

namespace Vge.Network
{
    public class SocketBase : SocketWork
    {
        /// <summary>
        /// Порт сервера
        /// </summary>
        public int Port { get; protected set; }

        protected SocketBase() { }
        public SocketBase(int port) => Port = port;

        /// <summary>
        /// Метод отправки пакетов запроса
        /// </summary>
        /// <param name="bytes">данные в массиве байт</param>
        /// <returns>результат отправки</returns>
        protected bool SenderOld(Socket socket, byte[] bytes)
        {
            if (!IsConnected || bytes.Length == 0)
            {
                return false;
            }
            try
            {
                // Отправляем пакет
                // TODO::2024-08-23 Net Task pool
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    socket.Send(ReceivingBytes.BytesSender(bytes));
                });
                return true;
            }
            catch (Exception e)
            {
                // Возвращаем ошибку
                OnError(new ErrorEventArgs(e));
                return false;
            }
        }
        
        /// <summary>
        /// Ответ готовности сообщения
        /// </summary>
        protected virtual void RbReceive(object sender, ServerPacketEventArgs e)
        {
            if (e.Packet.status == StatusNet.Receive)
            {
                OnReceivePacket(e);
            }
            else
            {
                OnReceive(e);
            }
        }

        #region Event

        /// <summary>
        /// Событие, ошибка
        /// </summary>
        public event ErrorEventHandler Error;
        /// <summary>
        /// Событие ошибки
        /// </summary>
        protected void OnError(ErrorEventArgs e) => Error?.Invoke(this, e);

        /// <summary>
        /// Событие, получать
        /// </summary>
        public event ServerPacketEventHandler Receive;
        /// <summary>
        /// Событие получать
        /// </summary>
        protected void OnReceive(ServerPacketEventArgs e) => Receive?.Invoke(this, e);

        /// <summary>
        /// Событие, получать пакет
        /// </summary>
        public event ServerPacketEventHandler ReceivePacket;
        /// <summary>
        /// Событие получать пакет
        /// </summary>
        protected void OnReceivePacket(ServerPacketEventArgs e) => ReceivePacket?.Invoke(this, e);

        #endregion
    }
}
