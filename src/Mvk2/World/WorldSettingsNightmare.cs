using Mvk2.World.Gen;
using System.IO;
using Vge.NBT;
using Vge.World;
using Vge.World.Сalendar;

namespace Mvk2.World
{
    /// <summary>
    /// Опции кошмарного мира
    /// </summary>
    public class WorldSettingsNightmare : WorldSettings
    {
        /// <summary>
        /// Для клиента
        /// </summary>
        public WorldSettingsNightmare() : base() { }

        /// <summary>
        /// Для сервера
        /// </summary>
        public WorldSettingsNightmare(string pathWorld) : base(pathWorld)
        {
            ChunkGenerate = new ChunkProviderGenerateDebug(NumberChunkSections);

            if (File.Exists(_pathFileSetting))
            {
                TagCompound nbt = NBTTools.ReadFromFile(_pathFileSetting, true);
                Calendar.SetTickCounter((uint)nbt.GetLong("TickCounter"));
            }
        }

        protected override void _Init()
        {
            IdSetting = 2;
            // HasNoSky = true;
            ActiveRadius = 3;
            NumberChunkSections = 16;
            Calendar = new СalendarNone();
        }
    }
}
