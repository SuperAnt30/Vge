using Vge.World;

namespace Mvk2.World
{
    /// <summary>
    /// Опции кошмарного мира
    /// </summary>
    public class WorldSettingsNightmare : WorldSettings
    {
        public WorldSettingsNightmare()
        {
            HasNoSky = true;
            ActiveRadius = 3;
            NumberChunkSections = 16;
        }
    }
}
