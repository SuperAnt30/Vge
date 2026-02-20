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
        public EnumAction Action { get; private set; }

        /// <summary>
        /// Задать импульс игроку
        /// </summary>
        public PacketS08PlayerPosLook(EnumAction action)
        {
            Action = action;
            X = Y = Z = 0;
            Yaw = Pitch = 0;
        }

        /// <summary>
        /// Задать импульс игроку
        /// </summary>
        public PacketS08PlayerPosLook(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
            Yaw = Pitch = 0;
            Action = EnumAction.Impulse;
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
            Action = EnumAction.Moving;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Action = (EnumAction)stream.Byte();
            if (Action != EnumAction.Impulse)
            {
                X = stream.Float();
                Y = stream.Float();
                Z = stream.Float();
                if (Action == EnumAction.Moving)
                {
                    Yaw = stream.Float();
                    Pitch = stream.Float();
                }
            }
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Byte((byte)Action);
            if (Action != EnumAction.Impulse)
            {
                stream.Float(X);
                stream.Float(Y);
                stream.Float(Z);
                if (Action == EnumAction.Moving)
                {
                    stream.Float(Yaw);
                    stream.Float(Pitch);
                }
            }
        }

        /// <summary>
        /// Варианты действия
        /// </summary>
        public enum EnumAction
        {
            /// <summary>
            /// Перемещение
            /// </summary>
            Moving = 0,
            /// <summary>
            /// Импульс
            /// </summary>
            Impulse = 1,
            /// <summary>
            /// Пробудить
            /// </summary>
            Awaken = 2
        }
    }
}
