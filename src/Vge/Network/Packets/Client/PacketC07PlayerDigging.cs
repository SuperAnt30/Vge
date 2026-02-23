using Vge.World.Block;

namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Пакет Игрок копает / ломает
    /// Swing - взмах
    /// </summary>
    public struct PacketC07PlayerDigging : IPacket
    {
        public byte Id => 0x07;

        private BlockPos _blockPos;
        public EnumDigging Digging { get; private set; }

        public BlockPos GetBlockPos() => _blockPos;

        public PacketC07PlayerDigging(BlockPos blockPos, EnumDigging digging)
        {
            _blockPos = blockPos;
            Digging = digging;
        }

        public void ReadPacket(ReadPacket stream)
        {
            _blockPos.ReadStream(stream);
            Digging = (EnumDigging)stream.Byte();
        }

        public void WritePacket(WritePacket stream)
        {
            _blockPos.WriteStream(stream);
            stream.Byte((byte)Digging);
        }

        /// <summary>
        /// Варианты действия разрушения
        /// </summary>
        public enum EnumDigging
        {
            /// <summary>
            /// Начали разрушать блок
            /// </summary>
            Start = 0,
            /// <summary>
            /// Отмена разрушения блока
            /// </summary>
            About = 1,
            /// <summary>
            /// Блок разрушен
            /// </summary>
            Stop = 2,
            /// <summary>
            /// Мгновенное разрушение
            /// </summary>
            Destroy = 3,
            /// <summary>
            /// Удар перед разрушениями, и может быть в холостую
            /// </summary>
            Hit = 4
        }
    }
}
