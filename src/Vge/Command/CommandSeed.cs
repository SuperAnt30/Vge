using Vge.Entity.Player;
using Vge.Games;
using Vge.Realms;
using Vge.World;

namespace Vge.Command
{
    /// <summary>
    /// Команда узнать игровой сид мира
    /// </summary>
    public class CommandSeed : CommandBase
    {
        public CommandSeed(GameServer server) : base(server)
            => Name = "seed";

        /// <summary>
        /// Возвращает true, если отправителю данной команды разрешено использовать эту команду
        /// </summary>
        public override string UseCommand(CommandSender sender)
            => Server.Settings.Seed.ToString();
    }
}
