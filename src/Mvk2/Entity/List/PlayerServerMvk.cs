using Mvk2.Entity.Inventory;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Item;
using Vge.Network;

namespace Mvk2.Entity.List
{
    /// <summary>
    /// Объект игрока, как локального так и сетевого, не сущность
    /// </summary>
    public class PlayerServerMvk : PlayerServer
    {
        public PlayerServerMvk(string login, string token, SocketSide socket, GameServer server) 
            : base(login, token, socket, server)
        {
            Inventory.SetStackInSlot(1, new ItemStack(Ce.Items.ItemObjects[0], 315));
            Inventory.SetStackInSlot(2, new ItemStack(Ce.Items.ItemObjects[2], 12));
            Inventory.SetStackInSlot(5, new ItemStack(Ce.Items.ItemObjects[4]));

            Inventory.SetStackInSlot(8, new ItemStack(Ce.Items.ItemObjects[0]));
            Inventory.SetStackInSlot(9, new ItemStack(Ce.Items.ItemObjects[4]));
            Inventory.SetStackInSlot(6, new ItemStack(Ce.Items.ItemObjects[3], 3));
        }

        /// <summary>
        /// Инициализация инвенторя
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _CreateInventory() => Inventory = new InventoryPlayer();
    }
}
