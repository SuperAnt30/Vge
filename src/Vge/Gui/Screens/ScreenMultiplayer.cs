using System;
using Vge.Gui.Controls;
using Vge.Renderer.Font;
using Vge.Util;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран многопользовательской игры
    /// </summary>
    public class ScreenMultiplayer : ScreenBase
    {
        protected readonly Label label;
        protected readonly Label labelAddress;
        protected readonly TextBox textBoxAddress;
        protected readonly Button buttonConnect;
        protected readonly Button buttonMenu;

        public ScreenMultiplayer(WindowMain window) : base(window)
        {
            FontBase font = window.Render.FontMain;
            label = new Label(window, font, L.T("Multiplayer"));
            label.SetTextAlight(EnumAlight.Center, EnumAlightVert.Bottom);
            labelAddress = new Label(window, font, 200, 40, L.T("IpAddress"));
            labelAddress.SetTextAlight(EnumAlight.Right, EnumAlightVert.Middle);

            textBoxAddress = new TextBox(window, font, 200, 40, Options.IpAddress, 
                TextBox.EnumRestrictions.IpPort, 15);

            buttonConnect = new Button(window, font, 200, L.T("Connect"));
            buttonConnect.Click += ButtonConnect_Click;
            buttonMenu = new Button(window, font, 200, L.T("Menu"));
            buttonMenu.Click += ButtonMenu_Click;
        }

        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            Options.IpAddress = textBoxAddress.Text;
            window.OptionsSave();
            window.GameNetRun(Options.IpAddress, 32021);
        }

        private void ButtonMenu_Click(object sender, EventArgs e)
            => window.LScreen.MainMenu();

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            AddControls(label);
            AddControls(labelAddress);
            AddControls(textBoxAddress);
            AddControls(buttonConnect);
            AddControls(buttonMenu);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            int h = Height / 2;
            int w = Width / 2;
            label.SetSize(Width - 100, label.Height).SetPosition(50, h - label.Height - 200);
            labelAddress.SetPosition(w - labelAddress.Width - 2, h - 92);
            textBoxAddress.SetPosition(w + 2, h - 92);
            buttonConnect.SetPosition(w - buttonConnect.Width - 2, h);
            buttonMenu.SetPosition(w + 2, h);
            
            
        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.5f, .2f, .2f, 1f);
            base.Draw(timeIndex);
        }
    }
}