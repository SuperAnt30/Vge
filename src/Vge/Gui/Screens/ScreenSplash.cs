namespace Vge.Gui.Screens
{
    /// <summary>
    /// Заставка
    /// </summary>
    public class ScreenSplash : ScreenBase
    {
        private int count;

        public ScreenSplash(WindowMain window) : base(window) { }

        public override void OnTick()
        {
            if (count++ > 40)
            {
                window.ScreenMainMenu();
            }
        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.9f, .9f, 1f, 1f);
        }
    }
}
