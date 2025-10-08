using Mvk2.Entity.List;
using Mvk2.Games;
using Mvk2.Gui.Controls;
using Mvk2.Packets;
using Mvk2.Renderer;
using System.Runtime.CompilerServices;
using Vge.Entity.Inventory;
using Vge.Gui.Controls;
using Vge.Gui.Screens;
using Vge.Item;
using Vge.Network.Packets.Client;
using Vge.Renderer.Font;

namespace Mvk2.Gui.Screens
{
    /// <summary>
    /// Окно хранилища для игры Малювеки 2
    /// </summary>
    public abstract class ScreenStorageMvk : ScreenWindow
    {
        protected readonly WindowMvk _windowMvk;

        protected readonly RenderMvk _render;

        /// <summary>
        /// Игрок мода, на клиенте
        /// </summary>
        protected readonly PlayerClientOwnerMvk _player;

        protected readonly ControlSlot[] _slot;

        /// <summary>
        /// Иконка для предмета в воздухе
        /// </summary>
        protected readonly ControlIcon _icon;
        /// <summary>
        /// Стак который используется в перемещении из слотов, образно он в указателе мыши
        /// </summary>
        protected ItemStack _stakAir;

        public ScreenStorageMvk(WindowMvk window, int width, int height) : base(window, width, height)
        {
            _windowMvk = window;
            _render = _windowMvk.GetRender();
            _toolTip = new ToolTipMvk(window);

            _slot = new ControlSlot[_GetSlotCount()];

            _icon = new ControlIcon(_windowMvk, null);

            _player = ((GameModClientMvk)_windowMvk.Game.ModClient).Player;
            _player.InvPlayer.SlotSetted += _InvPlayer_SlotSetted;

            _Init();
        }

        /// <summary>
        /// Инициализация слотов
        /// </summary>
        protected virtual void _Init()
        {
            //ControlSlot slot;
            //for (int i = 0; i < _slot.Length; i++)
            //{
            //    slot = new ControlSlot(_windowMvk, (byte)i, 
            //        _windowMvk.Game.Player.Inventory.GetStackInSlot(i));
            //    slot.ClickLeft += (sender, e) => _SendPacket((ControlSlot)sender, false);
            //    slot.ClickRight += (sender, e) => _SendPacket((ControlSlot)sender, true);
            //    _slot[i] = slot;
            //}
        }

        protected void _SetSlot(int index, ControlSlot slot)
        {
            slot.ClickLeft += (sender, e) => _SendPacket((ControlSlot)sender, false);
            slot.ClickRight += (sender, e) => _SendPacket((ControlSlot)sender, true);
            _slot[index] = slot;
        }

        /// <summary>
        /// Количество слотов
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual int _GetSlotCount() => 0;

        /// <summary>
        /// Название заголовка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual string _GetTitle() => "Storage";

        /// <summary>
        /// Изменился слот игрока
        /// </summary>
        private void _InvPlayer_SlotSetted(object sender, SlotEventArgs e)
        {
            // Console.WriteLine("SlotSetted " + e.ToString());
            if (e.SlotId == 255)
            {
                _stakAir = e.Stack;
                _icon.SetStack(e.Stack);
            }
            else
            {
                _InvPlayerSlotSetted(e);
            }
        }

        /// <summary>
        /// Изменён слот, не воздух
        /// </summary>
        protected virtual void _InvPlayerSlotSetted(SlotEventArgs e)
        {
            if (e.SlotId < _slot.Length)
            {
                _slot[e.SlotId].SetStack(e.Stack);
            }
        }

        protected void _SendPacket(ControlSlot controlSlot, bool isRight)
        {
            // Console.WriteLine("Click " + slotId + " " + (isRight ? "Rifgt" : ""));
            if (_CheckClick(controlSlot.SlotId))
            {
                _windowMvk.Game.TrancivePacket(new PacketC0EClickWindow((byte)EnumActionClickWindow.ClickSlot,
                    _windowMvk.Game.Key.KeyShift, isRight, controlSlot.SlotId));
            }
        }

        /// <summary>
        /// Нужна проверка, ненадо отправлять в пустую ячейку если в воздухе нет предмета
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _CheckClick(byte slotId)
        {
            if (_stakAir != null) return true;
            if (slotId < 100)
            {
                // инвентарь
                return _player.Inventory.GetStackInSlot(slotId) != null;
            }
            // склад
            return true;
        }

        protected override void _InitTitle()
        {
            FontBase font = window.Render.FontMain;
            _labelTitle = new Label(window, font, 250, 50, _GetTitle());
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
                _slot[i].SetPosition(PosX + 106 + i * 50, PosY + 300);
            }
            _slot[8].SetPosition(PosX + 6, PosY + 300);

            for (int i = 0; i < 5; i++)
            {
                // Одежда
                _slot[i + 9].SetPosition(PosX + 6, PosY + 36 + i * 50);
                _slot[i + 14].SetPosition(PosX + 176, PosY + 36 + i * 50);

                // Рюкзак
                _slot[i + 19].SetPosition(PosX + 256 + i * 50, PosY + 45);
                _slot[i + 24].SetPosition(PosX + 256 + i * 50, PosY + 95);
                _slot[i + 29].SetPosition(PosX + 256 + i * 50, PosY + 145);
                _slot[i + 34].SetPosition(PosX + 256 + i * 50, PosY + 195);
                _slot[i + 39].SetPosition(PosX + 256 + i * 50, PosY + 245);
            }
        }

        /// <summary>
        /// Клик за пределами окна
        /// </summary>
        protected override void _OnClickOutsideWindow() => _ThrowTheSlot();

        /// <summary>
        /// Выбросить слот который в руке
        /// </summary>
        protected virtual void _ThrowTheSlot()
        {
            if (_stakAir != null)
            {
                _windowMvk.Game.TrancivePacket(
                    new PacketC0EClickWindow((byte)EnumActionClickWindow.ThrowOutAir));
            }
        }

        /// <summary>
        /// Происходит перед закрытием окна
        /// </summary>
        protected virtual void _OnFinishing()
            // ?. - неужна для защиты, если сервер дисконект, и выкинули из игры
            => _windowMvk.Game?.TrancivePacket(new PacketC0EClickWindow((byte)EnumActionClickWindow.Close));
        
        public override void Dispose()
        {
            base.Dispose();
            _OnFinishing();
            _player.InvPlayer.SlotSetted -= _InvPlayer_SlotSetted;
        }
    }
}
