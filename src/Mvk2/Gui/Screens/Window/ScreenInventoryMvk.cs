using Mvk2.Gui.Controls;
using Mvk2.Packets;
using System;
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
            _player.InvPlayer.LimitBackpackChanged += _InvPlayer_LimitBackpackChanged;

            _windowMvk.Game.TrancivePacket(new PacketC0EClickWindow((byte)EnumActionClickWindow.OpenInventory));
            _UpBackpackEnabled();
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

            // Одежда
            for (int i = 0; i < 10; i++)
            {
                _SetSlot(i + 9, new ControlSlot(_windowMvk, (byte)(i + 9), 
                    _windowMvk.Game.Player.Inventory.GetStackInSlot(i + 9)));
            }

            // Рюкзак
            for (int i = 0; i < 25; i++)
            {
                _SetSlot(i + 19, new ControlSlot(_windowMvk, (byte)(i + 19), 
                    _windowMvk.Game.Player.Inventory.GetStackInSlot(i + 19)));
                if (i >= _player.InvPlayer.LimitBackpack)
                {
                    _slot[i + 19].SetEnable(false);
                }
            }
        }

        /// <summary>
        /// Количество слотов
        /// </summary>
        protected override int _GetSlotCount() => 19 + 25;
        /// <summary>
        /// Название заголовка
        /// </summary>
        protected override string _GetTitle() => L.T("Inventory");

        /// <summary>
        /// Обновить слоты рюкзака
        /// </summary>
        private void _UpBackpackEnabled()
        {
            int chek = _player.InvPlayer.LimitBackpack + 19;
            for (int i = 19; i < 44; i++)
            {
                bool b = _slot[i].Enabled;
                if (b != chek > i)
                {
                    _slot[i].SetEnable(!b);
                }
            }
        }

        /// <summary>
        /// Событие изменён лимит рюкзака
        /// </summary>
        private void _InvPlayer_LimitBackpackChanged(object sender, EventArgs e)
            => _UpBackpackEnabled();

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
            _player.InvPlayer.LimitBackpackChanged -= _InvPlayer_LimitBackpackChanged;
        }
    }
}
