using Mvk2.Entity.Inventory;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Games;

namespace Mvk2.Entity.List
{
    /// <summary>
    /// Объект игрока владельца, на клиенте Mvk
    /// </summary>
    public class PlayerClientOwnerMvk : PlayerClientOwner
    {
        public PlayerClientOwnerMvk(GameBase game) : base(game) { }

        /// <summary>
        /// Инициализация инвенторя
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _InitInventory() => Inventory = new InventoryPlayer();
    }
}
