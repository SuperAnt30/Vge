using Vge.Games;

namespace Vge.Gui.Huds
{
    /// <summary>
    /// Абстрактный класс индикации. Heads-Up Display
    /// </summary>
    public abstract class HudBase
    {
        /// <summary>
        /// Объект окна
        /// </summary>
        protected readonly GameBase game;

        public HudBase(GameBase game) => this.game = game;

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        public virtual void Draw() { }
    }
}
