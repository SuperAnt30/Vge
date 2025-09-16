using Mvk2.Entity.Inventory;
using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Inventory;
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
        protected override void _CreateInventory()
        {
            Inventory = new InventoryList(1, 11);
            Inventory.OutsideChanged += Inventory_OutsideChanged;
        }

        private void Inventory_OutsideChanged(object sender, EventArgs e)
            => Render.OutsideItemChanged();
    }
}
