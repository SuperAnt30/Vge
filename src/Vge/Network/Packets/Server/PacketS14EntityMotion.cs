using Vge.Entity;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Движение сущности
    /// </summary>
    public struct PacketS14EntityMotion : IPacket
    {
        public byte Id => 0x14;

        public int EntityId { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }
        public bool OnGround { get; private set; }
        public bool Sleep { get; private set; } // временно

        private bool _isLiving;

        public PacketS14EntityMotion(EntityBase entity)
        {
            EntityId = entity.Id;
            X = entity.PosX;
            Y = entity.PosY;
            Z = entity.PosZ;
            OnGround = entity.OnGround;
            Sleep = entity.IsPhysicSleepDebug();

            if (entity is EntityLiving entityLiving)
            {
                _isLiving = true;
                Yaw = entityLiving.RotationYaw;
                Pitch = entityLiving.RotationPitch;
            }
            else
            {
                _isLiving = false;
                Yaw = Pitch = 0;
            }
        }

        public void ReadPacket(ReadPacket stream)
        {
            EntityId = stream.Int();
            X = stream.Float();
            Y = stream.Float();
            Z = stream.Float();
            OnGround = stream.Bool();
            Sleep = stream.Bool();
            _isLiving = stream.Bool();
            if (_isLiving)
            {
                Yaw = stream.Float();
                Pitch = stream.Float();
            }
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(EntityId);
            stream.Float(X);
            stream.Float(Y);
            stream.Float(Z);
            stream.Bool(OnGround);
            stream.Bool(Sleep);
            stream.Bool(_isLiving);
            if (_isLiving)
            {
                stream.Float(Yaw);
                stream.Float(Pitch);
            }
        }
    }
}
