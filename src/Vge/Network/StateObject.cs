using System.Net.Sockets;

namespace Vge.Network
{
    /// <summary>
    /// Объект для работы с пакетами сокета
    /// </summary>
    public class StateObject : SocketWork
    {
        /// <summary>
        /// Размер получаемого буфера
        /// </summary>
        public const int BufferSize = 1024;
        /// <summary>
        /// Получить или задать получаемый буфер
        /// </summary>
        public byte[] Buffer { get; set; } = new byte[BufferSize];

        public StateObject(Socket workSocket) : base(workSocket) { }
    }
}
