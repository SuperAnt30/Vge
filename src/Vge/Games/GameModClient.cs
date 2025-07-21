using Vge.World;

namespace Vge.Games
{
    /// <summary>
    /// Объект игрового мода для клиентской части, этот объект наследуется другими проектами
    /// </summary>
    public class GameModClient
    {
        /// <summary>
        /// Объект окна
        /// </summary>
        protected readonly WindowMain _window;

        public GameModClient(WindowMain window) => _window = window;

        /// <summary>
        /// Инициализация после старта игры, когда уже все блоки и сущности загружены
        /// </summary>
        public virtual void InitAfterStartGame() { }

        /// <summary>
        /// Создать настройки мира по id
        /// </summary>
        public virtual WorldSettings CreateWorldSettings(byte id) => null;

    }
}
