using Mvk2.World;
using Vge.Games;
using Vge.World;

namespace Mvk2.Games
{
    /// <summary>
    /// Объект игрового мода для клиентской части, для Малювеки 2
    /// </summary>
    public class GameModClientMvk : GameModClient
    {
        /// <summary>
        /// Объект окна малювек
        /// </summary>
        private readonly WindowMvk _windowMvk;

        public GameModClientMvk(WindowMvk window) : base(window) => _windowMvk = window;

        /// <summary>
        /// Создать настройки мира по id
        /// </summary>
        public override WorldSettings CreateWorldSettings(byte id)
            => id == 2 ? new WorldSettingsNightmare() : new WorldSettings();
    }
}
