using Vge.Entity.Player;
using Vge.Gui.Huds;
using Vge.Gui.Screens;
using Vge.Network.Packets.Server;
using Vge.World;
using WinGL.Actions;

namespace Vge.Games
{
    /// <summary>
    /// Объект игрового мода для клиентской части, этот объект наследуется другими проектами
    /// </summary>
    public class GameModClient
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public GameBase Game { get; private set; }
        /// <summary>
        /// Объект окна
        /// </summary>
        protected readonly WindowMain _window;

        public GameModClient(WindowMain window) => _window = window;

        public void Init(GameBase game) => Game = game;

        /// <summary>
        /// Объект экрана
        /// </summary>
        public ScreenBase Screen => _window.Screen;

        /// <summary>
        /// Инициализация после старта игры, когда уже все блоки и сущности загружены
        /// </summary>
        public virtual void InitAfterStartGame() { }

        /// <summary>
        /// Создать настройки мира по id
        /// </summary>
        public virtual WorldSettings CreateWorldSettings(byte id) => null;

        /// <summary>
        /// Создать объект сетевого игрока
        /// </summary>
        public virtual PlayerClient CreatePlayerClient(PacketS0CSpawnPlayer packet)
            => new PlayerClient(Game, packet.Index, packet.Uuid, packet.Login, packet.IdWorld);

        /// <summary>
        /// Создать объект игрока владельца
        /// </summary>
        public virtual PlayerClientOwner CreatePlayerClientOwner() 
            => new PlayerClientOwner(Game);

        /// <summary>
        /// Создать объект индикация
        /// </summary>
        public virtual HudBase CreateHud() => new HudDebug(Game);

        #region Key

        /// <summary>
        /// Клавиша нажата
        /// </summary>
        public virtual void OnKeyDown(Keys keys) { }

        #endregion
    }
}
