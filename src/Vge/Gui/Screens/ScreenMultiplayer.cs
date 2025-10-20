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
        protected readonly Label labelNikname;
        protected readonly Label labelAddress;
        protected readonly Label labelToken;
        protected readonly TextBox textBoxAddress;
        protected readonly TextBox textBoxToken;
        protected readonly Button buttonConnect;
        protected readonly Button buttonMenu;

        public ScreenMultiplayer(WindowMain window) : base(window)
        {
            FontBase font = window.Render.FontMain;
            label = new Label(window, font, L.T("Multiplayer"));
            label.SetTextAlight(EnumAlight.Center, EnumAlightVert.Bottom);
            labelNikname = new Label(window, font, 300, 40, L.T("Nikname") + ": " + Options.Nickname);
            labelNikname.SetTextAlight(EnumAlight.Center, EnumAlightVert.Middle);
            labelToken = new Label(window, font, 200, 40, L.T("Token"));
            labelToken.SetTextAlight(EnumAlight.Right, EnumAlightVert.Middle);
            labelAddress = new Label(window, font, 200, 40, L.T("IpAddress"));
            labelAddress.SetTextAlight(EnumAlight.Right, EnumAlightVert.Middle);

            textBoxToken = new TextBox(window, font, 200, Options.Token,
               TextBox.EnumRestrictions.Name);
            textBoxAddress = new TextBox(window, font, 200, Options.IpAddress, 
                TextBox.EnumRestrictions.IpPort, 15);
           
            buttonConnect = new ButtonThin(window, font, 128, L.T("Connect"));
            buttonConnect.Click += ButtonConnect_Click;
            buttonMenu = new ButtonThin(window, font, 128, L.T("Menu"));
            buttonMenu.Click += ButtonMenu_Click;
        }

        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            Options.IpAddress = textBoxAddress.Text;
            Options.Token = textBoxToken.Text;
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
            AddControls(labelNikname);
            AddControls(labelToken);
            AddControls(labelAddress);
            AddControls(textBoxToken);
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

            labelNikname.SetPosition(w - labelNikname.Width / 2, h - 160);
            labelToken.SetPosition(w - labelToken.Width - 2, h - 116);
            textBoxToken.SetPosition(w + 2, h - 116);
            labelAddress.SetPosition(w - labelAddress.Width - 2, h - 72);
            textBoxAddress.SetPosition(w + 2, h - 72);
            buttonConnect.SetPosition(w - buttonConnect.Width - 2, h);
            buttonMenu.SetPosition(w + 2, h);
        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.486f, .569f, .616f, 1f);
            //gl.ClearColor(.827f, .796f, .745f, 1f);
            base.Draw(timeIndex);
        }
    }
}