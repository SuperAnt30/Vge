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
#if PhysicsServer
        public bool IsRotate { get; private set; }
#endif
        public PacketS08PlayerPosLook(float x, float y, float z, float yaw, float pitch)
        {
            X = x;
            Y = y;
            Z = z;
            Yaw = yaw;
            Pitch = pitch;
#if PhysicsServer
            IsRotate = true;
        }

        public PacketS08PlayerPosLook(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
            Yaw = Pitch = 0;
            IsRotate = false;
#endif
        }

        public void ReadPacket(ReadPacket stream)
        {
            X = stream.Float();
            Y = stream.Float();
            Z = stream.Float();
#if PhysicsServer
            IsRotate = stream.Bool();
            if (IsRotate)
#endif
            {
                Yaw = stream.Float();
                Pitch = stream.Float();
            }
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Float(X);
            stream.Float(Y);
            stream.Float(Z);
#if PhysicsServer
            stream.Bool(IsRotate);
            if (IsRotate)
#endif
            {
                stream.Float(Yaw);
                stream.Float(Pitch);
            }
        }
    }
}
