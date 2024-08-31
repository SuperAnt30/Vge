namespace Vge.Network
{
    /// <summary>
    /// Структура буфер пакета
    /// </summary>
    public struct PacketBuffer
    {
        /// <summary>
        /// Получить сетевой размер 
        /// </summary>
        public readonly int sizeNet;
        /// <summary>
        /// Получить массив байтов
        /// </summary>
        public readonly byte[] bytes;
        
        public PacketBuffer(byte[] bytes, int sizeNet)
        {
            this.bytes = bytes;
            this.sizeNet = sizeNet;
        }

        public PacketBuffer(byte[] bytes) : this(bytes, bytes.Length) { }
    }
}
