using System;
using Vge.Games;
using Vge.Gui.Controls;
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
        /// Выбранный слот
        /// </summary>
        private int slot = -1;

        protected readonly Label label;
        protected readonly Button buttonMenu;
        protected readonly Button[] buttonSlots = new Button[countSlot];
        protected readonly Button[] buttonSlotsDel = new Button[countSlot];

        public ScreenSingle(WindowMain window) : base(window)
        {
            window.ListSingle.Initialize();
            FontBase font = window.Render.FontMain;
            label = new Label(window, font, window.Width, 0, L.T("Single"));
            label.Multiline().SetTextAlight(EnumAlight.Center, EnumAlightVert.Middle);
            buttonMenu = new Button(window, font, 200, L.T("Menu"));
            buttonMenu.Click += Button_Click;

            for (int i = 0; i < countSlot; i++)
            {
                buttonSlots[i] = new Button(window, font, 356, "");
                buttonSlotsDel[i] = new Button(window, font, 40, "X");
                buttonSlots[i].Tag = i; // Номер слота
                buttonSlots[i].Click += ButtonSlots_Click;
                buttonSlotsDel[i].Tag = i; // Номер слота
                buttonSlotsDel[i].Click += ButtonSlotsDel_Click;
                SlotInit(i);
            }
        }

        private void SlotInit(int slot)
        {
            buttonSlots[slot].SetText(window.ListSingle.NameWorlds[slot]);
            buttonSlotsDel[slot].SetEnable(!window.ListSingle.EmptyWorlds[slot]);
        }

        private void Button_Click(object sender, EventArgs e)
            => window.LScreen.MainMenu();

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            AddControls(buttonMenu);
            AddControls(label);
            for (int i = 0; i < countSlot; i++)
            {
                AddControls(buttonSlots[i]);
                AddControls(buttonSlotsDel[i]);
            }
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            int w2 = Width / 2;

            int hf = 44 * countSlot;
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
                h += 44;
            }
            buttonMenu.SetPosition(w2 - buttonMenu.Width / 2, h + (label.Height - 40) / 2);
        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.5f, .3f, .02f, 1f);
            base.Draw(timeIndex);
        }

        private void ButtonSlots_Click(object sender, EventArgs e)
        {
            slot = (int)((Button)sender).Tag;
            if (window.ListSingle.EmptyWorlds[slot])
            {
                // Если пустой, создаём
                window.LScreen.CreateGame(slot);
            }
            else
            {
                // Если имеется загружаем
                window.GameLocalRun(slot, true);
            }
        }

        private void ButtonSlotsDel_Click(object sender, EventArgs e)
        {
            slot = (int)((Button)sender).Tag;
            window.LScreen.YesNo(this, string.Format(L.T("GameDelete {0}"), slot + 1));
        }

        /// <summary>
        /// Запуск от родителя
        /// </summary>
        public override void LaunchFromParent(EnumScreenParent enumParent)
        {
            if (enumParent == EnumScreenParent.Yes && slot != -1)
            {
                // Удаление слота
                window.ListSingle.GameRemove(slot);
                SlotInit(slot);
            }
        }
    }
}
