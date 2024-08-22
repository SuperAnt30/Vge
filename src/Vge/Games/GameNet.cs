using Vge.Network;

namespace Vge.Games
{
    /// <summary>
    /// Класс игры по сети, сервер не создаём
    /// </summary>
    public class GameNet : GameBase
    {
        public GameNet()
        {
            IsLoacl = false;
        }
    }

}
