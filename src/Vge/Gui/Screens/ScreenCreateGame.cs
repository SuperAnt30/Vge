using System;
using Vge.Games;
using Vge.Gui.Controls;
using Vge.Renderer.Font;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран создания игры
    /// </summary>
    public class ScreenCreateGame : ScreenBase
    {
        private readonly int slot;
        protected readonly Label label;
        protected readonly Label labelSeed;
        protected readonly TextBox textBoxSeed;
        protected readonly ButtonThin buttonCreate;
        protected readonly ButtonThin buttonCancel;

        public ScreenCreateGame(WindowMain window, int slot) : base(window)
        {
            this.slot = slot;
            FontBase font = window.Render.FontMain;
            label = new Label(window, font, Gi.Width - 100, 0, L.T("SingleCreate{0}", slot));
            label.SetTextAlight(EnumAlight.Center, EnumAlightVert.Bottom);

            labelSeed = new Label(window, font, 300, 40, L.T("Seed"));
            textBoxSeed = new TextBox(window, font, 300, "", TextBox.EnumRestrictions.Number);

            buttonCreate = new ButtonThin(window, font, 64, L.T("Create"));
            buttonCreate.Click += ButtonCreate_Click;
            buttonCancel = new ButtonThin(window, font, 64, L.T("Cancel"));
            buttonCancel.Click += ButtonCancel_Click;
        }

        private void ButtonCreate_Click(object sender, EventArgs e)
        {
            // Создаём игру
            long seed;
            try
            {
                seed = textBoxSeed.Text != "" ? long.Parse(textBoxSeed.Text) : 0;
            }
            catch
            {
                seed = 0;
            }
            window.GameLocalRun(new GameSettings(slot, seed));
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
            => window.LScreen.Single();

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            AddControls(label);
            AddControls(labelSeed);
            AddControls(textBoxSeed);
            AddControls(buttonCreate);
            AddControls(buttonCancel);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            int h = Height / 2;
            int w = Width / 2;
            int wl = w - labelSeed.Width / 2;
            label.SetSize(Width - 100, label.Height).SetPosition(50, h - label.Height - 200);
            labelSeed.SetPosition(wl, h - 132);
            textBoxSeed.SetPosition(wl, h - 92);

            buttonCreate.SetPosition(w - buttonCreate.Width - 2, h);
            buttonCancel.SetPosition(w + 2, h);
        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.827f, .796f, .745f, 1f);
            base.Draw(timeIndex);
        }
    }
}