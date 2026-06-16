using Vge.Command;
using Vge.Games;

namespace Mvk2.Command
{
    /// <summary>
    /// Статический класс, для регистрации комманд
    /// </summary>
    public sealed class ManagerCommandMvk
    {
        public static void Init(GameServer server, ManagerCommand manager)
        {
            manager.Registration(new CommandRegen(server));
            manager.Registration(new CommandGameMode(server));
        }
    }
}
