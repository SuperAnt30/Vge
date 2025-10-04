using Mvk2.Entity.Inventory;
using Mvk2.Packets;
using System;
using System.Runtime.CompilerServices;
using Vge.Entity;
using Vge.Entity.Inventory;
using Vge.Entity.List;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Item;
using Vge.Network;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.World;

namespace Mvk2.Entity.List
{
    /// <summary>
    /// Объект игрока, как локального так и сетевого, не сущность
    /// </summary>
    public class PlayerServerMvk : PlayerServer
    {
        /// <summary>
        /// Объект инвенторя игрока
        /// </summary>
        public InventoryPlayer InvPlayer { get; private set; }

        public PlayerServerMvk(string login, string token, SocketSide socket, GameServer server) 
            : base(login, token, socket, server)
        {
            Inventory.SetStackInSlot(0, new ItemStack(Ce.Items.ItemObjects[0], 1, 315));
            Inventory.SetStackInSlot(1, new ItemStack(Ce.Items.ItemObjects[1], 12));
            Inventory.SetStackInSlot(2, new ItemStack(Ce.Items.ItemObjects[2], 16));
            Inventory.SetStackInSlot(3, new ItemStack(Ce.Items.ItemObjects[6]));
            Inventory.SetStackInSlot(4, new ItemStack(Ce.Items.ItemObjects[7]));
         //   Inventory.SetStackInSlot(5, new ItemStack(Ce.Items.ItemObjects[4]));
            Inventory.SetStackInSlot(6, new ItemStack(Ce.Items.ItemObjects[3], 3));
            Inventory.SetStackInSlot(7, new ItemStack(Ce.Items.ItemObjects[5]));

            Inventory.SetStackInSlot(8, new ItemStack(Ce.Items.ItemObjects[0]));
            Inventory.SetStackInSlot(14, new ItemStack(Ce.Items.ItemObjects[4]));

            // При запуске на сервере
            InvPlayer.CheckingClothes(false); // иметируем клиента, чтоб не дропнуть предметы с рюкзака
        }

        /// <summary>
        /// Инициализация инвенторя
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _CreateInventory()
        {
            Inventory = InvPlayer = new InventoryPlayer(this);
            InvPlayer.SlotSetted += _InventoryPlayer_SlotSetted;
            InvPlayer.SlotStorageChanged += _InventoryPlayer_SlotStorageChanged;
        }

        /// <summary>
        /// Событие слот хранилища изменён
        /// </summary>
        private void _InventoryPlayer_SlotStorageChanged(object sender, SlotEventArgs e)
        {
            // SendToAllPlayersUseTileEntity Mvk 1
        }
        //=> GetWorld().Tracker.SendToAllTrackingEntity(this, new PacketS0BAnimation(Id, packet.Animation));

        /// <summary>
        /// Событие изменён слот
        /// </summary>
        private void _InventoryPlayer_SlotSetted(object sender, SlotEventArgs e)
        {
            // CheckKnowledge(stack);
            SendPacket(new PacketS2FSetSlot((short)e.SlotId, e.Stack));
        }

        /// <summary>
        /// Пакет: кликов по окну и контролам
        /// </summary>
        public override void PacketClickWindow(PacketC0EClickWindow packet)
        {
            // Пометка активности игрока
            MarkPlayerActive();
            EnumActionClickWindow action = (EnumActionClickWindow)packet.Action;

            if (action == EnumActionClickWindow.Close || action == EnumActionClickWindow.ThrowOutAir)
            {
                InvPlayer.ThrowOutAir();
            }

            switch (action)
            {
                case EnumActionClickWindow.OpenInventory:
                    InvPlayer.ServerOpenInventory();
                    break;
                case EnumActionClickWindow.Close:
                    Console.WriteLine("Close");
                    InvPlayer.ServerCloseInventory();
                    break;
                case EnumActionClickWindow.ClickSlot:
                    InvPlayer.ClickInventoryOnServer(packet.Number, packet.IsRight, packet.IsShift);
                    break;
            }
        }

        #region Drop

        /// <summary>
        /// Дропнуть предмет от сущности. Server
        /// </summary>
        /// <param name="itemStack">Стак предмета</param>
        /// <param name="inFrontOf">Флаг перед собой</param>
        /// <param name="longAway">Далеко бросить от себя</param>
        public override void DropItem(ItemStack itemStack, bool inFrontOf, bool longAway)
        {
            WorldServer worldServer = GetWorld();
            if (worldServer != null)
            {
                ushort id = Ce.Entities.IndexItem;
                EntityBase entity = Ce.Entities.CreateEntityServer(id, worldServer);
                if (entity is EntityItem entityItem)
                {
                    entityItem.SetEntityItemStack(itemStack);
                }
                worldServer.EntityDropsEntityInWorld(this, entity, inFrontOf, longAway);
            }
        }

        #endregion
    }
}
