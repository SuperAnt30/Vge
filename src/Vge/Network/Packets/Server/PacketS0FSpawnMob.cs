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
        public ushort IndexEntity { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        private bool _isLiving;

        public PacketS0FSpawnMob(EntityBase entity)
        {
            Index = entity.Id;
            IndexEntity = entity.IndexEntity;
            X = entity.PosX;
            Y = entity.PosY;
            Z = entity.PosZ;

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
            Index = stream.Int();
            IndexEntity = stream.UShort();
            X = stream.Float();
            Y = stream.Float();
            Z = stream.Float();
            _isLiving = stream.Bool();
            if (_isLiving)
            {
                Yaw = stream.Float();
                Pitch = stream.Float();
            }
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(Index);
            stream.UShort(IndexEntity);
            stream.Float(X);
            stream.Float(Y);
            stream.Float(Z);
            stream.Bool(_isLiving);
            if (_isLiving)
            {
                stream.Float(Yaw);
                stream.Float(Pitch);
            }
        }
    }
}
