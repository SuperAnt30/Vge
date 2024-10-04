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
        private readonly GameBase _game;

        public PlayerClient(GameBase game)
        {
            _game = game;
            Login = game.ToLoginPlayer();
            Token = game.ToTokenPlayer();
        }

        /// <summary>
        /// Получить время в милисекундах
        /// </summary>
        protected override long _Time() => _game.Time();
    }
}
