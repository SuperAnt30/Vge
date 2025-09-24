using Mvk2.Entity.List;
using Mvk2.Games;
using Mvk2.Gui.Controls;
using Mvk2.Packets;
using Mvk2.Renderer;
using System;
using Vge.Entity.Inventory;
using Vge.Entity.Render;
using Vge.Gui.Controls;
using Vge.Gui.Screens;
using Vge.Item;
using Vge.Network.Packets.Client;
using Vge.Renderer.Font;
using WinGL.Util;

namespace Mvk2.Gui.Screens
{
    /// <summary>
    /// Окно инвентаря для игры Малювеки 2
    /// </summary>
    public class ScreenInventoryMvk : ScreenWindow
    {
        private readonly WindowMvk _windowMvk;

        protected readonly RenderMvk _render;

        /// <summary>
        /// Игрок мода, на клиенте
        /// </summary>
        private readonly PlayerClientOwnerMvk _player;

        private readonly ControlSlot[] _slot;

        /// <summary>
        /// Иконка для предмета в воздухе
        /// </summary>
        private readonly ControlIcon _icon;
        /// <summary>
        /// Стак который используется в перемещении из слотов, образно он в указателе мыши
        /// </summary>
        private ItemStack _stakAir;

        public ScreenInventoryMvk(WindowMvk window) : base(window, 512, 354)
        {
            _windowMvk = window;
            _render = _windowMvk.GetRender();
            _slot = new ControlSlot[19];

            _icon = new ControlIcon(_windowMvk, null);

            ControlSlot slot;
            for (int i = 0; i < _slot.Length; i++)
            {
                slot = new ControlSlot(window, (byte)i,
                    _windowMvk.Game.Player.Inventory.GetStackInSlot(i));
                slot.ClickLeft += Slot_ClickLeft;
                slot.ClickRight += Slot_ClickRight;
                _slot[i] = slot;
            }
            _player = ((GameModClientMvk)_windowMvk.Game.ModClient).Player;
            _player.InvPlayer.SlotSetted += InvPlayer_SlotSetted;
        }

        /// <summary>
        /// Изменился слот игрока
        /// </summary>
        private void InvPlayer_SlotSetted(object sender, SlotEventArgs e)
        {
            if (e.SlotId == 255)
            {
                _stakAir = e.Stack;
                _icon.SetStack(e.Stack);
            }
            else
            {
                if (e.SlotId < _slot.Length)
                {
                    _slot[e.SlotId].SetStack(e.Stack);
               }
            }
        }

        private void Slot_ClickRight(object sender, System.EventArgs e)
            => _SendPacket(((ControlSlot)sender).SlotId, true);

        private void Slot_ClickLeft(object sender, System.EventArgs e)
            => _SendPacket(((ControlSlot)sender).SlotId, false);

        private void _SendPacket(byte slotId, bool isRight)
        {
            // Нужна проверка, ненадо отправлять в пустую ячейку если в воздухе нет предмета
            if (_player.Inventory.GetStackInSlot(slotId) != null || _stakAir != null)
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
            for (int i = 0; i < _slot.Length; i++)
            {
                AddControls(_slot[i]);
            }
            AddControls(_icon);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            // Расположение окна
            PosX = (Width - WidthWindow) / 2;
            PosY = (Height - HeightWindow) / 2;
            //PosY = 10;
            //PosX = 10;
            
            base.OnResized();
            _labelTitle.SetPosition(PosX + 16, PosY + 10);
            _buttonCancel.SetPosition(PosX + WidthWindow - 50, PosY);

            for (int i = 0; i < 8; i++)
            {
                _slot[i].SetPosition(PosX + 56 + i * 50, PosY + 300);
            }
            _slot[8].SetPosition(PosX + 56, PosY + 250);
            for (int i = 0; i < 10; i++)
            {
                _slot[i + 9].SetPosition(PosX + 6 + i * 50, PosY + 200);
            }
        }

        /// <summary>
        /// Запустить текстуру фона
        /// </summary>
        protected override void _BindTextureBg() => _windowMvk.GetRender().BindTextureInventory();

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            base.Draw(timeIndex);

            // Прорисовки игрока
            if (window.Game.Player.Render is EntityRenderAnimation renderAnimation)
            {
                int y = (PosY + 80) * si;
                int x = (PosX + 115) * si;
                float pitch = y > window.MouseY ? (1f - window.MouseY / (float)y)
                    : -(window.MouseY - y) / (float)(Gi.Height - y);
                float yaw = x > window.MouseX ? (1f - window.MouseX / (float)x)
                    : -(window.MouseX - x) / (float)(Gi.Width - x);

                window.Game.Render.DepthOn();
                renderAnimation.DrawGui((PosX + 115) * si, (PosY + 190) * si,
                    Glm.Sin(yaw), Glm.Sin(pitch), 36 * si);
                window.Game.Render.DepthOff();
            }
        }
    }
}
