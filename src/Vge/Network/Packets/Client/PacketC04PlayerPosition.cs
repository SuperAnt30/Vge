using Vge.Entity;
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
        public EntityPos Position { get; private set; }
        public bool IsSneaking { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool OnGround { get; private set; }

        public PacketC04PlayerPosition(EntityPos pos, 
            bool sneaking, bool sprinting, bool onGround, byte world)
        {
            Position = pos;
            IsSneaking = sneaking;
            IsSprinting = sprinting;
            OnGround = onGround;
            World = world;
        }

        public void ReadPacket(ReadPacket stream)
        {
            World = stream.Byte();
            Position = new EntityPos();
            Position.ReadPacket(stream);
            //IsSneaking = stream.Bool();
            IsSprinting = stream.Bool();
            //OnGround = stream.Bool();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Byte(World);
            Position.WritePacket(stream);
            //stream.Bool(IsSneaking);
            stream.Bool(IsSprinting);
            //stream.Bool(OnGround);
        }
    }
}
