using System;
using System.Runtime.CompilerServices;
using Vge.Gui.Controls;
using Vge.Renderer.Font;
using WinGL.Util;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран условия, где надо принять решения и вернутся в предыдущий экран
    /// </summary>
    public class ScreenYesNo : ScreenWindow
    {
        private readonly ScreenBase _parent;
        protected readonly Label _label;
        protected readonly Button _buttonYes;
        protected readonly Button _buttonNo;

        public ScreenYesNo(WindowMain window, ScreenBase parent, string text) : base(window, 512f, 320, 200, true)
        {
            FontBase font = window.Render.FontMain;
            _parent = parent;
            _label = new Label(window, font, WidthWindow - 52, 112, text);
            _label.Multiline().SetTextAlight(EnumAlight.Center, EnumAlightVert.Middle);
            _label.Click += Label_Click;
            _buttonYes = new ButtonThin(window, font, 64, L.T("Yes"));
            _buttonYes.Click += ButtonYes_Click;
            _buttonNo = new ButtonThin(window, font, 64, L.T("No"));
            _buttonNo.Click += ButtonNo_Click;
        }

        /// <summary>
        /// Название заголовка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override string _GetTitle() => L.T("Warning");

        /// <summary>
        /// Закрытие скрина
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _Close() => window.LScreen.Parent(_parent, EnumScreenParent.No);

        /// <summary>
        /// Клик за пределами окна
        /// </summary>
        protected override void _OnClickOutsideWindow() { }

        private void Label_Click(object sender, EventArgs e) => Clipboard.SetText(_label.Text);

        private void ButtonYes_Click(object sender, EventArgs e)
            => window.LScreen.Parent(_parent, EnumScreenParent.Yes);

        private void ButtonNo_Click(object sender, EventArgs e) => _Close();

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void _OnInitialize()
        {
            base._OnInitialize();
            _AddControls(_label);
            _AddControls(_buttonYes);
            _AddControls(_buttonNo);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void _OnResized()
        {
            PosX = (Width - WidthWindow) / 2;
            PosY = (Height - HeightWindow) / 2;
            base._OnResized();

            _label.SetPosition(PosX + 26, PosY + 32);

            _buttonYes.SetPosition(PosX + 90, PosY + 156);
            _buttonNo.SetPosition(PosX + 166, PosY + 156);
        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.486f, .569f, .616f, 1f);
            base.Draw(timeIndex);
        }
    }
}