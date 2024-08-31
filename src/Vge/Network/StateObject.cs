using System.Net.Sockets;

namespace Vge.Network
{
    /// <summary>
    /// Объект для работы с пакетами сокета
    /// </summary>
    public class StateObject 
    {
        /// <summary>
        /// Получить истину есть ли соединение
        /// </summary>
        public virtual bool IsConnected => WorkSocket != null ? WorkSocket.Connected : false;

        /// <summary>
        /// Получить рабочий сокет
        /// </summary>
        public Socket WorkSocket { get; protected set; } = null;


        public StateObject(Socket workSocket) => WorkSocket = workSocket;

        public override string ToString()
        {
            return WorkSocket.RemoteEndPoint.ToString();
        }

        /// <summary>
        /// Размер получаемого буфера
        /// </summary>
        public const int BufferSize = 1024;

        /// <summary>
        /// Получить или задать получаемый буфер
        /// </summary>
        public byte[] Buffer { get; set; } = new byte[BufferSize];
    }
}
