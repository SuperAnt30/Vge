using System;
using Vge.Gui.Controls;
using Vge.Renderer.Font;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Скрин окна чата
    /// </summary>
    public class ScreenChat : ScreenBase
    {
        /// <summary>
        /// Контрол написания текста
        /// </summary>
        protected readonly TextBox _textBoxMessage;

        protected readonly Button _buttonClose;

        public ScreenChat(WindowMain window) : base(window)
        {
            FontBase font = window.Render.FontMain;

            _textBoxMessage = new TextBox(window, font, 300, "Message...", TextBox.EnumRestrictions.Name, 16);
            _textBoxMessage.FixFocus();
            _buttonClose = new Button(window, font, 300, L.T("Close"));
            _buttonClose.Click += ButtonClose_Click;
        }

        private void ButtonClose_Click(object sender, EventArgs e)
           => window.LScreen.Close();

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            AddControls(_textBoxMessage);
            AddControls(_buttonClose);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            int w = Width / 2;
            int h = Height / 2;
            _textBoxMessage.SetPosition(8, Height - 48);
            _buttonClose.SetPosition(8, Height - 96);
        }
    }
}
