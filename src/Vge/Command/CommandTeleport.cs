using Vge.Games;
using Vge.Management;
using Vge.Realms;

namespace Vge.Command
{
    /// <summary>
    /// Команда телепортация игрока
    /// </summary>
    public class CommandTeleport : CommandBase
    {
        public CommandTeleport(GameServer server) : base(server)
        {
            Name = "tp";
        }

        /// <summary>
        /// Возвращает true, если отправителю данной команды разрешено использовать эту команду
        /// </summary>
        public override string UseCommand(CommandSender sender)
        {
            PlayerServer player = sender.GetPlayer();
            if (player == null)
            {
                return ChatStyle.Red + L.S("CommandsTpNotPlayer");
            }
            string[] commandParams = sender.GetCommandParams();
            if (commandParams.Length == 1)
            {
                // Пробуем телепортироваться к игроку если это его имя
                //string login = commandParams[0];
                //PlayerServer entity = Server.Players.FindPlayerByLogin(login);
                //if (entity == null)
                //{
                //    return ChatStyle.Red + L.S("CommandsTpNotPlayer") + " [" + login + "]";
                //}
                //player.SetPositionServer(entity.Position.X, entity.Position.Y, entity.Position.Z);
                //return ChatStyle.Gray + L.S("CommandsTpPlayer") + " " + login;

                // Временно смена мира 0 или 1
                string world = commandParams[0];
                if (world == "0" || world == "1")
                {
                    byte worldId = byte.Parse(world);
                    if (worldId != player.IdWorld)
                    {
                        player.ChangeWorld(worldId);
                        return ChatStyle.Green + L.S("WorldTp") + world;
                    }
                    return ChatStyle.Yellow + L.S("WorldTpNone");
                }

                return ChatStyle.Red + L.S("CommandsTpErrorParmas");
            }
            if (commandParams.Length < 3)
            {
                return ChatStyle.Red + L.S("CommandsTpErrorParmas");
            }
            int[] param = new int[3];
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    param[i] = int.Parse(commandParams[i]);
                }
                catch
                {
                    return ChatStyle.Red + L.S("CommandsTpErrorParmas");
                }
            }
            player.SetPositionServer(param[0], param[1], param[2]);
            return ChatStyle.Green + L.S("CommandsTp") + player.ToStringPosition();
        }
    }
}
