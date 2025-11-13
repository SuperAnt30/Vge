using Mvk2.Entity.List;
using Mvk2.Item;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Item;
using Vge.Network;
using Vge.TileEntity;

namespace Mvk2.Games
{
    /// <summary>
    /// Объект игрового мода для серверной части, для Малювеки 2
    /// </summary>
    public class GameModServerMvk : GameModServer
    {
        public GameModServerMvk() : base() { }

        /// <summary>
        /// Создать объект сетевого игрока
        /// </summary>
        public override PlayerServer CreatePlayerServer(string login, string token, SocketSide socketSide)
            => new PlayerServerMvk(login, token, socketSide, Server);

        /// <summary>
        /// Вторая инициализация когда готовы предметы и блоки и сущности
        /// </summary>
        public override void InitTwo()
        {
            // Для отладки
            Ce.TileHole = new TileEntityHole(Server, 48);

            Ce.TileHole.SetStackInSlot(0, new ItemStack(ItemsRegMvk.AxeIron, 1, 315));
            Ce.TileHole.SetStackInSlot(1, new ItemStack(ItemsRegMvk.Tie));
        }
    }
}
