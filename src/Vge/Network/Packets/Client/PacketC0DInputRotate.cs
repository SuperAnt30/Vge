namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Пакет инпутов вращения мыши используется для физики на сервере
    /// </summary>
    public struct PacketC0DInputRotate : IPacket
    {
        public byte Id => 0x0D;

        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        public PacketC0DInputRotate(float yaw, float pitch)
        {
            Yaw = yaw;
            Pitch = pitch;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Yaw = stream.Float();
            Pitch = stream.Float();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Float(Yaw);
            stream.Float(Pitch);
        }
    }
}

