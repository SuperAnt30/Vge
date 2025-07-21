using Vge.Entity.Player;
using Vge.Games;
using Vge.Realms;
using Vge.World;

namespace Vge.Command
{
    /// <summary>
    /// Команда добавить время
    /// </summary>
    public class CommandTime : CommandBase
    {
        public CommandTime(GameServer server) : base(server)
        {
            Name = "time";
            NameMin = "t";
        }

        /// <summary>
        /// Возвращает true, если отправителю данной команды разрешено использовать эту команду
        /// </summary>
        public override string UseCommand(CommandSender sender)
        {
            PlayerServer player = sender.GetPlayer();
            if (player == null)
            {
                return ChatStyle.Red + L.S("CommandsTimeNotPlayer");
            }
            string[] commandParams = sender.GetCommandParams();
            if (commandParams.Length == 0)
            {
                return ChatStyle.Red + L.S("CommandsTimeNotParmas");
            }

            string param = commandParams[0].ToLower();
            WorldServer world = player.GetWorld();
            int speed = world.Settings.Calendar.GetSpeedDay();
            if (speed == 0)
            {
                // Нет дней
                return ChatStyle.Red + L.S("CommandsTimeNotDayWorld");
            }

            uint totalWorldTick = world.Settings.Calendar.TickCounter;
            uint timeDay = (uint)(totalWorldTick % speed);
            if (param.Equals("day") || param.Equals("d"))
            {
                float f = speed * (7f / 24f); // на 7 утра целимся
                world.SetTickCounter(totalWorldTick + (uint)(timeDay < f ? f : speed + f) - timeDay);
                world.Tracker.SendToAllMessage(ChatStyle.Yellow + L.S("CommandsTimeDay"));
                return "";
            }

            if (param.Equals("night") || param.Equals("n"))
            {
                float f = speed * (19f / 24f); // на 19 часов вечера целимся
                world.SetTickCounter(totalWorldTick + (uint)(timeDay < f ? f : speed + f) - timeDay);
                world.Tracker.SendToAllMessage(ChatStyle.Aqua + L.S("CommandsTimeNight"));
                return "";
            }

            return ChatStyle.Red + L.S("CommandsTimeErrorParmas");
        }

    }
}
