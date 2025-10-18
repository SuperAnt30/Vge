using System;
using Vge.Gui.Controls;
using Vge.Renderer.Font;
using WinGL.Util;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран условия, где надо принять решения и вернутся в предыдущий экран
    /// </summary>
    public class ScreenYesNo : ScreenBase
    {
        private readonly ScreenBase parent;
        protected readonly Label label;
        protected readonly Button64 buttonYes;
        protected readonly Button64 buttonNo;

        public ScreenYesNo(WindowMain window, ScreenBase parent, string text) : base(window)
        {
            FontBase font = window.Render.FontMain;
            this.parent = parent;
            label = new Label(window, font, Gi.Width - 100, 0, text);
            label.Multiline().SetTextAlight(EnumAlight.Center, EnumAlightVert.Bottom);
            label.Click += Label_Click;
            buttonYes = new Button64(window, font, L.T("Yes"));
            buttonYes.Click += ButtonYes_Click;
            buttonNo = new Button64(window, font, L.T("No"));
            buttonNo.Click += ButtonNo_Click;
        }

        private void Label_Click(object sender, EventArgs e)
            => Clipboard.SetText(label.Text);

        private void ButtonYes_Click(object sender, EventArgs e)
            => window.LScreen.Parent(parent, EnumScreenParent.Yes);

        private void ButtonNo_Click(object sender, EventArgs e)
            => window.LScreen.Parent(parent, EnumScreenParent.No);

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            AddControls(buttonYes);
            AddControls(buttonNo);
            AddControls(label);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            int h = Height / 2;
            int w = Width / 2;
            buttonYes.SetPosition(w - buttonYes.Width + 2, h);
            buttonNo.SetPosition(w + 2, h);
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