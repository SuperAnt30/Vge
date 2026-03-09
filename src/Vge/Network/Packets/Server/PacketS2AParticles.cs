using System.IO;
using Vge.Item;
using WinGL.Util;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Частички, эффект
    /// </summary>
    public struct PacketS2AParticles : IPacket
    {
        public byte Id => 0x2A;

        public int PacketId { get; private set; }
        public byte Count { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float OffsetX { get; private set; }
        public float OffsetY { get; private set; }
        public float OffsetZ { get; private set; }
        public float Motion { get; private set; }
        public int Parameter { get; private set; }

        public PacketS2AParticles(int packetId, byte count, Vector3 position, 
            Vector3 offset, float motion, int parameter)
        {
            PacketId = packetId;
            Count = count;
            X = position.X;
            Y = position.Y;
            Z = position.Z;
            OffsetX = offset.X;
            OffsetY = offset.Y;
            OffsetZ = offset.Z;
            Motion = motion;
            Parameter = parameter;
        }

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
            stream.UShort((ushort)PacketId);
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
