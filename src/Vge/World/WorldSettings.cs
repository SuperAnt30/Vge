using Vge.NBT;
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
        /// Имя пути к папке мира
        /// </summary>
        public readonly string PathWorld;
        /// <summary>
        /// Имя файла с путем к настройкам мира
        /// </summary>
        protected readonly string _pathFileSetting;

        /// <summary>
        /// Для сервера
        /// </summary>
        protected WorldSettings(string pathWorld)
        {
            PathWorld = pathWorld;
            _pathFileSetting = PathWorld + "setting.dat";
            _Init();
        }
        /// <summary>
        /// Для клиента
        /// </summary>
        protected WorldSettings() => _Init();

        protected virtual void _Init() { }

        /// <summary>
        /// Пакет Возраждение в мире
        /// </summary>
        public void PacketRespawnInWorld(PacketS07RespawnInWorld packet)
        {
            HasNoSky = packet.HasNoSky;
            NumberChunkSections = packet.NumberChunkSections;
            Calendar.SetTickCounter(packet.TickCounter);
        }

        /// <summary>
        /// Сохраняем доп данных мира
        /// </summary>
        public void WriteToFile()
        {
            TagCompound nbt = new TagCompound();
            _WriteToNBT(nbt);
            NBTTools.WriteToFile(nbt, _pathFileSetting, true);
        }

        /// <summary>
        /// Сохранить данные мира
        /// </summary>
        protected virtual void _WriteToNBT(TagCompound nbt) 
        {
            nbt.SetLong("TickCounter", Calendar.TickCounter);
        }
    }
}
