using Vge.Util;
using Vge.World.Block;
using WinGL.Util;

namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Отправляем на сервер взаимодействие с выбранным предметом в руке без RayCast блока
    /// </summary>
    public struct PacketC05UseItem : IPacket
    {
        public byte Id => 0x05;

        /// <summary>
        /// Вспомогательное ли действие
        /// </summary>
        public bool IsSecond { get; private set; }
        /// <summary>
        /// Дополнительный цифровой параметр
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Взаимодействие вспомогательного предмета без блока с параметром
        /// </summary>
        public PacketC05UseItem(int number)
        {
            Number = number;
            IsSecond = true;
        }

        public void ReadPacket(ReadPacket stream)
        {
            IsSecond = stream.Bool();
            if (IsSecond)
            {
                Number = stream.Int();
            }
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Bool(IsSecond);
            if (IsSecond)
            {
                stream.Int(Number);
            }
        }
    }
}
