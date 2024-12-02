using Vge.World.Block;

namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Пакет Игрок копает / ломает
    /// </summary>
    public struct PacketC07PlayerDigging : IPacket
    {
        public byte Id => 0x07;

        public BlockPos BlockPosition { get; private set; }
        public EnumDigging Digging { get; private set; }

        public PacketC07PlayerDigging(BlockPos blockPos, EnumDigging digging)
        {
            BlockPosition = blockPos;
            Digging = digging;
        }

        public void ReadPacket(ReadPacket stream)
        {
            BlockPosition = new BlockPos(stream.Int(), stream.Int(), stream.Int());
            Digging = (EnumDigging)stream.Byte();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(BlockPosition.X);
            stream.Int(BlockPosition.Y);
            stream.Int(BlockPosition.Z);
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
