using System;
using System.Runtime.CompilerServices;
using Vge.Games;
using Vge.Gui.Controls;
using Vge.Realms;
using Vge.Renderer.Font;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран выбора одиночной игры
    /// </summary>
    public class ScreenSingle : ScreenWindow
    {
        private const int _countSlot = ListSingleGame.CountSlot;
        /// <summary>
        /// Список одиночных игр
        /// </summary>
        private readonly ListSingleGame _listSingle = new ListSingleGame();

        /// <summary>
        /// Выбранный слот
        /// </summary>
        private int _slot = -1;

        protected readonly Button[] _buttonSlots = new Button[_countSlot];
        protected readonly ButtonRemove[] _buttonSlotsDel = new ButtonRemove[_countSlot];
        protected readonly Button _buttonMenu;

        public ScreenSingle(WindowMain window) : base(window, 512f, 512, 416, true)
        {
            _listSingle.Initialize();
            FontBase font = window.Render.FontMain;
            _buttonMenu = new ButtonThin(window, font, 128, L.T("Menu"));
            _buttonMenu.Click += _Button_Click;

            bool empty = false;
            for (int i = 0; i < _countSlot; i++)
            {
                
                _buttonSlots[i] = new ButtonThin(window, font, 356, "");
                _buttonSlotsDel[i] = new ButtonRemove(window);
                _buttonSlots[i].Tag = i; // Номер слота
                _buttonSlots[i].Click += _ButtonSlots_Click;
                _buttonSlotsDel[i].Tag = i; // Номер слота
                _buttonSlotsDel[i].Click += _ButtonSlotsDel_Click;
                _SlotInit(i);
                if (empty)
                {
                    if (_listSingle.EmptyWorlds[i])
                    {
                        _buttonSlots[i].SetEnable(false);
                    }
                }
                else if (_listSingle.EmptyWorlds[i])
                {
                    empty = true;
                }
            }
        }

        /// <summary>
        /// Название заголовка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override string _GetTitle() => L.T("Singleplayer");

        /// <summary>
        /// Закрытие скрина
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _Close() => window.LScreen.MainMenu();

        /// <summary>
        /// Клик за пределами окна
        /// </summary>
        protected override void _OnClickOutsideWindow() { }

        private void _SlotInit(int slot)
        {
            _buttonSlots[slot].SetText(ChatStyle.Bolb + _listSingle.NameWorlds[slot] + ChatStyle.Reset);
            _buttonSlotsDel[slot].SetVisible(!_listSingle.EmptyWorlds[slot]);
        }

        private void _Button_Click(object sender, EventArgs e) => _Close();

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void _OnInitialize()
        {
            base._OnInitialize();
            for (int i = 0; i < _countSlot; i++)
            {
                _AddControls(_buttonSlots[i]);
                _AddControls(_buttonSlotsDel[i]);
            }
            _AddControls(_buttonMenu);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void _OnResized()
        {
            // Расположение окна
            PosX = (Width - WidthWindow) / 2;
            PosY = (Height - HeightWindow) / 2;
            base._OnResized();

            int h = 64;
            for (int i = 0; i < _countSlot; i++)
            {
                _buttonSlots[i].SetPosition(PosX + 78, PosY + h);
                _buttonSlotsDel[i].SetPosition(PosX + 440, PosY + h);
                h += 32;
            }
            _buttonMenu.SetPosition(PosX + 192, PosY + 372);
        }

        private void _ButtonSlots_Click(object sender, EventArgs e)
        {
            _slot = (int)((Button)sender).Tag;
            if (_listSingle.EmptyWorlds[_slot])
            {
                // Если пустой, создаём
                window.LScreen.CreateGame(_slot);
            }
            else
            {
                // Если имеется загружаем
                window.GameLocalRun(new GameSettings(_slot));
            }
        }

        private void _ButtonSlotsDel_Click(object sender, EventArgs e)
        {
            _slot = (int)((ButtonRemove)sender).Tag;
            window.LScreen.YesNo(this, string.Format(L.T("GameDeleteSlot{0}"), _slot + 1));
        }

        /// <summary>
        /// Запуск от родителя
        /// </summary>
        public override void LaunchFromParent(EnumScreenParent enumParent)
        {
            if (enumParent == EnumScreenParent.Yes && _slot != -1)
            {
                // Удаление слота
                _listSingle.GameRemove(_slot);
                _SlotInit(_slot);
            }
        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.486f, .569f, .616f, 1f);
            base.Draw(timeIndex);
        }
    }
}
