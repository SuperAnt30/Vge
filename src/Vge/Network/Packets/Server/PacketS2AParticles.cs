using WinGL.Util;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Частички, эффект
    /// </summary>
    public struct PacketS2AParticles : IPacket
    {
        public byte Id => 0x2A;

        public ushort PacketId { get; private set; }
        public byte Count { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float OffsetX { get; private set; }
        public float OffsetY { get; private set; }
        public float OffsetZ { get; private set; }
        public float Motion { get; private set; }
        public int Parameter { get; private set; }

        public PacketS2AParticles(ushort packetId, int count, Vector3 position, 
            Vector3 offset, float motion, int parameter)
        {
            PacketId = packetId;
            Count = (byte)count;
            X = position.X;
            Y = position.Y;
            Z = position.Z;
            OffsetX = offset.X;
            OffsetY = offset.Y;
            OffsetZ = offset.Z;
            Motion = motion;
            Parameter = parameter;
        }

        public Vector3 GetPosition() => new Vector3(X, Y, Z);
        public Vector3 GetOffset() => new Vector3(OffsetX, OffsetY, OffsetZ);

        public void ReadPacket(ReadPacket stream)
        {
            PacketId = stream.UShort();
            Count = stream.Byte();
            X = stream.Float();
            Y = stream.Float();
            Z = stream.Float();
            OffsetX = stream.Float();
            OffsetY = stream.Float();
            OffsetZ = stream.Float();
            Motion = stream.Float();
            Parameter = stream.Int();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.UShort(PacketId);
            stream.Byte(Count);
            stream.Float(X);
            stream.Float(Y);
            stream.Float(Z);
            stream.Float(OffsetX);
            stream.Float(OffsetY);
            stream.Float(OffsetZ);
            stream.Float(Motion);
            stream.Int(Parameter);
        }
    }
}
