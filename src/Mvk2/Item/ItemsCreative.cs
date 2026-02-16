using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Item;
using Vge.Network.Packets.Server;
using Vge.World.BlockEntity;

namespace Mvk2.Item
{
    /// <summary>
    /// Объект регистрации и данных креативногоинвентаря
    /// </summary>
    public sealed class ItemsCreative : IBlockStorage
    {
        private readonly ItemStack[] _stacks;

        public ItemsCreative()
        {
            _stacks = new ItemStack[48];

            _stacks[0] = new ItemStack(ItemsRegMvk.Cobblestone);
            _stacks[1] = new ItemStack(ItemsRegMvk.FlowerClover);
            _stacks[2] = new ItemStack(ItemsRegMvk.Brol);
            _stacks[12] = new ItemStack(ItemsRegMvk.AxeIron, 1, ItemsRegMvk.AxeIron.MaxDamage);
        }

        /// <summary>
        /// Добавляет стак предметов в инвентарь, возвращает false, если это невозможно.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddItemStackToInventory(ItemStack stack) => CanPutItemStack(0, stack);

        /// <summary>
        /// Проверяем можно ли установить данный стак в определённой ячейке склада
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanPutItemStack(int slotIn, ItemStack stack) => stack != null;

        /// <summary>
        /// Проверить равенства
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckEquals(IBlockStorage blockStorage) => false;

        /// <summary>
        /// Получить стак в конкретном слоте
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemStack GetStackInSlot(int slotIn) => _stacks[slotIn];

        /// <summary>
        /// Открыли окно, вызывается объектом EntityPlayerServer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OpenWindow(PlayerServer entityPlayer)
            => entityPlayer.SendPacket(new PacketS30WindowItems(false, _stacks));

        /// <summary>
        /// Задать стак в конкретном слоте
        /// </summary>
        /// <param name="isModify">Отметить что было изменение, для перезаписи на сервере в файл сохранения</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStackInSlot(int slotIn, ItemStack stack) { }
    }
}
