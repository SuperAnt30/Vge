using Mvk2.Entity.Inventory;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Games;

namespace Mvk2.Entity.List
{
    /// <summary>
    /// Объект игроков на клиентской стороне
    /// </summary>
    public class PlayerClientMvk : PlayerClient
    {
        public PlayerClientMvk(GameBase game, int id, string uuid, string login, byte idWorld) 
            : base(game, id, uuid, login, idWorld) { }

        /// <summary>
        /// Инициализация инвенторя
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _InitInventory() => Inventory = new InventoryPlayer();
    }
}
