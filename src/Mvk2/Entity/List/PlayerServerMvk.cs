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
            Inventory.SetInventorySlotContents(0, new ItemStack(Ce.Items.ItemObjects[1]));
            Inventory.SetInventorySlotContents(2, new ItemStack(Ce.Items.ItemObjects[2]));
            Inventory.SetInventorySlotContents(3, new ItemStack(Ce.Items.ItemObjects[3]));
            Inventory.SetInventorySlotContents(6, new ItemStack(Ce.Items.ItemObjects[0]));
        }

        /// <summary>
        /// Инициализация инвенторя
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _InitInventory() => Inventory = new InventoryPlayer();
    }
}
