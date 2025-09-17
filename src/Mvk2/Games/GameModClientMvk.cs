using Mvk2.Entity.List;
using Mvk2.Gui;
using Mvk2.World;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Gui.Huds;
using Vge.Network.Packets.Server;
using Vge.World;
using WinGL.Actions;

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

        public GameModClientMvk(WindowMvk window) : base(window) 
            => _windowMvk = window;

        /// <summary>
        /// Создать настройки мира по id
        /// </summary>
        public override WorldSettings CreateWorldSettings(byte id)
        {
            if (id == 2) return new WorldSettingsNightmare();
            return new WorldSettingsIsland();
        }

        /// <summary>
        /// Создать объект сетевого игрока
        /// </summary>
        public override PlayerClient CreatePlayerClient(PacketS0CSpawnPlayer packet)
            => new PlayerClientMvk(Game, packet.Index, packet.Uuid, packet.Login, packet.IdWorld);

        /// <summary>
        /// Создать объект игрока владельца
        /// </summary>
        public override PlayerClientOwner CreatePlayerClientOwner()
            => new PlayerClientOwnerMvk(Game);

        /// <summary>
        /// Создать объект индикация
        /// </summary>
        public override HudBase CreateHud() => new HudMvk(Game, _windowMvk.GetRender());

        #region Key

        /// <summary>
        /// Клавиша нажата
        /// </summary>
        public override void OnKeyDown(Keys keys)
        {
            switch (keys)
            {
                // Окно чата Клавиша "T" или "~"
                case Keys.T: case Keys.Oemtilde: _windowMvk.LScreenMvk.Chat(); break;
                // скрин инвентаря
                case Keys.E: _windowMvk.LScreenMvk.Inventory(); break;
            }
        }

        #endregion
    }
}
