﻿using Mvk2.Entity.Inventory;
using Mvk2.Entity.List;
using Mvk2.Games;
using Mvk2.Packets;
using Mvk2.Renderer;
using System;
using System.Runtime.CompilerServices;
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
    /// Окно хранилища
    /// </summary>
    public abstract class ScreenStorage : ScreenWindow
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

        public ScreenStorage(WindowMvk window) : base(window, 512f, 456, 420)
        {
            _pocketCount = InventoryPlayerMvk.PocketCount;
            _clothCount = InventoryPlayerMvk.ClothCount;
            _backpackCount = InventoryPlayerMvk.BackpackCount;
            _inventoryCount = (byte)(_pocketCount + _clothCount + _backpackCount);

            _windowMvk = window;
            _render = _windowMvk.GetRender();
            _toolTip = new ToolTipMvk(window);

            _slot = new ControlSlot[_GetSlotCount()];

            _icon = new ControlIcon(_windowMvk, _render.FontSmall, null);

            _player = ((GameModClientMvk)_windowMvk.Game.ModClient).Player;
            _player.InvPlayer.SlotSetted += _InvPlayer_SlotSetted;
            _player.InvPlayer.LimitPocketChanged += _InvPlayer_LimitPocketChanged;
            _player.InvPlayer.LimitBackpackChanged += _InvPlayer_LimitBackpackChanged;

            _Init();

            _UpPocketEnabled();
            _UpBackpackEnabled();
        }

        /// <summary>
        /// Инициализация слотов, инвентарь.
        /// </summary>
        protected virtual void _Init()
        {
            // Данная инициализация, для карманов и рюкзака, для другий можно просто перенаследовать
            for (byte i = 0; i < _inventoryCount; i++)
            {
                _SetSlot(i, new ControlSlot(_windowMvk, _render.FontSmall, i,
                    _windowMvk.Game.Player.Inventory.GetStackInSlot(i)));
            }
        }

        /// <summary>
        /// Обновить слоты карманов
        /// </summary>
        private void _UpPocketEnabled()
        {
            int chek = _player.InvPlayer.LimitPocket;
            for (int i = 0; i < _pocketCount; i++)
            {
                bool b = _slot[i].Enabled;
                if (b != chek > i)
                {
                    _slot[i].SetEnable(!b);
                }
            }
        }

        /// <summary>
        /// Обновить слоты рюкзака
        /// </summary>
        private void _UpBackpackEnabled()
        {
            int from = _pocketCount + _clothCount;
            int count = _inventoryCount;
            int chek = _player.InvPlayer.LimitBackpack + from;
            for (int i = from; i < count; i++)
            {
                bool b = _slot[i].Enabled;
                if (b != chek > i)
                {
                    _slot[i].SetEnable(!b);
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
        protected virtual int _GetSlotCount() => _inventoryCount;

        /// <summary>
        /// Название заголовка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual string _GetTitle() => "Storage";

        /// <summary>
        /// Событие изменён лимит кармана
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _InvPlayer_LimitPocketChanged(object sender, EventArgs e)
            => _UpPocketEnabled();

        /// <summary>
        /// Событие изменён лимит рюкзака
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _InvPlayer_LimitBackpackChanged(object sender, EventArgs e)
            => _UpBackpackEnabled();

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
            if (e.SlotId < _inventoryCount)
            {
                // Инвентарь
                _slot[e.SlotId].SetStack(e.Stack);
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
            _buttonCancel = new Button(window, font, 30, "X", 28);
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
            int h = Height - HeightWindow;
            PosY = h > 160 ? h / 2 : (h > 80 ? h - 80 : 0); // Условие, чтоб снизу всегда было видно 80
            
            _labelTitle.SetPosition(PosX + 16, PosY + 10);
            _buttonCancel.SetPosition(PosX + WidthWindow - 50, PosY);

            int i;
            // Карманы
            for (i = 0; i < _pocketCount; i++)
            {
                _slot[i].SetPosition(PosX + 12 + i * 36, PosY + 372);
            }

            // Одежда
            for (i = 0; i < 5; i++)
            {
                _slot[i + _pocketCount].SetPosition(PosX + 12, PosY + 188 + i * 36);
                _slot[i + _pocketCount + 5].SetPosition(PosX + 152, PosY + 188 + i * 36);
            }

            // Рюкзак
            int from = _pocketCount + _clothCount;
            i = 0;
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 7; x++)
                {
                    _slot[i + from].SetPosition(PosX + 192 + x * 36, PosY + 188 + y * 36);
                    i++;
                }
            }
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            base.Draw(timeIndex);
            _toolTip.Draw();
        }

        /// <summary>
        /// Дополнительная прорисовка не контролов
        /// </summary>
        protected override void _DrawAdd()
        {
            base._DrawAdd();
            // Прорисовки игрока
            if (window.Game.Player.Render is EntityRenderAnimation renderAnimation)
            {
                int y = (PosY + 240) * si;
                int x = (PosX + 102) * si;

                float pitch = y > window.MouseY ? (1f - window.MouseY / (float)y)
                    : -(window.MouseY - y) / (float)(Gi.Height - y);
                float yaw = x > window.MouseX ? (1f - window.MouseX / (float)x)
                    : -(window.MouseX - x) / (float)(Gi.Width - x);

                window.Game.Render.DepthOn();
                renderAnimation.DrawGui((PosX + 102) * si, (PosY + 350) * si,
                    Glm.Sin(yaw), Glm.Sin(pitch), 36 * si);
                window.Game.Render.DepthOff();
                window.Render.ShaderBindGuiColor();
            }
            //window.Render.BindTextureWidgets();
        }

        /// <summary>
        /// Клик за пределами окна
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            _player.InvPlayer.LimitPocketChanged -= _InvPlayer_LimitPocketChanged;
            _player.InvPlayer.LimitBackpackChanged -= _InvPlayer_LimitBackpackChanged;
        }
    }
}
