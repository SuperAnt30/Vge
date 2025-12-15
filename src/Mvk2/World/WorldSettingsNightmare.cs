using Mvk2.World.Gen;
using Vge.World;
using Vge.World.Сalendar;

namespace Mvk2.World
{
    /// <summary>
    /// Опции кошмарного мира
    /// </summary>
    public class WorldSettingsNightmare : WorldSettings
    {
        public WorldSettingsNightmare()
        {
            IdSetting = 2;
           // HasNoSky = true;
            ActiveRadius = 3;
            NumberChunkSections = 16;
            Calendar = new СalendarNone();
            ChunkGenerate = new ChunkProviderGenerateDebug(NumberChunkSections);
        }
    }
}
