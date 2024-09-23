namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Загрузка мира, только для локальной
    /// </summary>
    public struct PacketS02LoadingGame : IPacket
    {
        public byte GetId() => 0x02;

        private EnumStatus status;
        private ushort value;

        public EnumStatus GetStatus() => status;
        public ushort GetValue() => value;

        public PacketS02LoadingGame(EnumStatus status)
        {
            this.status = status;
            value = 0;
        }

        public PacketS02LoadingGame(ushort value)
        {
            status = EnumStatus.Begin;
            this.value = value;
        }

        public void ReadPacket(ReadPacket stream)
        {
            status = (EnumStatus)stream.Byte();
            if (status == EnumStatus.Begin)
            {
                value = stream.UShort();
            }
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Byte((byte)status);
            if (status == EnumStatus.Begin)
            {
                stream.UShort(value);
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
            InvalidToken = 7
        }
    }
}
