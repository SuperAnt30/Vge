using Vge.Gui.Screens;

namespace Mvk2.Gui.Screens
{
    /**
     * 
     * В Игре наследуются от ScreenWindow:
     *  |
     *  +-ScreenChatMvk * (Окно чата) =Chat
     *  |
     *  +-
     *  |
     *  +-ScreenInventoryMvk -
     */

    /// <summary>
    /// Запуск экранов для Малювек
    /// </summary>
    public class LaunchScreenMvk : LaunchScreen
    {
        /// <summary>
        /// Объект окна Mvk
        /// </summary>
        protected readonly WindowMvk _windowMvk;

        public LaunchScreenMvk(WindowMvk window) : base(window)
            => _windowMvk = window;

        /// <summary>
        /// Создать скрин заставки
        /// </summary>
        public override void Splash() => window.ScreenCreate(new ScreenSplashMvk(_windowMvk));
        /// <summary>
        /// Создать скрин выбора одиночной игры
        /// </summary>
        public override void Single() => window.ScreenCreate(new ScreenSingleMvk(_windowMvk));
        /// <summary>
        /// Создать скрин опций
        /// </summary>
        public override void Options(ScreenBase parent, bool inGame) 
            => window.ScreenCreate(new ScreenOptionsMvk(_windowMvk, parent, inGame), false);

        /// <summary>
        /// Создать скрин чата
        /// </summary>
        public void Chat() => window.ScreenCreate(new ScreenChatMvk(_windowMvk));

        /// <summary>
        /// Создать скрин инвентаря
        /// </summary>
        public void Inventory() => window.ScreenCreate(new ScreenInventory(_windowMvk));

        /// <summary>
        /// Создать скрин хранилища отладки
        /// </summary>
        public void StorageDebug() => window.ScreenCreate(new ScreenStorageHole(_windowMvk));
    }
}
