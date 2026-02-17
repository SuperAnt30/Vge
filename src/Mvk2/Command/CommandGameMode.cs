using Vge.Command;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Realms;

namespace Mvk2.Command
{
    /// <summary>
    /// Команда режим игры
    /// </summary>
    public class CommandGameMode : CommandBase
    {
        public CommandGameMode(GameServer server) : base(server)
        {
            Name = "gamemode";
            NameMin = "gm";
        }

        /// <summary>
        /// Возвращает true, если отправителю данной команды разрешено использовать эту команду
        /// </summary>
        public override string UseCommand(CommandSender sender)
        {
            string[] commandParams = sender.GetCommandParams();
            PlayerServer player = sender.GetPlayer();
            bool another = false;
            if (player == null)
            {
                if (commandParams.Length > 1)
                {
                    // Пробуем проверить ник
                    player = Server.Players.FindPlayerByLogin(commandParams[1]);
                }
                if (player == null)
                {
                    return ChatStyle.Red + L.S("CommandsGamemodeNotPlayer");
                }
                another = true;
            }

            if (commandParams.Length == 0)
            {
                return ChatStyle.Red + L.S("CommandsGamemodeNotParams");
            }

            string param = commandParams[0].ToLower();
            string result = "";
            if (param.Equals("survival") || param.Equals("s") || param.Equals("0"))
            {
                // режим выживания
                player.SetGameMode(0);
                result = ChatStyle.Yellow + L.S("CommandsGamemodeSurvival");
            }
            else if (param.Equals("creative") || param.Equals("c") || param.Equals("1"))
            {
                // творческий режим
                player.SetGameMode(1);
                result = ChatStyle.Green + L.S("CommandsGamemodeCreative");
            }
            else if (param.Equals("spectator") || param.Equals("sp") || param.Equals("2"))
            {
                // режим наблюдателя
                player.SetGameMode(2);
                result = ChatStyle.Gray + L.S("CommandsGamemodeSpectator");
            }
            else
            {
                return ChatStyle.Red + L.S("CommandsGamemodeErrorParmas");
            }
            if (another)
            {
                player.SendMessage(result);
            }

            return result;
        }
    }
}
