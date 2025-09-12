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
        protected override void _CreateInventory()
        {
            InventoryPlayer inventoryPlayer = new InventoryPlayer();
            Inventory = inventoryPlayer;
            inventoryPlayer.CurrentItemChanged += InventoryPlayer_CurrentItemChanged;
        }

        private void InventoryPlayer_CurrentItemChanged(object sender, System.EventArgs e)
            => Render.CurrentItemChanged();
    }
}
