using Vge.World;

namespace Vge.Network.Packets.Server
{
    /// <summary>
    /// Возраждение в мире, передаём параметры мира
    /// </summary>
    public struct PacketS07RespawnInWorld : IPacket
    {
        public byte Id => 0x07;

        /// <summary>
        /// Индекс мира
        /// </summary>
        public byte IdWorld { get; private set; }
        /// <summary>
        /// ID объекта настроек мира
        /// </summary>
        public byte IdSetting { get; private set; }
        /// <summary>
        /// Не имеет неба, true
        /// </summary>
        public bool HasNoSky { get; private set; }
        /// <summary>
        /// Количество секций в чанке
        /// </summary>
        public byte NumberChunkSections { get; private set; }
        /// <summary>
        /// Время мира
        /// </summary>
        public uint TickCounter { get; private set; }

        /// <summary>
        /// Поставить проверку на загрузку начальная и финишная
        /// </summary>
        public PacketS07RespawnInWorld(byte idWorld, WorldSettings worldSettings)
        {
            IdSetting = worldSettings.IdSetting;
            NumberChunkSections = worldSettings.NumberChunkSections;
            HasNoSky = worldSettings.HasNoSky;
            TickCounter = worldSettings.Calendar.TickCounter;
            IdWorld = idWorld;
        }

        public void ReadPacket(ReadPacket stream)
        {
            IdWorld = stream.Byte();
            IdSetting = stream.Byte();
            NumberChunkSections = stream.Byte();
            HasNoSky = stream.Bool();
            TickCounter = stream.UInt();
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Byte(IdWorld);
            stream.Byte(IdSetting);
            stream.Byte(NumberChunkSections);
            stream.Bool(HasNoSky);
            stream.UInt(TickCounter);
        }
    }
}
