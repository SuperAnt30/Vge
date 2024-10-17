namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Загрузка мира, только для локальной
    /// </summary>
    public struct PacketS02LoadingGame : IPacket
    {
        public byte Id => 0x02;

        public EnumStatus Status { get; private set; }
        public ushort Value { get; private set; }

        public PacketS02LoadingGame(EnumStatus status)
        {
            Status = status;
            Value = 0;
        }

        public PacketS02LoadingGame(ushort value)
        {
            Status = EnumStatus.Begin;
            Value = value;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Status = (EnumStatus)stream.Byte();
            if (Status == EnumStatus.Begin)
            {
                Value = stream.UShort();
            }
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Byte((byte)Status);
            if (Status == EnumStatus.Begin)
            {
                stream.UShort(Value);
            }
        }

        public enum EnumStatus
        {
            /// <summary>
            /// Начальный запуск для ответа сколько шагов (local)
            /// </summary>
            Begin = 1,
            /// <summary>
            /// Шаг загрузки (local)
            /// </summary>
            Step = 2,
            /// <summary>
            /// Начальный запуск по сети (net)
            /// </summary>
            BeginNet = 3,
            /// <summary>
            /// Отказ версия сервера иная (net)
            /// </summary>
            VersionAnother = 4,
            /// <summary>
            /// Отказ по дубликате никнейма (net) 
            /// </summary>
            LoginDuplicate = 5,
            /// <summary>
            /// Отказ по некорректности никнейма (net) 
            /// </summary>
            LoginIncorrect = 6,
            /// <summary>
            /// Не верный токен
            /// </summary>
            InvalidToken = 7,
            /// <summary>
            /// Для серверной части, сервер запущен
            /// </summary>
            ServerGo = 8
        }
    }
}
