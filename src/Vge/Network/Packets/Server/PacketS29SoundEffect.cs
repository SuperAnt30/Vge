using Vge.Item;
using WinGL.Util;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Звуковой эффект в глобальных координатах мира
    /// </summary>
    public struct PacketS29SoundEffect : IPacket
    {
        public byte Id => 0x29;

        public int SoundId { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float Volume { get; private set; }
        public float Pitch { get; private set; }

        public PacketS29SoundEffect(int soundId, Vector3 position, float volume, float pitch)
        {
            SoundId = soundId;
            X = position.X;
            Y = position.Y;
            Z = position.Z;
            Volume = volume;
            Pitch = pitch;
        }

        public void ReadPacket(ReadPacket stream)
        {
            SoundId = stream.UShort();
            X = stream.Float();
            Y = stream.Float();
            Z = stream.Float();
            Volume = stream.Byte() / 255f;
            Pitch = stream.Float();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.UShort((ushort)SoundId);
            stream.Float(X);
            stream.Float(Y);
            stream.Float(Z);
            stream.Byte((byte)Mth.Clamp((int)(Volume * 255), 0, 255));
            stream.Float(Pitch);
        }
    }
}
