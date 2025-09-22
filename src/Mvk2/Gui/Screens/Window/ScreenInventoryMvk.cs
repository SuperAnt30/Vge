using Mvk2.Gui.Controls;
using Mvk2.Packets;
using Vge.Entity.Inventory;
using Vge.Gui.Controls;
using Vge.Gui.Screens;
using Vge.Item;
using Vge.Network.Packets.Client;
using Vge.Renderer.Font;

namespace Mvk2.Gui.Screens
{
    /// <summary>
    /// Окно инвентаря для игры Малювеки 2
    /// </summary>
    public class ScreenInventoryMvk : ScreenWindow
    {
        private readonly WindowMvk _windowMvk;

        private readonly ControlSlot[] _slot;

        /// <summary>
        /// Стак который используется в перемещении из слотов, образно он в указателе мыши
        /// </summary>
        private ItemStack _stakAir;

        public ScreenInventoryMvk(WindowMvk window) : base(window, 512, 354)
        {
            _windowMvk = window;
            _slot = new ControlSlot[8];

            ControlSlot slot;
            for (int i = 0; i < 8; i++)
            {
                slot = new ControlSlot(window, (byte)i,
                    _windowMvk.Game.Player.Inventory.GetStackInSlot(i));
                slot.ClickLeft += Slot_ClickLeft;
                slot.ClickRight += Slot_ClickRight;
                _slot[i] = slot;
            }
        }

        private void Slot_ClickRight(object sender, System.EventArgs e)
            => _SendPacket(((ControlSlot)sender).SlotId, true);

        private void Slot_ClickLeft(object sender, System.EventArgs e)
            => _SendPacket(((ControlSlot)sender).SlotId, false);

        private void _SendPacket(byte slotId, bool isRight)
        {
            // Нужна проверка, ненадо отправлять в пустую ячейку если в воздухе нет предмета
            if (_windowMvk.Game.Player.Inventory.GetStackInSlot(slotId) != null || _stakAir != null)
            {
                _windowMvk.Game.TrancivePacket(new PacketC0EClickWindow(
                    (byte)EnumActionClickWindow.ClickSlot,
                    _windowMvk.Game.Key.KeyShift,
                    isRight,
                    slotId));
            }
        }

        protected override void _InitTitle()
        {
            FontBase font = window.Render.FontMain;
            _labelTitle = new Label(window, font, 250, 50, L.T("Inventory"));
            _labelTitle.SetTextAlight(EnumAlight.Left, EnumAlightVert.Top);
            _buttonCancel = new Button(window, font, 50, "X");
        }

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            for (int i = 0; i < 8; i++)
            {
                AddControls(_slot[i]);
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
            base.OnResized();
            _labelTitle.SetPosition(PosX + 16, PosY + 10);
            _buttonCancel.SetPosition(PosX + WidthWindow - 50, PosY);

            for (int i = 0; i < 8; i++)
            {
                _slot[i].SetPosition(PosX + 56 + i * 50, PosY + 300);
            }
        }

        /// <summary>
        /// Запустить текстуру фона
        /// </summary>
        protected override void _BindTextureBg() => _windowMvk.GetRender().BindTextureInventory();
    }
}
