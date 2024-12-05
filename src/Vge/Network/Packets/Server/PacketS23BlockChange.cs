using Vge.World;
using Vge.World.Block;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Отправляем клиенту один изменённый блок
    /// </summary>
    public struct PacketS23BlockChange : IPacket
    {
        public byte Id => 0x23;

        private BlockPos _blockPos;
        private BlockState _blockState;

        public BlockPos GetBlockPos() => _blockPos;
        public BlockState GetBlockState() => _blockState;

        public PacketS23BlockChange(WorldBase world, BlockPos blockPos)
        {
            _blockState = world.GetBlockState(blockPos);
            _blockPos = blockPos;
        }

        public void ReadPacket(ReadPacket stream)
        {
            _blockPos.ReadStream(stream);
            _blockState.ReadStream(stream);
        }

        public void WritePacket(WritePacket stream)
        {
            _blockPos.WriteStream(stream);
            _blockState.WriteStream(stream);
        }
    }
}
