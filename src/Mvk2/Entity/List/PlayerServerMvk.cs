using Mvk2.Entity.Inventory;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Network;

namespace Mvk2.Entity.List
{
    /// <summary>
    /// Объект игрока, как локального так и сетевого, не сущность
    /// </summary>
    public class PlayerServerMvk : PlayerServer
    {
        public PlayerServerMvk(string login, string token, SocketSide socket, GameServer server) 
            : base(login, token, socket, server) { }

        /// <summary>
        /// Инициализация инвенторя
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _InitInventory() => Inventory = new InventoryPlayer();
    }
}
