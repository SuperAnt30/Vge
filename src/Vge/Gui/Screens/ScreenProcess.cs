using Vge.Gui.Controls;
using Vge.Realms;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран процесс выполнения
    /// </summary>
    public class ScreenProcess : ScreenBase
    {
        protected readonly Label _label;

        public ScreenProcess(WindowMain window, string text) : base(window)
        {
            _label = new Label(window, window.Render.FontMain, 320, Height,
                ChatStyle.Bolb + text + ChatStyle.Reset);
            _label.SetTextAlight(EnumAlight.Center, EnumAlightVert.Middle);
        }

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void _OnInitialize()
        {
            base._OnInitialize();
            _AddControls(_label);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void _OnResized()
        {
            _label.SetPosition(Width / 2 - 160, 0);
        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.486f, .569f, .616f, 1f);
            base.Draw(timeIndex);
        }
    }
}
