using Mvk2.Entity.Inventory;
using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Network.Packets.Server;

namespace Mvk2.Entity.List
{
    /// <summary>
    /// Объект игрока владельца, на клиенте Mvk
    /// </summary>
    public class PlayerClientOwnerMvk : PlayerClientOwner
    {
        /// <summary>
        /// Объект инвенторя игрока
        /// </summary>
        private InventoryPlayer _inventoryPlayer;

        public PlayerClientOwnerMvk(GameBase game) : base(game) { }

        /// <summary>
        /// Инициализация инвенторя
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _CreateInventory()
        {
            Inventory = _inventoryPlayer = new InventoryPlayer();
            Inventory.OutsideChanged += Inventory_OutsideChanged;
            _inventoryPlayer.CurrentIndexChanged += Inventory_OutsideChanged;
        }

        private void Inventory_OutsideChanged(object sender, EventArgs e)
            => Render.OutsideItemChanged();

        #region Packet

        /// <summary>
        /// Пакет управления передвежением и изменением слота
        /// </summary>
        public override void PacketSetSlot(PacketS2FSetSlot packet)
        {
            short slotId = packet.SlotId;
            // Slot slot = packet.GetSlot();
            if (slotId == 255)
            {
                // Пришёл стак для воздуха

            }
            else if (slotId < 100)
            {
                // Пришёл стак для инвентаря
                _inventoryPlayer.SetInventorySlotContents(slotId, packet.Stack);
            //    if (slot == ClientMain.Player.InventPlayer.CurrentItem)
            //    {
            //        ClientMain.Player.ItemInWorldManagerDestroyAbout();
            //    }
            }
            else
            {
                // Пришёл стак для склада
            //    ClientMain.Screen.AcceptNetworkPackage(packet);
            }
        }

        #endregion
    }
}
