using System;
using System.Runtime.CompilerServices;
using Vge.Gui.Controls;
using Vge.Renderer.Font;
using WinGL.Util;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран оповещения, ошибки
    /// </summary>
    public class ScreenNotification : ScreenWindow
    {
        protected readonly Label _label;
        protected readonly Button _button;

        public ScreenNotification(WindowMain window, string notification) : base(window, 512f, 320, 200, true)
        {
            FontBase font = window.Render.FontMain;
            _label = new Label(window, font, WidthWindow - 52, 112, notification);
            _label.Multiline().SetTextAlight(EnumAlight.Center, EnumAlightVert.Middle);
            _label.Click += Label_Click;
            _button = new ButtonThin(window, font, 128, L.T("Menu"));
            _button.Click += Button_Click;
        }

        /// <summary>
        /// Название заголовка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override string _GetTitle() => L.T("Notification");

        /// <summary>
        /// Закрытие скрина
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _Close() => window.LScreen.MainMenu();

        /// <summary>
        /// Клик за пределами окна
        /// </summary>
        protected override void _OnClickOutsideWindow() { }

        private void Label_Click(object sender, EventArgs e) => Clipboard.SetText(_label.Text);

        private void Button_Click(object sender, EventArgs e) => _Close();

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            AddControls(_label);
            AddControls(_button);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            PosX = (Width - WidthWindow) / 2;
            PosY = (Height - HeightWindow) / 2;
            base.OnResized();

            _label.SetPosition(PosX + 26, PosY + 32);
            _button.SetPosition(PosX + 96, PosY + 156);
        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.486f, .569f, .616f, 1f);
            base.Draw(timeIndex);
        }
    }
}
