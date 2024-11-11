using Vge.Entity;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Пакет расположения игрока, при старте, телепорт, рестарте и тп
    /// </summary>
    public struct PacketS08PlayerPosLook : IPacket
    {
        public byte Id => 0x08;

        public EntityPos Position { get; private set; }

        public PacketS08PlayerPosLook(EntityPos pos)
        {
            Position = pos;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Position = new EntityPos();
            Position.ReadPacket(stream);
        }

        public void WritePacket(WritePacket stream)
        {
            Position.WritePacket(stream);
        }
    }
}
