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
        public InventoryPlayerMvk InvPlayer { get; private set; }

        public PlayerClientOwnerMvk(GameBase game) : base(game) { }

        /// <summary>
        /// Инициализация инвенторя
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _CreateInventory()
        {
            Inventory = InvPlayer = new InventoryPlayerMvk(null);
            Inventory.OutsideChanged += _Inventory_OutsideChanged;
            InvPlayer.CurrentIndexChanged += _Inventory_OutsideChanged;
        }

        private void _Inventory_OutsideChanged(object sender, EventArgs e)
            => Render.OutsideItemChanged();

        #region Packet

        /// <summary>
        /// Пакет управления передвежением и изменением слота
        /// </summary>
        public override void PacketSetSlot(PacketS2FSetSlot packet)
        {
            short slotId = packet.SlotId;

            if (slotId < 100 || slotId == 255)
            {
                // Пришёл стак для инвентаря
                InvPlayer.SetInventorySlotContents(slotId, packet.Stack);

            //    if (slot == ClientMain.Player.InventPlayer.CurrentItem)
            //    {
            //        ClientMain.Player.ItemInWorldManagerDestroyAbout();
            //    }

            }
            else
            {
                // Пришёл стак для склада
                _game.ModClient.Screen?.AcceptNetworkPackage(packet);
            }
        }

        #endregion
    }
}
