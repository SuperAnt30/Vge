using Mvk2.Entity.Inventory;
using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Network.Packets.Server;
using Vge.World.Chunk;
using WinGL.Util;

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

        public PlayerClientOwnerMvk(GameBase game) : base(game)
        {
            _handManager = new HandManagerClient(game, this);
        }

        /// <summary>
        /// Инициализация инвенторя
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _CreateInventory()
        {
            Inventory = InvPlayer = new InventoryPlayerMvk(null);
            Inventory.OutsideChanged += (sender, e) => Render.OutsideItemChanged();
        }

        /// <summary>
        /// Для определения параметров блока чанк и локальные координаты блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override string _ToBlockInfo(ChunkBase chunk, Vector3i pos) 
            => DebugMvk.ToBlockInfo(chunk, pos);

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
                //Console.WriteLine(slotId + " " + packet.Stack?.ToString());

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
