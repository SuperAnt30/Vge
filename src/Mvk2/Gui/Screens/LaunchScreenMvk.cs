using Vge.Gui.Screens;

namespace Mvk2.Gui.Screens
{
    /// <summary>
    /// Запуск экранов для Малювек
    /// </summary>
    public class LaunchScreenMvk : LaunchScreen
    {
        /// <summary>
        /// Объект окна Mvk
        /// </summary>
        protected readonly WindowMvk windowMvk;

        public LaunchScreenMvk(WindowMvk window) : base(window)
            => windowMvk = window;

        /// <summary>
        /// Создать скрин заставки
        /// </summary>
        public override void Splash() => window.ScreenCreate(new ScreenSplashMvk(windowMvk));
        /// <summary>
        /// Создать скрин выбора одиночной игры
        /// </summary>
        public override void Single() => window.ScreenCreate(new ScreenSingleMvk(windowMvk));
    }
}
