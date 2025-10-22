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
        protected readonly WindowMvk _windowMvk;

        public LaunchScreenMvk(WindowMvk window) : base(window)
            => _windowMvk = window;

        /// <summary>
        /// Создать скрин заставки
        /// </summary>
        public override void Splash() => window.ScreenCreate(new ScreenSplashMvk(_windowMvk));
        /// <summary>
        /// Создать скрин главного меню
        /// </summary>
        public override void MainMenu() => window.ScreenCreate(new ScreenMainMenuMvk(_windowMvk));
        /// <summary>
        /// Создать скрин меню во время игры
        /// </summary>
        public override void InGameMenu() => window.ScreenCreate(new ScreenInGameMenuMvk(_windowMvk));
        /// <summary>
        /// Создать скрин выбора одиночной игры
        /// </summary>
        public override void Single() => window.ScreenCreate(new ScreenSingleMvk(_windowMvk));
        /// <summary>
        /// Создать скрин создания одиночной игры
        /// </summary>
        public override void CreateGame(int slot) => window.ScreenCreate(new ScreenCreateGameMvk(_windowMvk, slot));
        /// <summary>
        /// Создание скрина многопользовательской игры
        /// </summary>
        public override void Multiplayer() => window.ScreenCreate(new ScreenMultiplayerMvk(_windowMvk));
        /// <summary>
        /// Создать скрин опций
        /// </summary>
        public override void Options(ScreenBase parent, bool inGame) 
            => window.ScreenCreate(new ScreenOptionsMvk(_windowMvk, parent, inGame), false);
        /// <summary>
        /// Создать скрин оповещения, после которого выйдем в меню
        /// </summary>
        public override void Notification(string notification)
            => window.ScreenCreate(new ScreenNotificationMvk(_windowMvk, notification));
        /// <summary>
        /// Создать скрин сообщения YesNo
        /// </summary>
        public override void YesNo(ScreenBase parent, string text)
            => window.ScreenCreate(new ScreenYesNoMvk(_windowMvk, parent, text), false);

        

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
