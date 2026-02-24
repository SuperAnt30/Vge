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
        /// <summary>
        /// Процесс разрушения блока
        /// </summary>
        public byte Process { get; private set; }

        public BlockPos GetBlockPos() => _blockPos;

        public PacketC07PlayerDigging(BlockPos blockPos, byte process)
        {
            _blockPos = blockPos;
            Digging =  EnumDigging.ProcessDestroy;
            Process = process;
        }

        public PacketC07PlayerDigging(BlockPos blockPos)
        {
            _blockPos = blockPos;
            Digging = EnumDigging.Destroy;
            Process = 0;
        }

        public void ReadPacket(ReadPacket stream)
        {
            _blockPos.ReadStream(stream);
            Digging = (EnumDigging)stream.Byte();
            if (Digging == EnumDigging.ProcessDestroy)
            {
                Process = stream.Byte();
            }
        }

        public void WritePacket(WritePacket stream)
        {
            _blockPos.WriteStream(stream);
            stream.Byte((byte)Digging);
            if (Digging == EnumDigging.ProcessDestroy)
            {
                stream.Byte(Process);
            }
        }

        /// <summary>
        /// Варианты действия разрушения
        /// </summary>
        public enum EnumDigging
        {
            /// <summary>
            /// Просто удар, никуда не попадая, взмах
            /// </summary>
            None = 0,
            /// <summary>
            /// Разрушен блок
            /// </summary>
            Destroy = 1,
            /// <summary>
            /// Процесс разрушения блока
            /// </summary>
            ProcessDestroy = 2
        }
    }
}
