using Vge.Entity.Player;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Пакет атрибу игрока
    /// </summary>
    public struct PacketS39PlayerAbilities : IPacket
    {
        public byte Id => 0x39;

        public bool CreativeMode { get; private set; }
        public bool NoClip { get; private set; }
        public bool AllowFlying { get; private set; }
        public bool DisableDamage { get; private set; }
        public bool Spectator { get; private set; }

        public PacketS39PlayerAbilities(PlayerServer player)
        {
            CreativeMode = player.CreativeMode;
            NoClip = player.NoClip;
            AllowFlying = player.AllowFlying;
            DisableDamage = player.DisableDamage;
            Spectator = player.IsSpectator();
        }

        public void ReadPacket(ReadPacket stream)
        {
            CreativeMode = stream.Bool();
            NoClip = stream.Bool();
            AllowFlying = stream.Bool();
            DisableDamage = stream.Bool();
            Spectator = stream.Bool();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Bool(CreativeMode);
            stream.Bool(NoClip);
            stream.Bool(AllowFlying);
            stream.Bool(DisableDamage);
            stream.Bool(Spectator);
        }
    }
}
