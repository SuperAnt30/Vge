using Vge.World;
using Vge.World.Сalendar;

namespace Mvk2.World
{
    /// <summary>
    /// Опции основного мира остров
    /// </summary>
    public class WorldSettingsIsland : WorldSettings
    {
        public WorldSettingsIsland()
        {
            IdSetting = 1;
            ActiveRadius = 8;
            NumberChunkSections = 8;
            //Calendar = new Сalendar32(36000); // 24000 при 20 тиках = 20 мин. При 30 тиках = 36000
            Calendar = new Сalendar32(36000);
        }
    }
}
