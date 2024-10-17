using System.Numerics;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Пакет расположения игрока, при старте, телепорт, рестарте и тп
    /// </summary>
    public struct PacketS08PlayerPosLook : IPacket
    {
        public byte Id => 0x08;

        private Vector3 pos;
        private float yaw;
        private float pitch;

        public Vector3 GetPos() => pos;
        public float GetYaw() => yaw;
        public float GetPitch() => pitch;

        public PacketS08PlayerPosLook(Vector3 pos, float yaw, float pitch)
        {
            this.pos = pos;
            this.yaw = yaw;
            this.pitch = pitch;
        }

        public void ReadPacket(ReadPacket stream)
        {
            pos = new Vector3(stream.Float(), stream.Float(), stream.Float());
            yaw = stream.Float();
            pitch = stream.Float();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Float(pos.X);
            stream.Float(pos.Y);
            stream.Float(pos.Z);
            stream.Float(yaw);
            stream.Float(pitch);
        }
    }
}
