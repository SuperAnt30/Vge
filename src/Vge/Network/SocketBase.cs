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

        protected readonly byte[] buff = new byte[1024];

        protected SocketBase() { }
        public SocketBase(int port) => Port = port;

        /// <summary>
        /// Метод отправки пакетов запроса
        /// </summary>
        /// <param name="bytes">данные в массиве байт</param>
        /// <returns>результат отправки</returns>
        protected void SenderOld(Socket socket, byte[] bytes)
        {
            if (!IsConnected || bytes.Length == 0)
            {
                return;
            }
            // Отправляем пакет
            //  System.Threading.Thread.Sleep(100);
            // TODO::2024-08-23 Net Task pool
            //System.Threading.Tasks.Task.Factory.StartNew(() =>
            //{
            //    //System.Threading.Thread.Sleep(300);
            //    socket.Send(ReceivingBytes.BytesSender(bytes));
            //});
            try
            {
                //socket.Send(ReceivingBytes.BytesSender(bytes));

                byte[] ret = new byte[bytes.Length + 5];
                Buffer.BlockCopy(new byte[] { 1 }, 0, ret, 0, 1);
                Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, ret, 1, 4);
                Buffer.BlockCopy(bytes, 0, ret, 5, bytes.Length);
                socket.Send(ret);
                
            }
            catch
            {
                // Ошибку глушим, так-как разрыв сети будет отработан в другом месте,
                // а тут отправка пакета, но если был разрыв сети, пакет не уйдёт.
                // Но сделав ошибку, сломает корректность disconnection
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
