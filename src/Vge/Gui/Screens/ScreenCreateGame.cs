using System;
using Vge.Gui.Controls;
using Vge.Renderer.Font;
using WinGL.Util;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран создания игры
    /// </summary>
    public class ScreenCreateGame : ScreenBase
    {
        private readonly int slot;
        private readonly Label label;
        private readonly Button buttonCreate;
        private readonly Button buttonCancel;

        public ScreenCreateGame(WindowMain window, int slot) : base(window)
        {
            // TODO::добавить контрол зерна
            this.slot = slot;
            FontBase font = window.Render.FontMain;
            label = new Label(window, font, window.Width - 100, 0, "SingleCreate #" + slot);
            label.Multiline().SetTextAlight(EnumAlight.Center, EnumAlightVert.Bottom);
            label.Click += Label_Click;
            buttonCreate = new Button(window, font, 200, L.T("Create"));
            buttonCreate.Click += ButtonCreate_Click;
            buttonCancel = new Button(window, font, 200, L.T("Cancel"));
            buttonCancel.Click += ButtonCancel_Click;
        }

        private void Label_Click(object sender, EventArgs e)
            => Clipboard.SetText(label.Text);

        private void ButtonCreate_Click(object sender, EventArgs e)
        {
            // Создаём игру 
            long seed = 4;
            window.GameLocalRun(slot, false, seed);
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
            => window.LScreen.Single();

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            AddControls(buttonCreate);
            AddControls(buttonCancel);
            AddControls(label);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            int h = Height / 2;
            int w = Width / 2;
            buttonCreate.SetPosition(w - buttonCreate.Width + 2, h);
            buttonCancel.SetPosition(w + 2, h);
            label.SetSize(Width - 100, label.Height);
            label.SetPosition(50, h - label.Height - 20);
        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.5f, .2f, .2f, 1f);
            base.Draw(timeIndex);
        }
    }
}