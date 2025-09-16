using Mvk2.Entity.Inventory;
using System;
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
            Inventory.OutsideChanged += Inventory_OutsideChanged;
            inventoryPlayer.CurrentIndexChanged += Inventory_OutsideChanged;
        }

        private void Inventory_OutsideChanged(object sender, EventArgs e)
            => Render.OutsideItemChanged();
    }
}
