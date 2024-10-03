using Vge.Games;

namespace Vge.Management
{
    /// <summary>
    /// Объект игрока, на клиенте
    /// </summary>
    public class PlayerClient : PlayerBase
    {
        /// <summary>
        /// Класс  игры
        /// </summary>
        private readonly GameBase game;

        public PlayerClient(GameBase game)
        {
            this.game = game;
            Login = game.ToLoginPlayer();
            Token = game.ToTokenPlayer();
        }

        /// <summary>
        /// Получить время в милисекундах
        /// </summary>
        protected override long Time() => game.Time();
    }
}
