using Mvk2.Entity.List;
using Vge.Entity.Inventory;

namespace Mvk2.Entity.Inventory
{
    /// <summary>
    /// Инвентарь игрока
    /// </summary>
    public class InventoryPlayerMvk : InventoryPlayer
    {
        //public const int 

        public InventoryPlayerMvk(PlayerServerMvk playerServer)
            // Первый слот одеждый это ячейка левой руки
            : base(playerServer, 8, 11, 25) { }
    }
}
