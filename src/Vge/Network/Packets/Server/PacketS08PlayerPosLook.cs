namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Пакет расположения игрока, при старте, телепорт, рестарте и тп
    /// </summary>
    public struct PacketS08PlayerPosLook : IPacket
    {
        public byte Id => 0x08;

        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        public PacketS08PlayerPosLook(float x, float y, float z, float yaw, float pitch)
        {
            X = x;
            Y = y;
            Z = z;
            Yaw = yaw;
            Pitch = pitch;
        }

        public void ReadPacket(ReadPacket stream)
        {
            X = stream.Float();
            Y = stream.Float();
            Z = stream.Float();
            Yaw = stream.Float();
            Pitch = stream.Float();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Float(X);
            stream.Float(Y);
            stream.Float(Z);
            stream.Float(Yaw);
            stream.Float(Pitch);
        }
    }
}
