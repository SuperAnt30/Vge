using Vge.Network.Packets.Server;
using Vge.World.Сalendar;

namespace Vge.World
{
    /// <summary>
    /// Опции мира
    /// </summary>
    public class WorldSettings
    {
        /// <summary>
        /// ID объекта настроек мира
        /// </summary>
        public byte IdSetting { get; protected set; } = 1;
        /// <summary>
        /// Не имеет неба, true
        /// </summary>
        public bool HasNoSky { get; protected set; } = false;
        /// <summary>
        /// Активный радиус обзора для сервера, нужен для спавна и тиков блоков
        /// </summary>
        public byte ActiveRadius { get; protected set; } = 8;
        /// <summary>
        /// Количество секций в чанке. Максимально 32
        /// </summary>
        public byte NumberChunkSections { get; protected set; } = 8;
        /// <summary>
        /// Календарь
        /// </summary>
        public IСalendar Calendar { get; protected set; }

        public WorldSettings()
            //=> Calendar = new Сalendar32(36000); // 24000 при 20 тиках = 20 мин. При 30 тиках = 36000
            => Calendar = new Сalendar32(360);

        /// <summary>
        /// Пакет Возраждение в мире
        /// </summary>
        public void PacketRespawnInWorld(PacketS07RespawnInWorld packet)
        {
            HasNoSky = packet.HasNoSky;
            NumberChunkSections = packet.NumberChunkSections;
            Calendar.SetTickCounter(packet.TickCounter);
        }
    }
}
