using Vge.Entity.Inventory;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Item;
using Vge.Network.Packets.Server;

namespace Vge.TileEntity
{
    /// <summary>
    /// Базовый класс 
    /// </summary>
    public class TileEntityBase : ITileEntityInventory
    {
        /// <summary>
        /// Количество слотов
        /// </summary>
        public const int Count = 5;

        private ItemStack[] _stacks = new ItemStack[Count];

        /// <summary>
        /// Управление контейнером для передачи пачками
        /// </summary>
        private readonly ConteinerManagement _conteiner = new ConteinerManagement(100);

        private GameServer _server;

        public TileEntityBase(GameServer server)
        {
            _server = server;
            _conteiner.SendSetSlot += _Conteiner_SendSetSlot;
        }

        /// <summary>
        /// Тут поподаем когда через Shift заносятся стаки
        /// </summary>
        private void _Conteiner_SendSetSlot(object sender, SlotEventArgs e)
        {
            // ВРЕМЕННО!!!!
            _server.Players.SendToAll(new PacketS2FSetSlot((short)e.SlotId, e.Stack));
        }

        /// <summary>
        /// Открыли окно, вызывается объектом EntityPlayerServer
        /// </summary>
        public virtual void OpenWindow(PlayerServer entityPlayer)
            => entityPlayer.SendPacket(new PacketS30WindowItems(false, _stacks));

        #region ItemStack

        /// <summary>
        /// Получить стак в конкретном слоте
        /// </summary>
        public virtual ItemStack GetStackInSlot(int slotIn) => _stacks[slotIn];

        /// <summary>
        /// Задать стак в конкретном слоте
        /// </summary>
        /// <param name="isModify">Отметить что было изменение, для перезаписи на сервере в файл сохранения</param>
        public virtual void SetStackInSlot(int slotIn, ItemStack stack)
        {
            _stacks[slotIn] = stack;
        }

        /// <summary>
        /// Добавляет стак предметов в инвентарь, возвращает false, если это невозможно.
        /// </summary>
        public virtual bool AddItemStackToInventory(ItemStack stack) 
            => CanPutItemStack(0, stack) && _conteiner.AddItemStackToInventory(_stacks, 0, stack, Count);

        /// <summary>
        /// Проверяем можно ли установить данный стак в определённой ячейке склада
        /// </summary>
        public virtual bool CanPutItemStack(int slotIn, ItemStack stack) => stack != null;

        /// <summary>
        /// Заспавнить предметы при разрушении блока
        /// </summary>
        public virtual void SpawnAsEntityOnBreakBlock() { }

        #endregion
    }
}
