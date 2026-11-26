using Vge.Network.Packets.Client;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Пакет анимации
    /// </summary>
    public struct PacketS0BAnimation : IPacket
    {
        public byte Id => 0x0B;

        public int EntityId { get; private set; }
        public byte Moving { get; private set; }
        public string Code { get; private set; }
        public float Speed { get; private set; }
        public EnumAction Action { get; private set; }

        public PacketS0BAnimation(int entityId, PacketC0APlayerAnimation packet)
        {
            EntityId = entityId;
            Moving = packet.Moving;
            Action = (EnumAction)packet.Action;
            Code = packet.Code;
            Speed = packet.Speed;
        }

        public PacketS0BAnimation(int entityId, byte moving)
        {
            EntityId = entityId;
            Moving = moving;
            Action = EnumAction.Moving;
            Code = "";
            Speed = 0;
        }

        public PacketS0BAnimation(int entityId, EnumAction action)
        {
            EntityId = entityId;
            Action = action;
            Code = "";
            Speed = 0;
            Moving = 0;
        }

        public PacketS0BAnimation(int entityId, string code, EnumAction action = EnumAction.Code, float speed = 1)
        {
            EntityId = entityId;
            Moving = 0;
            Action = action;
            Code = code;
            Speed = speed;
        }

        public void ReadPacket(ReadPacket stream)
        {
            EntityId = stream.Int();
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
            stream.Int(EntityId);
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
