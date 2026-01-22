using Vge.Network.Packets.Server;
using Vge.Util;
using Vge.World.Block;
using Vge.World.Gen;
using Vge.World.Сalendar;

namespace Vge.World
{
    /// <summary>
    /// Опции мира
    /// </summary>
    abstract public class WorldSettings
    {
        /// <summary>
        /// ID объекта настроек мира
        /// </summary>
        public byte IdSetting { get; protected set; }
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
        /// <summary>
        /// Объект для генерации чанков, только для серверной части
        /// </summary>
        public IChunkProviderGenerate ChunkGenerate { get; protected set; }
        /// <summary>
        /// Массив кеш блоков для обновления блоков текущего мира в потоке тиков.
        /// НЕ ГЕНЕРАЦИЯ ЧАНКА, чанк генерируется в другом потоке!
        /// </summary>
        public readonly ArrayFast<BlockCache> BlockCaches = new ArrayFast<BlockCache>(16384);

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
