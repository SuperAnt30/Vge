using Vge.Util;
using Vge.World.Block;
using WinGL.Util;

namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Отправляем на сервер установку или взаимодействия с блоком
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
        /// Может на этот блок поставить другой, к примеру трава
        /// </summary>
        public bool Replaceable { get; private set; }
        /// <summary>
        /// Поставить блок
        /// </summary>
        public bool Put { get; private set; }

        private BlockPos _blockPos;
        /// <summary>
        /// Позиция блока где устанавливаем блок
        /// </summary>
        public BlockPos GetBlockPos() => _blockPos;

        /// <summary>
        /// Установить блок
        /// </summary>
        public PacketC08PlayerBlockPlacement(BlockPos blockPos, Pole side, Vector3 facing,
            bool replaceable, bool put)
        {
            _blockPos = blockPos;
            Side = side;
            Facing = facing;
            Replaceable = replaceable;
            Put = put;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Put = stream.Bool();
            Replaceable = stream.Bool();
            _blockPos.ReadStream(stream);
            Side = (Pole)stream.Byte();
            Facing = new Vector3(stream.Byte() / 16f, stream.Byte() / 16f, stream.Byte() / 16f);
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Bool(Put);
            stream.Bool(Replaceable);
            _blockPos.WriteStream(stream);
            stream.Byte((byte)Side);
            stream.Byte((byte)(Facing.X * 16f));
            stream.Byte((byte)(Facing.Y * 16f));
            stream.Byte((byte)(Facing.Z * 16f));
        }
    }
}
