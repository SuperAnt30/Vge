using Mvk2.Entity;
using Mvk2.Entity.List;
using Mvk2.Gui;
using Mvk2.World;
using Mvk2.World.Biome;
using Mvk2.World.BlockEntity;
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

        /// <summary>
        /// Игрок мода, на клиенте
        /// </summary>
        public PlayerClientOwnerMvk Player { get; private set; }

        public GameModClientMvk(WindowMvk window) : base(window)
        {
            _windowMvk = window;
            Colors.CreateGrass(Biomes.ColorsGrass);
            Colors.CreateWater(Biomes.ColorsWater);
        }

        /// <summary>
        /// Корректировка блоков, сущностей и прочего перед инициализации миров, 
        /// тут только сетевой!
        /// Для инициализация ID сущностей и подобного.
        /// </summary>
        public override void CorrectObjects(PacketS02LoadingGame packet)
        {
            base.CorrectObjects(packet);
            // Присвоение корректных ID
            EntitiesRegMvk.InitId();
            BlocksEntityRegMvk.InitId();
        }

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
        {
            Player = new PlayerClientOwnerMvk(Game);
            return Player;
        }

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
                // скрин хранилища
                case Keys.R: _windowMvk.LScreenMvk.StorageDebug(); break;
                // скрин креативного инвентаря
                case Keys.C: 
                    if (Player.CreativeMode) _windowMvk.LScreenMvk.CreativeInventory(); break;
                case Keys.F5: Player.ViewCameraNext(); break;
                // Скрин
                case Keys.F6: DebugMvk.ScreenFileBiomeArea(Game); break;
                // DrawOrto
                case Keys.F7: Player.DebugOrtoNext(); break;
                    
            }
        }

        #endregion
    }
}
