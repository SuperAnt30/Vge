using System;
using System.Runtime.CompilerServices;
using Vge.Gui.Controls;
using Vge.Realms;
using Vge.Renderer.Font;
using Vge.Util;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран многопользовательской игры
    /// </summary>
    public class ScreenMultiplayer : ScreenWindow
    {
        protected readonly Label _labelNikname;
        protected readonly Label _labelAddress;
        protected readonly Label _labelToken;
        protected readonly TextBox _textBoxAddress;
        protected readonly TextBox _textBoxToken;
        protected readonly Button _buttonConnect;
        protected readonly Button _buttonMenu;

        public ScreenMultiplayer(WindowMain window) : base(window, 512f, 512, 416, true)
        {
            FontBase font = window.Render.FontMain;
            _labelNikname = new Label(window, font, 300, 24, 
                L.T("Nikname") + " " + ChatStyle.Bolb + Options.Nickname);
            _labelNikname.SetTextAlight(EnumAlight.Center, EnumAlightVert.Middle);
            _labelToken = new Label(window, font, 130, 24, L.T("Token"));
            _labelToken.SetTextAlight(EnumAlight.Right, EnumAlightVert.Middle);
            _labelAddress = new Label(window, font, 130, 24, L.T("IpAddress"));
            _labelAddress.SetTextAlight(EnumAlight.Right, EnumAlightVert.Middle);

            _textBoxToken = new TextBox(window, font, 200, Options.Token,
               TextBox.EnumRestrictions.Name);
            _textBoxAddress = new TextBox(window, font, 200, Options.IpAddress, 
                TextBox.EnumRestrictions.IpPort, 15);
           
            _buttonConnect = new ButtonThin(window, font, 128, L.T("Connect"));
            _buttonConnect.Click += ButtonConnect_Click;
            _buttonMenu = new ButtonThin(window, font, 128, L.T("Menu"));
            _buttonMenu.Click += ButtonMenu_Click;
        }

        /// <summary>
        /// Название заголовка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override string _GetTitle() => L.T("Multiplayer");

        /// <summary>
        /// Закрытие скрина
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _Close() => window.LScreen.MainMenu();

        /// <summary>
        /// Клик за пределами окна
        /// </summary>
        protected override void _OnClickOutsideWindow() { }

        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            Options.IpAddress = _textBoxAddress.Text;
            Options.Token = _textBoxToken.Text;
            window.OptionsSave();
            window.GameNetRun(Options.IpAddress, 32021);
        }

        private void ButtonMenu_Click(object sender, EventArgs e) => _Close();

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            AddControls(_labelNikname);
            AddControls(_labelToken);
            AddControls(_labelAddress);
            AddControls(_textBoxToken);
            AddControls(_textBoxAddress);
            AddControls(_buttonConnect);
            AddControls(_buttonMenu);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            PosX = (Width - WidthWindow) / 2;
            PosY = (Height - HeightWindow) / 2;
            base.OnResized();

            _labelNikname.SetPosition(PosX + 100, PosY + 80);
            _labelToken.SetPosition(PosX + 52, PosY + 130);
            _textBoxToken.SetPosition(PosX + 188, PosY + 130);
            _labelAddress.SetPosition(PosX + 52, PosY + 180);
            _textBoxAddress.SetPosition(PosX + 188, PosY + 180);
            _buttonConnect.SetPosition(PosX + 122, PosY + 372);
            _buttonMenu.SetPosition(PosX + 262, PosY + 372);
        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.486f, .569f, .616f, 1f);
            base.Draw(timeIndex);
        }
    }
}