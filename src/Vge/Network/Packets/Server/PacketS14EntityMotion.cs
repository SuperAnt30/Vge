using Vge.Entity;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Движение сущности
    /// </summary>
    public struct PacketS14EntityMotion : IPacket
    {
        public byte Id => 0x14;

        public int Index { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }
        public bool OnGround { get; private set; }
        public bool Sleep { get; private set; } // временно

        public PacketS14EntityMotion(EntityBase entity)
        {
            Index = entity.Id;
            X = entity.PosX;
            Y = entity.PosY;
            Z = entity.PosZ;
            Yaw = entity.RotationYaw;
            Pitch = entity.RotationPitch;
            OnGround = entity.OnGround;
            Sleep = entity.IsPhysicSleepDebug();
        }

        public void ReadPacket(ReadPacket stream)
        {
            Index = stream.Int();
            X = stream.Float();
            Y = stream.Float();
            Z = stream.Float();
            Yaw = stream.Float();
            Pitch = stream.Float();
            OnGround = stream.Bool();
            Sleep = stream.Bool();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(Index);
            stream.Float(X);
            stream.Float(Y);
            stream.Float(Z);
            stream.Float(Yaw);
            stream.Float(Pitch);
            stream.Bool(OnGround);
            stream.Bool(Sleep);
        }
    }
}
