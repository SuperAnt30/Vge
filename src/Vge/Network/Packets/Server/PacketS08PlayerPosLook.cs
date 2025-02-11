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
        public bool Impulse { get; private set; }

        /// <summary>
        /// Задать импульс игроку
        /// </summary>
        public PacketS08PlayerPosLook(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
            Yaw = Pitch = 0;
            Impulse = true;
        }

        /// <summary>
        /// Задать расположение игроку
        /// </summary>
        public PacketS08PlayerPosLook(float x, float y, float z, float yaw, float pitch)
        {
            X = x;
            Y = y;
            Z = z;
            Yaw = yaw;
            Pitch = pitch;
            Impulse = false;
        }

        public void ReadPacket(ReadPacket stream)
        {
            X = stream.Float();
            Y = stream.Float();
            Z = stream.Float();
            Impulse = stream.Bool();
            if (!Impulse)
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
            stream.Bool(Impulse);
            if (!Impulse)
            {
                stream.Float(Yaw);
                stream.Float(Pitch);
            }
        }
    }
}
