using Mvk2.Gui.Controls;
using Mvk2.Packets;
using Vge.Entity.Inventory;
using Vge.Network;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.TileEntity;
using WinGL.Actions;

namespace Mvk2.Gui.Screens
{
    /// <summary>
    /// Окно инвентаря для игры Малювеки 2
    /// </summary>
    public class ScreenStorageDebugMvk : ScreenStorageMvk
    {
        public ScreenStorageDebugMvk(WindowMvk window) : base(window, 512, 354)
        {
            _windowMvk.Game.TrancivePacket(new PacketC0EClickWindow((byte)EnumActionClickWindow.OpenBoxDebug));
        }

        /// <summary>
        /// Инициализация слотов
        /// </summary>
        protected override void _Init()
        {
            // Быстрый выбор плюс правая рука
            for (int i = 0; i < 9; i++)
            {
                _SetSlot(i, new ControlSlot(_windowMvk, (byte)i, 
                    _windowMvk.Game.Player.Inventory.GetStackInSlot(i)));
            }

            // Рюкзак
            for (int i = 0; i < 25; i++)
            {
                _SetSlot(i + 9, new ControlSlot(_windowMvk, (byte)(i + 19),
                    _windowMvk.Game.Player.Inventory.GetStackInSlot(i + 19)));
                if (i >= _player.InvPlayer.LimitBackpack)
                {
                    _slot[i + 9].SetEnable(false);
                }
            }

            // Ящик
            for (int i = 0; i < TileEntityBase.Count; i++)
            {
                _SetSlot(i + 34, new ControlSlot(_windowMvk, (byte)(i + 100), null));
                _slot[i + 34].SetEnable(false);
                //_windowMvk.Game.Player.Inventory.GetStackInSlot(i + 100)));
            }
        }

        private void _SetSlot(int index, ControlSlot slot)
        {
            slot.ClickLeft += (sender, e) => _SendPacket(((ControlSlot)sender).SlotId, false);
            slot.ClickRight += (sender, e) => _SendPacket(((ControlSlot)sender).SlotId, true);
            _slot[index] = slot;
        }

        /// <summary>
        /// Изменён слот, не воздух
        /// </summary>
        protected override void _InvPlayerSlotSetted(SlotEventArgs e)
        {
            if (e.SlotId < 9)
            {
                // Быстрый выбор плюс правая рука
                _slot[e.SlotId].SetStack(e.Stack);
            }
            else if (e.SlotId >= 19 && e.SlotId < 44)
            {
                // Рюкзак
                _slot[e.SlotId - 10].SetStack(e.Stack);
            }
            else if (e.SlotId >= 100)
            {
                // Ящик
                _slot[e.SlotId - 100 + 34].SetStack(e.Stack);
            }
        }

        /// <summary>
        /// Количество слотов
        /// </summary>
        protected override int _GetSlotCount() => 9 + 25 + TileEntityBase.Count;

        /// <summary>
        /// Название заголовка
        /// </summary>
        protected override string _GetTitle() => L.T("Debug");

        /// <summary>
        /// Получить сетевой пакет
        /// </summary>
        public override void AcceptNetworkPackage(IPacket packet)
        {
            if (packet is PacketS2FSetSlot packetS2F)
            {
                // Изменился один слот
                _slot[packetS2F.SlotId - 100 + 34].SetStack(packetS2F.Stack);
            }
            else if (packet is PacketS30WindowItems packetS30)
            {
                // Загрузить все слоты в ящик
                for (int i = 34; i < 34 + TileEntityBase.Count; i++)
                {
                    _slot[i].SetStack(packetS30.Stacks[i - 34]);
                    _slot[i].SetEnable(true);
                }
            }
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            // Расположение окна
            PosX = (Width - WidthWindow) / 2;
            PosY = (Height - HeightWindow) / 2;
            
            _labelTitle.SetPosition(PosX + 16, PosY + 10);
            _buttonCancel.SetPosition(PosX + WidthWindow - 50, PosY);

            for (int i = 0; i < 8; i++)
            {
                _slot[i].SetPosition(PosX + 106 + i * 50, PosY + 300);
            }
            _slot[8].SetPosition(PosX + 6, PosY + 300);

            for (int i = 0; i < 5; i++)
            {
                // Одежда
                //_slot[i + 9].SetPosition(PosX + 6, PosY + 36 + i * 50);
                //_slot[i + 14].SetPosition(PosX + 176, PosY + 36 + i * 50);

                // Рюкзак
                _slot[i + 9].SetPosition(PosX + 256 + i * 50, PosY + 45);
                _slot[i + 14].SetPosition(PosX + 256 + i * 50, PosY + 95);
                _slot[i + 19].SetPosition(PosX + 256 + i * 50, PosY + 145);
                _slot[i + 24].SetPosition(PosX + 256 + i * 50, PosY + 195);
                _slot[i + 29].SetPosition(PosX + 256 + i * 50, PosY + 245);

                // Ящик
                _slot[i + 34].SetPosition(PosX + 6, PosY + 45 + i * 50);
                //_slot[i + 39].SetPosition(PosX + 56, PosY + 45 + i * 50);
                //_slot[i + 44].SetPosition(PosX + 106, PosY + 45 + i * 50);
                //_slot[i + 49].SetPosition(PosX + 156, PosY + 45 + i * 50);
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
        protected override void _BindTextureBg() => _windowMvk.GetRender().BindTextureConteinerStorage();

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            base.Draw(timeIndex);
            _toolTip.Draw();
        }

        //public override void Dispose()
        //{
        //    base.Dispose();
        //}
    }
}
