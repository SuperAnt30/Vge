using System.Net.Sockets;
using System.Text;

namespace Vge.Network
{
    /// <summary>
    /// Объект сервера пинга
    /// </summary>
    public struct ServerPacket
    {
        /// <summary>
        /// Получить рабочий сокет
        /// </summary>
        public readonly Socket workSocket;
        /// <summary>
        /// Получить статус запроса
        /// </summary>
        public readonly StatusNet status;
        /// <summary>
        /// Получить сетевой размер 
        /// </summary>
        public readonly int sizeNet;
        /// <summary>
        /// Получить массив байтов
        /// </summary>
        public readonly byte[] bytes;
        
        public ServerPacket(Socket workSocket, StatusNet status)
        {
            this.workSocket = workSocket;
            this.status = status;
            bytes = new byte[0];
            sizeNet = 0;
        }

        public ServerPacket(Socket workSocket, byte[] bytes, int sizeNet)
        {
            this.workSocket = workSocket;
            status = StatusNet.Receive;
            this.bytes = bytes;
            this.sizeNet = sizeNet;
        }

        public ServerPacket(Socket workSocket, byte[] bytes) 
            : this(workSocket, bytes, bytes.Length) { }

        /// <summary>
        /// Получить истину пустого массива байт
        /// </summary>
        public bool BytesIsEmpty => bytes.Length == 0;
        /// <summary>
        /// Получить конечный размер 
        /// </summary>
        public int SizeOriginal => bytes.Length;
        /// <summary>
        /// Получить истину есть ли соединение
        /// </summary>
        public bool IsConnected => workSocket != null ? workSocket.Connected : false;

        /// <summary>
        /// Получить заголовок сетевого потока
        /// </summary>
        public int Handle() => workSocket != null ? workSocket.Handle.ToInt32() : 0;

        /// <summary>
        /// Получить строку конвектированного массива байт из UTF8
        /// </summary>
        public string BytesToString()
        {
            if (!BytesIsEmpty)
            {
                return Encoding.UTF8.GetString(bytes);
            }
            return "";
        }

        /// <summary>
        /// Получить статус запроса в виде строки
        /// </summary>
        public string StatusToString() => status.ToString();
    }
}
