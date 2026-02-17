using Mvk2.Gui.Controls;
using Mvk2.Item;
using Mvk2.Packets;
using Mvk2.World.BlockEntity.List;
using System;
using System.Runtime.CompilerServices;
using Vge.Gui.Controls;
using Vge.Gui.Screens;
using Vge.Item;
using Vge.Network;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using WinGL.Actions;

namespace Mvk2.Gui.Screens
{
    /// <summary>
    /// Окно креативного инвентаря для игры Малювеки 2
    /// </summary>
    public class ScreenStorageCreative : ScreenStorage
    {
        /// <summary>
        /// Количество закладок
        /// </summary>
        private const int _countTab = ItemsCreative.CountTab;
        /// <summary>
        /// Количенство ячеек в одной закладке
        /// </summary>
        private readonly int _count = ItemsCreative.Count;
        /// <summary>
        /// Массив кнопок закладок
        /// </summary>
        private readonly ButtonTab[] _buttonTab = new ButtonTab[_countTab];
        private readonly Label _labelPage;
        private readonly ButtonArrow _buttonBack;
        private readonly ButtonArrow _buttonNext;
        /// <summary>
        /// Номер закладки
        /// </summary>
        private int _tab = 0;
        /// <summary>
        /// Номер страницы
        /// </summary>
        private int _page = 0;
        /// <summary>
        /// Количество страниц в закладке
        /// </summary>
        private int _countPages;

        public ScreenStorageCreative(WindowMvk window) : base(window)
        {
            for (int i = 0; i < _countTab; i++)
            {
                _buttonTab[i] = new ButtonTab(window, i,
                    ItemsCreative.NameTab[i]);
                _buttonTab[i].Tag = i;
                _buttonTab[i].Click += _ButtonTab_Click;
            }

            _labelPage = new Label(window, window.Render.FontMain, 40, 16, "");
            _buttonBack = new ButtonArrow(window, false);
            _buttonBack.Click += (sender, e) => _PageBackNext(false);
            _buttonNext = new ButtonArrow(window, true);
            _buttonNext.Click += (sender, e) => _PageBackNext(true);
            _ActionTab();
        }

        /// <summary>
        /// Активация закладки
        /// </summary>
        private void _ActionTab()
        {
            _buttonTab[_tab].SetEnable(false);
            _countPages = ItemsRegMvk.Creative.GetCountPage(_tab);
            _page = 0;
            _ActionPage();
        }

        private void _ActionPage()
        {
            ItemsCreative itemsCreative = ItemsRegMvk.Creative;

            // Загрузить все слоты в ящик
            for (int i = _inventoryCount; i < _inventoryCount + _count; i++)
            {
                _slot[i].SetStack(itemsCreative.GetStackInSlot(_tab, _page, i - _inventoryCount));
                _slot[i].SetEnable(true);
            }

            _labelPage.SetText(string.Format("{0}/{1}", _page + 1, _countPages));
            _buttonBack.SetEnable(_page > 0);
            _buttonNext.SetEnable(_page < _countPages - 1);

            _windowMvk.Game.TrancivePacket(new PacketC0EClickWindow(
                (byte)EnumActionClickWindow.OpenCreativeInventory,
                _tab << 8 | _page & 255));
        }

        /// <summary>
        /// Инициализация слотов
        /// </summary>
        protected override void _Init()
        {
            base._Init();

            // Ящик
            for (int i = 0; i < _count; i++)
            {
                _SetSlot(i + _inventoryCount, new ControlSlotMvk(_windowMvk, null, (byte)(i + 100)));
                _slot[i + _inventoryCount].SetEnable(false);
            }
        }

        /// <summary>
        /// Название заголовка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override string _GetTitle() => L.T("Creative");

        /// <summary>
        /// Количество слотов
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override int _GetSlotCount() => _inventoryCount + _count;

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void _OnInitialize()
        {
            for (int i = 0; i < _countTab; i++)
            {
                _AddControls(_buttonTab[i]);
            }
            _AddControls(_labelPage);
            _AddControls(_buttonNext);
            _AddControls(_buttonBack);
            // Иконка выбранного будет в базе, по этому в конце
            base._OnInitialize();
        }

        private void _ButtonTab_Click(object sender, EventArgs e)
        {
            _buttonTab[_tab].SetEnable(true);
            _tab = (int)((WidgetBase)sender).Tag;
            _ActionTab();
        }

        /// <summary>
        /// Получить сетевой пакет
        /// </summary>
        public override void AcceptNetworkPackage(IPacket packet)
        {
            if (packet is PacketS2FSetSlot packetS2F)
            {
                // Изменился один слот (из вне поменялся, образно другой игрок)
                _slot[packetS2F.SlotId - 100 + _inventoryCount].SetStack(packetS2F.Stack);
            }
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void _OnResized()
        {
            base._OnResized();

            // Ящик на 48 слот
            int i = 0;
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 12; x++)
                {
                    _slot[i + _inventoryCount].SetPosition(PosX + 12 + x * 36, PosY + 27 + y * 36);
                    i++;
                }
            }

            for (i = 0; i < _countTab; i++)
            {
                _buttonTab[i].SetPosition(PosX + 92 + i * 26, PosY + 4);
            }

            _buttonBack.SetPosition(PosX + 340, PosY + 4);
            _labelPage.SetPosition(PosX + 364, PosY + 8);
            _buttonNext.SetPosition(PosX + 404, PosY + 4);
        }

        public override void OnKeyDown(Keys keys)
        {
            base.OnKeyDown(keys);

            if (keys == Keys.C)
            {
                _Close();
            }
            else if (keys == Keys.Left)
            {
                _PageBackNext(false);
            }
            else if (keys == Keys.Right)
            {
                _PageBackNext(true);
            }
        }

        /// <summary>
        /// Вращение колёсика мыши
        /// </summary>
        /// <param name="delta">смещение</param>
        public override void OnMouseWheel(int delta, int x, int y)
        {
            if (delta != 0) _PageBackNext(delta < 0);
        }

        private void _PageBackNext(bool next)
        {
            if (next)
            {
                if (_page < _countPages - 1)
                {
                    _page++;
                    _ActionPage();
                }
            }
            else
            {
                if (_page > 0)
                {
                    _page--;
                    _ActionPage();
                }
            }
        }

        /// <summary>
        /// Запустить текстуру фона
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _BindTextureBg() => _windowMvk.GetRender().BindTextureConteinerStorage();
    }
}
