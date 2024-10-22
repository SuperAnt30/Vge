using WinGL.Util;

namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Пакет расположения игрока
    /// </summary>
    public struct PacketC04PlayerPosition : IPacket
    {
        public byte Id => 0x04;

        public byte World { get; private set; }
        public Vector3 Position { get; private set; }
        public bool IsSneaking { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool OnGround { get; private set; }

        public PacketC04PlayerPosition(Vector3 pos, bool sneaking, bool sprinting, bool onGround, byte world)
        {
            Position = pos;
            IsSneaking = sneaking;
            IsSprinting = sprinting;
            OnGround = onGround;
            World = world;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Position = new Vector3(stream.Float(), stream.Float(), stream.Float());
            World = stream.Byte();
            //IsSneaking = stream.Bool();
            //IsSprinting = stream.Bool();
            //OnGround = stream.Bool();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Float(Position.X);
            stream.Float(Position.Y);
            stream.Float(Position.Z);
            stream.Byte(World);
            //stream.Bool(IsSneaking);
            //stream.Bool(IsSprinting);
            //stream.Bool(OnGround);
        }
    }
}
