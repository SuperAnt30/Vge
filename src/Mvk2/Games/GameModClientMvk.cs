using Vge;
using Vge.Games;

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
    }
}
