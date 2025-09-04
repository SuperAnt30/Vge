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

        /// <summary>
        /// Массив блоков
        /// </summary>
        public string[] Blocks { get; private set; }
        /// <summary>
        /// Массив предметов
        /// </summary>
        public string[] Items { get; private set; }
        /// <summary>
        /// Массив сущностей
        /// </summary>
        public string[] Entities { get; private set; }

        /// <summary>
        /// Передать таблицу блоков
        /// </summary>
        public PacketS02LoadingGame(string[] blocks, string[] items, string[] entities)
        {
            Status = EnumStatus.BeginNet;
            Blocks = blocks;
            Items = items;
            Entities = entities;
            Value = 0;
        }

        public PacketS02LoadingGame(EnumStatus status)
        {
            Entities = Items = Blocks = new string[0];
            Status = status;
            Value = 0;
        }

        public PacketS02LoadingGame(ushort value)
        {
            Entities = Items = Blocks = new string[0];
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
            else if (Status == EnumStatus.BeginNet)
            {
                ushort count = stream.UShort();
                Blocks = new string[count];
                for (int i = 0; i < count; i++)
                {
                    Blocks[i] = stream.String();
                }
                count = stream.UShort();
                Items = new string[count];
                for (int i = 0; i < count; i++)
                {
                    Items[i] = stream.String();
                }
                count = stream.UShort();
                Entities = new string[count];
                for (int i = 0; i < count; i++)
                {
                    Entities[i] = stream.String();
                }
            }
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Byte((byte)Status);
            if (Status == EnumStatus.Begin)
            {
                stream.UShort(Value);
            }
            else if (Status == EnumStatus.BeginNet)
            {
                ushort count = (ushort)Blocks.Length;
                stream.UShort(count);
                for (int i = 0; i < count; i++)
                {
                    stream.String(Blocks[i]);
                }
                count = (ushort)Items.Length;
                stream.UShort(count);
                for (int i = 0; i < count; i++)
                {
                    stream.String(Items[i]);
                }
                count = (ushort)Entities.Length;
                stream.UShort(count);
                for (int i = 0; i < count; i++)
                {
                    stream.String(Entities[i]);
                }
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
