namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Отправляем на сервер урон предмета в руке
    /// </summary>
    public struct PacketC06DamageItem : IPacket
    {
        public byte Id => 0x06;

        /// <summary>
        /// Урон предмета левой руки
        /// </summary>
        public bool IsLeft { get; private set; }
        /// <summary>
        /// Урон предмета
        /// </summary>
        public byte Damage { get; private set; }

        /// <summary>
        /// Сила урона предмета в правой руке
        /// </summary>
        public PacketC06DamageItem(byte damage)
        {
            Damage = damage;
            IsLeft = false;
        }

        /// <summary>
        /// Сила урона предмета в руке выбранная параметром
        /// </summary>
        public PacketC06DamageItem(byte damage, bool isLeft)
        {
            Damage = damage;
            IsLeft = isLeft;
        }

        public void ReadPacket(ReadPacket stream)
        {
            IsLeft = stream.Bool();
            Damage = stream.Byte();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Bool(IsLeft);
            stream.Byte(Damage);
        }
    }
}
