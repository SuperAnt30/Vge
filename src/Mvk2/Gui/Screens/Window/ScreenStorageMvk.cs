using Mvk2.Entity.Inventory;
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
        /// Количество ячеек карманов, не меньше 2 (_requiredPocket)
        /// </summary>
        protected readonly byte _pocketCount;
        /// <summary>
        /// Количество ячеек одежды, первый слот это предмет левой руки 
        /// </summary>
        protected readonly byte _clothCount;
        /// <summary>
        /// Количество ячеек рюкзака
        /// </summary>
        protected readonly byte _backpackCount;
        /// <summary>
        /// Количество ячеек инвентаря, т.е. карман + рюкзак + правая рука
        /// </summary>
        protected readonly byte _inventoryCount;

        /// <summary>
        /// Стак который используется в перемещении из слотов, образно он в указателе мыши
        /// </summary>
        protected ItemStack _stakAir;

        public ScreenStorageMvk(WindowMvk window, int width, int height) : base(window, width, height)
        {
            _pocketCount = InventoryPlayerMvk.PocketCount;
            _clothCount = InventoryPlayerMvk.ClothCount;
            _backpackCount = InventoryPlayerMvk.BackpackCount;
            _inventoryCount = (byte)(_pocketCount + _backpackCount + 1);

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
        /// Инициализация слотов. base._Init() Вызываем только для карманов и рюкзака
        /// </summary>
        protected virtual void _Init()
        {
            // Данная инициализация, для карманов и рюкзака, для другий можно просто перенаследовать

            // Карманы
            for (byte i = 0; i < _pocketCount; i++)
            {
                _SetSlot(i, new ControlSlot(_windowMvk, i,
                    _windowMvk.Game.Player.Inventory.GetStackInSlot(i)));
                if (i >= _player.InvPlayer.LimitPocket)
                {
                    _slot[i].SetEnable(false);
                }
            }

            // Правая рука
            _SetSlot(_pocketCount, new ControlSlot(_windowMvk, _pocketCount,
                    _windowMvk.Game.Player.Inventory.GetStackInSlot(_pocketCount)));

            // Рюкзак
            int from = _pocketCount + 1; // плюс правая рука
            int count = from + _backpackCount;
            int bias = _clothCount - 1; // без правой руки
            for (int i = from; i < count; i++)
            {
                _SetSlot(i, new ControlSlot(_windowMvk, (byte)(i + bias),
                    _windowMvk.Game.Player.Inventory.GetStackInSlot(i + bias)));
                if (i - from >= _player.InvPlayer.LimitBackpack)
                {
                    _slot[i].SetEnable(false);
                }
            }
        }

        /// <summary>
        /// Задать слот в конкретную ячейку массива, и присвоить им отклики событий
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        /// Изменён слот, не воздух. Наследовать не надо если работа со складом
        /// </summary>
        protected virtual void _InvPlayerSlotSetted(SlotEventArgs e)
        {
            if (e.SlotId <= _pocketCount)
            {
                // Карманы плюс правая рука
                _slot[e.SlotId].SetStack(e.Stack);
            }
            else if (e.SlotId >= (_pocketCount + _clothCount)
                && e.SlotId < (_pocketCount + _clothCount + _backpackCount))
            {
                // Рюкзак
                _slot[e.SlotId - _pocketCount - 2].SetStack(e.Stack);
            }
            else if (e.SlotId >= 100)
            {
                // Ящик
                _slot[e.SlotId - 100 + _inventoryCount].SetStack(e.Stack);
            }
        }

        protected void _SendPacket(ControlSlot controlSlot, bool isRight)
        {
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
            base.OnResized();

            // Расположение окна
            PosX = (Width - WidthWindow) / 2;
            PosY = (Height - HeightWindow) / 2;
            //PosY = 10;
            //PosX = 10;
            
            _labelTitle.SetPosition(PosX + 16, PosY + 10);
            _buttonCancel.SetPosition(PosX + WidthWindow - 50, PosY);
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
