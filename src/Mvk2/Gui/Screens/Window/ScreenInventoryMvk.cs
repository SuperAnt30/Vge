using Mvk2.Gui.Controls;
using Mvk2.Packets;
using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Inventory;
using Vge.Entity.Render;
using Vge.Network.Packets.Client;
using WinGL.Actions;
using WinGL.Util;

namespace Mvk2.Gui.Screens
{
    /// <summary>
    /// Окно инвентаря для игры Малювеки 2
    /// </summary>
    public class ScreenInventoryMvk : ScreenStorageMvk
    {
        public ScreenInventoryMvk(WindowMvk window) : base(window, 512, 354)
        {
            _player.InvPlayer.LimitPocketChanged += _InvPlayer_LimitPocketChanged;
            _player.InvPlayer.LimitBackpackChanged += _InvPlayer_LimitBackpackChanged;

            _windowMvk.Game.TrancivePacket(new PacketC0EClickWindow((byte)EnumActionClickWindow.OpenInventory));
            _UpPocketEnabled();
            _UpBackpackEnabled();
        }

        /// <summary>
        /// Инициализация слотов
        /// </summary>
        protected override void _Init()
        {
            int count = _GetSlotCount();

            for (byte i = 0; i < count; i++)
            {
                _SetSlot(i, new ControlSlot(_windowMvk, i,
                    _windowMvk.Game.Player.Inventory.GetStackInSlot(i)));
            }
        }

        /// <summary>
        /// Количество слотов
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override int _GetSlotCount() => _pocketCount + _clothCount + _backpackCount;
        /// <summary>
        /// Название заголовка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override string _GetTitle() => L.T("Inventory");

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
            int count = _GetSlotCount();
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
        /// Событие изменён лимит кармана
        /// </summary>
        private void _InvPlayer_LimitPocketChanged(object sender, EventArgs e)
            => _UpPocketEnabled();

        /// <summary>
        /// Событие изменён лимит рюкзака
        /// </summary>
        private void _InvPlayer_LimitBackpackChanged(object sender, EventArgs e)
            => _UpBackpackEnabled();

        /// <summary>
        /// Изменён слот, не воздух
        /// </summary>
        protected override void _InvPlayerSlotSetted(SlotEventArgs e)
        {
            if (e.SlotId < _slot.Length)
            {
                _slot[e.SlotId].SetStack(e.Stack);
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

            // Одежда
            int from = _pocketCount + 1;
            for (int i = 0; i < 4; i++)
            {
                
                _slot[i + from].SetPosition(PosX + 6, PosY + 36 + i * 50);
                _slot[i + from + 4].SetPosition(PosX + 176, PosY + 36 + i * 50);
            }

            // Рюкзак
            from = _pocketCount + _clothCount;
            for (int i = 0; i < 5; i++)
            {
                
                _slot[i + from].SetPosition(PosX + 256 + i * 50, PosY + 45);
                _slot[i + from + 5].SetPosition(PosX + 256 + i * 50, PosY + 95);
                _slot[i + from + 10].SetPosition(PosX + 256 + i * 50, PosY + 145);
            }

            //for (int i = 0; i < 5; i++)
            //{
            //    // Одежда
            //    _slot[i + 9].SetPosition(PosX + 6, PosY + 36 + i * 50);
            //    _slot[i + 14].SetPosition(PosX + 176, PosY + 36 + i * 50);

            //    // Рюкзак
            //    _slot[i + 19].SetPosition(PosX + 256 + i * 50, PosY + 45);
            //    _slot[i + 24].SetPosition(PosX + 256 + i * 50, PosY + 95);
            //    _slot[i + 29].SetPosition(PosX + 256 + i * 50, PosY + 145);
            //    _slot[i + 34].SetPosition(PosX + 256 + i * 50, PosY + 195);
            //    _slot[i + 39].SetPosition(PosX + 256 + i * 50, PosY + 245);
            //}
        }

        public override void OnKeyDown(Keys keys)
        {
            base.OnKeyDown(keys);

            if (keys == Keys.E)
            {
                _Close();
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

            _toolTip.Draw();
        }

        public override void Dispose()
        {
            base.Dispose();
            _player.InvPlayer.LimitPocketChanged -= _InvPlayer_LimitPocketChanged;
            _player.InvPlayer.LimitBackpackChanged -= _InvPlayer_LimitBackpackChanged;
        }
    }
}
