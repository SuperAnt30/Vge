using Vge.Entity.Player;
using Vge.Games;
using Vge.Realms;
using Vge.World;

namespace Vge.Command
{
    /// <summary>
    /// Команда погода
    /// </summary>
    public class CommandWeather : CommandBase
    {
        public CommandWeather(GameServer server) : base(server)
        {
            Name = "weather";
            NameMin = "w";
        }

        /// <summary>
        /// Возвращает true, если отправителю данной команды разрешено использовать эту команду
        /// </summary>
        public override string UseCommand(CommandSender sender)
        {
            PlayerServer player = sender.GetPlayer();
            if (player == null)
            {
                return ChatStyle.Red + L.S("CommandsWeatherNotPlayer");
            }
            string[] commandParams = sender.GetCommandParams();
            if (commandParams.Length == 0)
            {
                return ChatStyle.Red + L.S("CommandsWeatherNotParmas");
            }

            string param = commandParams[0].ToLower();
            WorldServer worldServer = player.GetWorldServer();

            if (param.Equals("thunder") || param.Equals("t"))
            {
                worldServer.Settings.Environment.SetEnvironmentServer(6);
                worldServer.Tracker.SendToAllMessage(ChatStyle.Yellow + L.S("CommandsWeatherThunder"));
                return "";
            }
            if (param.Equals("clear") || param.Equals("c"))
            {
                worldServer.Settings.Environment.SetEnvironmentServer(0);
                worldServer.Settings.Environment.SetEnvironmentServer(7);
                worldServer.Tracker.SendToAllMessage(ChatStyle.Yellow + L.S("CommandsWeatherClear"));
                return "";
            }
            if (param.Equals("rain") || param.Equals("r"))
            {
                worldServer.Settings.Environment.SetEnvironmentServer(8);
                worldServer.Tracker.SendToAllMessage(ChatStyle.Yellow + L.S("CommandsWeatherRain"));
                return "";
            }
            if (param.Equals("showers") || param.Equals("s"))
            {
                worldServer.Settings.Environment.SetEnvironmentServer(9);
                worldServer.Tracker.SendToAllMessage(ChatStyle.Yellow + L.S("CommandsWeatherShowers"));
                return "";
            }
            if (param.Equals("mostly") || param.Equals("m"))
            {
                worldServer.Settings.Environment.SetEnvironmentServer(3);
                worldServer.Tracker.SendToAllMessage(ChatStyle.Yellow + L.S("CommandsWeatherMostlyCloudy"));
                return "";
            }
            if (param.Equals("overcast") || param.Equals("o"))
            {
                worldServer.Settings.Environment.SetEnvironmentServer(5);
                worldServer.Tracker.SendToAllMessage(ChatStyle.Yellow + L.S("CommandsWeatherOvercast"));
                return "";
            }

            return ChatStyle.Red + L.S("CommandsWeatherErrorParmas");
        }
    }
}
