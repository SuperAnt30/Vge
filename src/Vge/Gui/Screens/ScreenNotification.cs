using System;
using Vge.Gui.Controls;
using Vge.Renderer.Font;
using WinGL.Util;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран оповещения, ошибки
    /// </summary>
    public class ScreenNotification : ScreenBase
    {
        protected readonly Label label;
        protected readonly Button button;

        public ScreenNotification(WindowMain window, string notification) : base(window)
        {
            FontBase font = window.Render.FontMain;
            label = new Label(window, font, window.Width - 100, 0, notification);
            label.Multiline().SetTextAlight(EnumAlight.Center, EnumAlightVert.Bottom);
            label.Click += Label_Click;
            button = new Button(window, font, 200, L.T("Menu"));
            button.Click += Button_Click;
        }

        private void Label_Click(object sender, EventArgs e)
            => Clipboard.SetText(label.Text);

        private void Button_Click(object sender, EventArgs e)
            => window.LScreen.MainMenu();

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            AddControls(button);
            AddControls(label);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            int h = Height / 2;
            int w = Width; 
            button.SetPosition((w - button.Width) / 2, h);
            label.SetSize(w - 100, 0).SetPosition(50, h - 20);
        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.5f, .2f, .2f, 1f);
            base.Draw(timeIndex);
        }
    }
}
