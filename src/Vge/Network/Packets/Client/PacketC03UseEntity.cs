namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Отправляем на сервер взаимодействие с сущностью
    /// </summary>
    public struct PacketC03UseEntity : IPacket
    {
        public byte Id => 0x03;

        /// <summary>
        /// Id сущности с которой взаимодействуем
        /// </summary>
        public int Index { get; private set; }
        public EnumAction Action { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public PacketC03UseEntity(int id, EnumAction action, float x, float y, float z)
        {
            Index = id;
            Action = action;
            X = x;
            Y = y;
            Z = z;
        }
        public PacketC03UseEntity(int id, float x, float y, float z)
        {
            Index = id;
            Action = EnumAction.Inpulse;
            X = x;
            Y = y;
            Z = z;
        }
        public PacketC03UseEntity(int id, EnumAction action)
        {
            Index = id;
            Action = action;
            X = Y = Z = 0;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Index = stream.Int();
            Action = (EnumAction)stream.Byte();
            X = stream.Float();
            Y = stream.Float();
            Z = stream.Float();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Int(Index);
            stream.Byte((byte)Action);
            stream.Float(X);
            stream.Float(Y);
            stream.Float(Z);
        }

        /// <summary>
        /// Варианты действия
        /// </summary>
        public enum EnumAction
        {
            /// <summary>
            /// Импульс
            /// </summary>
            Inpulse = 1,
            /// <summary>
            /// Пробудить
            /// </summary>
            Awaken = 2,
            /// <summary>
            /// Взаимодействие
            /// </summary>
            Interact = 3,
            /// <summary>
            /// Атака
            /// </summary>
            Attack = 4
        }
    }
}

