namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Пакет анимации
    /// </summary>
    public struct PacketC0APlayerAnimation : IPacket
    {
        public byte Id => 0x0A;

        public byte Moving { get; private set; }
        public string Code { get; private set; }
        public float Speed { get; private set; }
        public EnumAction Action { get; private set; }

        public PacketC0APlayerAnimation(byte moving)
        {
            Moving = moving;
            Action = EnumAction.Moving;
            Code = "";
            Speed = 0;
        }

        public PacketC0APlayerAnimation(string code, EnumAction action = EnumAction.Code, float speed = 1)
        {
            Moving = 0;
            Action = action;
            Code = code;
            Speed = speed;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Action = (EnumAction)stream.Byte();
            if (Action == EnumAction.Moving)
            {
                Moving = stream.Byte();
            }
            else if (Action == EnumAction.Code || Action == EnumAction.CodeAdd)
            {
                Code = stream.String();
                Speed = stream.Float();
            }
            else if (Action == EnumAction.Stoping)
            {
                Code = stream.String();
            }
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Byte((byte)Action);
            if (Action == EnumAction.Moving)
            {
                stream.Byte(Moving);
            }
            else if (Action == EnumAction.Code || Action == EnumAction.CodeAdd)
            {
                stream.String(Code);
                stream.Float(Speed);
            }
            else if (Action == EnumAction.Stoping)
            {
                stream.String(Code);
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
            /// По коду
            /// </summary>
            Code = 1,
            /// <summary>
            /// Дополнительный по коду
            /// </summary>
            CodeAdd = 2,
            /// <summary>
            /// Остановка клипа
            /// </summary>
            Stoping = 3,
            /// <summary>
            /// Анимация открыть глаза
            /// </summary>
            EyeOpen = 4,
            /// <summary>
            /// Анимация закрыть глаза
            /// </summary>
            EyeClose = 5
        }
    }
}
