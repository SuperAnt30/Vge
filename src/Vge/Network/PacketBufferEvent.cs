namespace Vge.Network
{
    /// <summary>
    /// Создания делегата для PacketBuffer
    /// </summary>
    public delegate void PacketBufferEventHandler(object sender, PacketBufferEventArgs e);

    /// <summary>
    /// Объект для события PacketBuffer
    /// </summary>
    public class PacketBufferEventArgs
    {
        /// <summary>
        /// Получить массив байтов
        /// </summary>
        public readonly byte[] Bytes;
        /// <summary>
        /// Количество элементов
        /// </summary>
        public readonly int Count;
        /// <summary>
        /// Объект стороны сокета
        /// </summary>
        public readonly SocketSide Side;

        public PacketBufferEventArgs(byte[] bytes, int count, SocketSide socketSide)
        {
            Bytes = bytes;
            Count = count;
            Side = socketSide;
        }

        public PacketBufferEventArgs(byte[] bytes, SocketSide socketSide)
        {
            Bytes = bytes;
            Count = bytes.Length;
            Side = socketSide;
        }

        public PacketBufferEventArgs(byte[] bytes, int count)
        {
            Bytes = bytes;
            Count = count;
            Side = null;
        }

        public PacketBufferEventArgs(byte[] bytes)
        {
            Bytes = bytes;
            Count = bytes.Length;
            Side = null;
        }
    }
}
