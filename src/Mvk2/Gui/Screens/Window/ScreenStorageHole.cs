using Mvk2.Gui.Controls;
using Mvk2.Packets;
using System.Runtime.CompilerServices;
using Vge.Network;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.TileEntity;
using WinGL.Actions;

namespace Mvk2.Gui.Screens
{
    /// <summary>
    /// Окно дыры хранения для игры Малювеки 2
    /// </summary>
    public class ScreenStorageHole : ScreenStorage
    {
        public ScreenStorageHole(WindowMvk window) : base(window)
            => _windowMvk.Game.TrancivePacket(new PacketC0EClickWindow((byte)EnumActionClickWindow.OpenBoxDebug));

        /// <summary>
        /// Инициализация слотов
        /// </summary>
        protected override void _Init()
        {
            base._Init();

            // Ящик
            for (int i = 0; i < TileEntityHole.Count; i++)
            {
                _SetSlot(i + _inventoryCount, new ControlSlot(_windowMvk, (byte)(i + 100), null));
                _slot[i + _inventoryCount].SetEnable(false);
            }
        }

        /// <summary>
        /// Название заголовка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override string _GetTitle() => L.T("Hole");

        /// <summary>
        /// Количество слотов
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override int _GetSlotCount() => _inventoryCount + TileEntityHole.Count;

        /// <summary>
        /// Получить сетевой пакет
        /// </summary>
        public override void AcceptNetworkPackage(IPacket packet)
        {
            if (packet is PacketS2FSetSlot packetS2F)
            {
                // Изменился один слот (из вне поменялся, образно другой игрок)
                _slot[packetS2F.SlotId - 100 + _inventoryCount].SetStack(packetS2F.Stack);
            }
            else if (packet is PacketS30WindowItems packetS30)
            {
                // Загрузить все слоты в ящик
                for (int i = _inventoryCount; i < _inventoryCount + TileEntityHole.Count; i++)
                {
                    _slot[i].SetStack(packetS30.Stacks[i - _inventoryCount]);
                    _slot[i].SetEnable(true);
                }
            }
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            base.OnResized();

            // Ящик на 48 слот
            int i = 0;
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 12; x++)
                {
                    _slot[i + _inventoryCount].SetPosition(PosX + 11 + x * 36, PosY + 32 + y * 36);
                    i++;
                }
            }
        }

        public override void OnKeyDown(Keys keys)
        {
            base.OnKeyDown(keys);

            if (keys == Keys.R)
            {
                _Close();
            }
        }

        /// <summary>
        /// Запустить текстуру фона
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _BindTextureBg() => _windowMvk.GetRender().BindTextureConteinerStorage();
    }
}
