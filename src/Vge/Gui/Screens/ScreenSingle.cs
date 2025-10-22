using System;
using Vge.Games;
using Vge.Gui.Controls;
using Vge.Realms;
using Vge.Renderer.Font;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран выбора одиночной игры
    /// </summary>
    public class ScreenSingle : ScreenBase
    {
        private const int countSlot = ListSingleGame.CountSlot;
        /// <summary>
        /// Список одиночных игр
        /// </summary>
        private readonly ListSingleGame listSingle = new ListSingleGame();

        /// <summary>
        /// Выбранный слот
        /// </summary>
        private int slot = -1;

        protected readonly Label label;
        protected readonly Button buttonMenu;
        protected readonly Button[] buttonSlots = new Button[countSlot];
        protected readonly ButtonRemove[] buttonSlotsDel = new ButtonRemove[countSlot];

        public ScreenSingle(WindowMain window) : base(window)
        {
            listSingle.Initialize();
            FontBase font = window.Render.FontMain;
            label = new Label(window, font, Gi.Width, 0, L.T("Singleplayer"));
            label.Multiline().SetTextAlight(EnumAlight.Center, EnumAlightVert.Middle);
            buttonMenu = new ButtonThin(window, font, 128, L.T("Menu"));
            buttonMenu.Click += Button_Click;

            for (int i = 0; i < countSlot; i++)
            {
                buttonSlots[i] = new ButtonThin(window, font, 356, "");
                buttonSlotsDel[i] = new ButtonRemove(window);
                buttonSlots[i].Tag = i; // Номер слота
                buttonSlots[i].Click += ButtonSlots_Click;
                buttonSlotsDel[i].Tag = i; // Номер слота
                buttonSlotsDel[i].Click += ButtonSlotsDel_Click;
                SlotInit(i);
            }
        }

        private void SlotInit(int slot)
        {
            buttonSlots[slot].SetText(ChatStyle.Bolb + listSingle.NameWorlds[slot] + ChatStyle.Reset);
            buttonSlotsDel[slot].SetVisible(!listSingle.EmptyWorlds[slot]);
        }

        private void Button_Click(object sender, EventArgs e)
            => window.LScreen.MainMenu();

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void _OnInitialize()
        {
            _AddControls(buttonMenu);
            _AddControls(label);
            for (int i = 0; i < countSlot; i++)
            {
                _AddControls(buttonSlots[i]);
                _AddControls(buttonSlotsDel[i]);
            }
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void _OnResized()
        {
            int w2 = Width / 2;

            int hf = 28 * countSlot;
            int hup = (Height - hf) / 2;
            int hupFix = 80;
            int h;
            if (hup > hupFix)
            {
                h = hup - hupFix;
                label.SetSize(label.Width, hupFix);
            }
            else
            {
                h = 0;
                label.SetSize(label.Width, hup);
            }
            
            label.SetPosition(w2 - label.Width / 2, h);
            h += label.Height;
            for (int i = 0; i < countSlot; i++)
            {
                buttonSlots[i].SetPosition(w2 - 200, h);
                buttonSlotsDel[i].SetPosition(w2 + 160, h);
                h += 28;
            }
            buttonMenu.SetPosition(w2 - buttonMenu.Width / 2, h + (label.Height - 24) / 2);
        }

        private void ButtonSlots_Click(object sender, EventArgs e)
        {
            slot = (int)((Button)sender).Tag;
            if (listSingle.EmptyWorlds[slot])
            {
                // Если пустой, создаём
                window.LScreen.CreateGame(slot);
            }
            else
            {
                // Если имеется загружаем
                window.GameLocalRun(new GameSettings(slot));
            }
        }

        private void ButtonSlotsDel_Click(object sender, EventArgs e)
        {
            slot = (int)((ButtonRemove)sender).Tag;
            window.LScreen.YesNo(this, string.Format(L.T("GameDeleteSlot{0}"), slot + 1));
        }

        /// <summary>
        /// Запуск от родителя
        /// </summary>
        public override void LaunchFromParent(EnumScreenParent enumParent)
        {
            if (enumParent == EnumScreenParent.Yes && slot != -1)
            {
                // Удаление слота
                listSingle.GameRemove(slot);
                SlotInit(slot);
            }
        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.486f, .569f, .616f, 1f);
            base.Draw(timeIndex);
        }
    }
}
