using Vge.Entity;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Пакет спавна моба
    /// </summary>
    public struct PacketS0FSpawnMob : IPacket
    {
        public byte Id => 0x0F;

        public int Index { get; private set; }
        public EnumEntity Type { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        public PacketS0FSpawnMob(EntityBase entity)
        {
            Index = entity.Id;
            Type = entity.Type;
            X = entity.PosX;
            Y = entity.PosY;
            Z = entity.PosZ;
            Yaw = entity.RotationYaw;
            Pitch = entity.RotationPitch;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Index = stream.Int();
            Type = (EnumEntity)stream.Byte();
            X = stream.Float();
            Y = stream.Float();
            Z = stream.Float();
            Yaw = stream.Float();
            Pitch = stream.Float();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(Index);
            stream.Byte((byte)Type);
            stream.Float(X);
            stream.Float(Y);
            stream.Float(Z);
            stream.Float(Yaw);
            stream.Float(Pitch);
        }
    }
}
