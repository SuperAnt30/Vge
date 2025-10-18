using Mvk2;
using Vge.Gui.Controls;
using Vge.Renderer.Font;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Скрин окна чата
    /// </summary>
    public class ScreenChatMvk : ScreenChat
    {
        private readonly WindowMvk _windowMvk;

        public ScreenChatMvk(WindowMvk window) : base(window, 512, 354)
        {
            _windowMvk = window;
        }

        protected override void _InitTitle()
        {
            FontBase font = window.Render.FontMain;
            _labelTitle = new Label(window, font, 250, 50, L.T("Chat"));
            _labelTitle.SetTextAlight(EnumAlight.Left, EnumAlightVert.Top);
            _buttonClose = new ButtonClose(window);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            PosX = 8;
            PosY = Height - HeightWindow - 8;
            base.OnResized();
            _labelTitle.SetPosition(PosX + 16, PosY + 10);
            _buttonClose.SetPosition(PosX + WidthWindow - 50, PosY);
        }

        /// <summary>
        /// Запустить текстуру фона
        /// </summary>
        protected override void _BindTextureBg() => _windowMvk.GetRender().BindTextureChat();
    }
}
