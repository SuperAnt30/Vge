using System.Net.Sockets;

namespace Vge.Network
{
    /// <summary>
    /// Объект для рабочего сокета
    /// </summary>
    public class SocketWork
    {
        /// <summary>
        /// Получить рабочий сокет
        /// </summary>
        public Socket WorkSocket { get; protected set; } = null;

        protected SocketWork() { }
        public SocketWork(Socket workSocket) => WorkSocket = workSocket;

        /// <summary>
        /// Получить истину есть ли соединение
        /// </summary>
        public virtual bool IsConnected => WorkSocket != null ? WorkSocket.Connected : false;

        public override string ToString() => WorkSocket.RemoteEndPoint.ToString();
    }
}
