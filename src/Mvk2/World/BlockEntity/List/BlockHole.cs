using System.Runtime.CompilerServices;
using Vge.Entity.Inventory;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Item;
using Vge.NBT;
using Vge.Network.Packets.Server;
using Vge.World.BlockEntity;

namespace Mvk2.World.BlockEntity.List
{
    /// <summary>
    /// Блок дыры хранения для всех, не привязан к конкретному блоку.
    /// Один на весь мир
    /// </summary>
    public class BlockHole : IBlockStorage
    {
        /// <summary>
        /// Количество ячеек дыры
        /// </summary>
        public const int Count = 48;
        /// <summary>
        /// Количество стаков в дыре
        /// </summary>
        private readonly ItemStack[] _stacks;

        /// <summary>
        /// Управление контейнером для передачи пачками
        /// </summary>
        private readonly ConteinerManagement _conteiner = new ConteinerManagement(100);

        private readonly GameServer _server;

        public BlockHole(GameServer server)
        {
            _stacks = new ItemStack[Count];
            _server = server;
            _conteiner.SendSetSlot += _Conteiner_SendSetSlot;
        }

        /// <summary>
        /// Тут поподаем когда через Shift заносятся стаки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _Conteiner_SendSetSlot(object sender, SlotEventArgs e)
        {
            // Обновляем всем кто сейчас смотрит содержимое
            _server.Players.SendToAllUseBlockStorage(this, new PacketS2FSetSlot((short)e.SlotId, e.Stack));
        }

        /// <summary>
        /// Открыли окно, вызывается объектом EntityPlayerServer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void OpenWindow(PlayerServer entityPlayer)
            => entityPlayer.SendPacket(new PacketS30WindowItems(false, _stacks));

        /// <summary>
        /// Проверить равенства
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckEquals(IBlockStorage blockStorage)
        {
            if (blockStorage == null) return false;
            return blockStorage.GetType() == typeof(BlockHole);
        }

        #region ItemStack

        /// <summary>
        /// Получить стак в конкретном слоте
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ItemStack GetStackInSlot(int slotIn) => _stacks[slotIn];

        /// <summary>
        /// Задать стак в конкретном слоте
        /// </summary>
        /// <param name="isModify">Отметить что было изменение, для перезаписи на сервере в файл сохранения</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetStackInSlot(int slotIn, ItemStack stack) => _stacks[slotIn] = stack;

        /// <summary>
        /// Добавляет стак предметов в инвентарь, возвращает false, если это невозможно.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool AddItemStackToInventory(ItemStack stack)
            => CanPutItemStack(0, stack) && _conteiner.AddItemStackToInventory(_stacks, 0, stack, _stacks.Length);

        /// <summary>
        /// Проверяем можно ли установить данный стак в определённой ячейке склада
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool CanPutItemStack(int slotIn, ItemStack stack) => stack != null;

        /// <summary>
        /// Заспавнить предметы при разрушении блока
        /// </summary>
        //public virtual void SpawnAsEntityOnBreakBlock() { }

        #endregion

        public void WriteToNBT(TagCompound nbt)
        {
            NBTTools.ItemStacksWriteToNBT(nbt, "Hole", _stacks);
        }

        public void ReadFromNBT(TagCompound nbt)
        {
            Slot[] slots = NBTTools.ItemStacksReadFromNBT(nbt, "Hole");
            foreach (Slot slot in slots)
            {
                if (slot.SlotId < Count)
                {
                    SetStackInSlot(slot.SlotId, slot.Stack);
                }
            }
        }
    }
}
