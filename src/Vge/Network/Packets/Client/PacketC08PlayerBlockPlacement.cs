using Vge.Util;
using Vge.World.Block;
using WinGL.Util;

namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Отправляем на сервер установку блока или взаимодецствие
    /// </summary>
    public struct PacketC08PlayerBlockPlacement : IPacket
    {
        public byte Id => 0x08;

        /// <summary>
        /// С какой стороны устанавливаем блок
        /// </summary>
        public Pole Side { get; private set; }
        /// <summary>
        /// Точка куда устанавливаем блок (параметр с RayCast)
        /// значение в пределах 0..1, образно фиксируем пиксел клика на стороне
        /// </summary>
        public Vector3 Facing { get; private set; }
        /// <summary>
        /// Flag: 0 - PutBlock, 1 - BlockActivated, 2 - UseItemRightClick
        /// </summary>
        //private byte flag;

        private BlockPos _blockPos;

        /// <summary>
        /// Позиция блока где устанавливаем блок
        /// </summary>
        public BlockPos GetBlockPos() => _blockPos;

        public PacketC08PlayerBlockPlacement(BlockPos blockPos, Pole side, Vector3 facing)
        {
            _blockPos = blockPos;
            Side = side;
            Facing = facing;
        }

        public void ReadPacket(ReadPacket stream)
        {
            _blockPos.ReadStream(stream);
            Side = (Pole)stream.Byte();
            Facing = new Vector3(stream.Byte() / 16f, stream.Byte() / 16f, stream.Byte() / 16f);
        }

        public void WritePacket(WritePacket stream)
        {
            _blockPos.WriteStream(stream);
            stream.Byte((byte)Side);
            stream.Byte((byte)(Facing.X * 16f));
            stream.Byte((byte)(Facing.Y * 16f));
            stream.Byte((byte)(Facing.Z * 16f));
        }
    }
}
