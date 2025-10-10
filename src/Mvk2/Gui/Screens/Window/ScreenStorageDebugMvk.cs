using Mvk2.Gui.Controls;
using Mvk2.Packets;
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
            base._Init();

            // Ящик
            for (int i = 0; i < TileEntityBase.Count; i++)
            {
                _SetSlot(i + _inventoryCount, new ControlSlot(_windowMvk, (byte)(i + 100), null));
                _slot[i + _inventoryCount].SetEnable(false);
            }
        }

        /// <summary>
        /// Количество слотов
        /// </summary>
        protected override int _GetSlotCount() => _inventoryCount + TileEntityBase.Count;

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
                _slot[packetS2F.SlotId - 100 + _inventoryCount].SetStack(packetS2F.Stack);
            }
            else if (packet is PacketS30WindowItems packetS30)
            {
                // Загрузить все слоты в ящик
                for (int i = _inventoryCount; i < _inventoryCount + TileEntityBase.Count; i++)
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

            // Карманы
            for (int i = 0; i < _pocketCount; i++)
            {
                _slot[i].SetPosition(PosX + 106 + i * 50, PosY + 300);
            }
            // Правая рука
            _slot[_pocketCount].SetPosition(PosX + 6, PosY + 300);

            // Рюкзак
            int from = _pocketCount + 1;
            for (int i = 0; i < 5; i++)
            {

                _slot[i + from].SetPosition(PosX + 256 + i * 50, PosY + 45);
                _slot[i + from + 5].SetPosition(PosX + 256 + i * 50, PosY + 95);
                _slot[i + from + 10].SetPosition(PosX + 256 + i * 50, PosY + 145);
            }

            for (int i = 0; i < 5; i++)
            {
                // Ящик
               _slot[i + _inventoryCount].SetPosition(PosX + 6, PosY + 45 + i * 50);
            }

            //for (int i = 0; i < 5; i++)
            //{
            //    // Рюкзак
            //    //_slot[i + 9].SetPosition(PosX + 256 + i * 50, PosY + 45);
            //    //_slot[i + 14].SetPosition(PosX + 256 + i * 50, PosY + 95);
            //    //_slot[i + 19].SetPosition(PosX + 256 + i * 50, PosY + 145);
            //    //_slot[i + 24].SetPosition(PosX + 256 + i * 50, PosY + 195);
            //    //_slot[i + 29].SetPosition(PosX + 256 + i * 50, PosY + 245);

            //    // Ящик
            //  // _slot[i + 34].SetPosition(PosX + 6, PosY + 45 + i * 50);
            //    //_slot[i + 39].SetPosition(PosX + 56, PosY + 45 + i * 50);
            //    //_slot[i + 44].SetPosition(PosX + 106, PosY + 45 + i * 50);
            //    //_slot[i + 49].SetPosition(PosX + 156, PosY + 45 + i * 50);
            //}
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
    }
}
