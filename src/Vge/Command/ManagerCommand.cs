using System;
using System.Collections.Generic;
using Vge.Games;
using Vge.Realms;

namespace Vge.Command
{
    /// <summary>
    /// Менеджер отвечающий за выполнение команд
    /// </summary>
    public class ManagerCommand
    {
        /// <summary>
        /// Перечень всех доступных команд
        /// </summary>
        private readonly Dictionary<string, CommandBase> _commands = new Dictionary<string, CommandBase>();

        public ManagerCommand(GameServer server)
        {
            //_Registration(new CommandKill(world));
            Registration(new CommandTeleport(server));
            Registration(new CommandTime(server));
            Registration(new CommandSeed(server));

            server.ModServer.InitCommand(this);

            //_Registration(new CommandKick(server));

            //Registration(new CommandFix(world));

            //Registration(new CommandKnowledge(world));
            //Registration(new CommandSpawn(world));
            //Registration(new CommandExperience(world));
            
        }

        /// <summary>
        /// Регистрация команды
        /// </summary>
        public void Registration(CommandBase command)
        {
            if (command.NameMin != "") _commands.Add(command.NameMin, command);
            _commands.Add(command.Name, command);
        }

        /// <summary>
        /// Выполнение команды
        /// </summary>
        /// <param name="sender">Отправитель команды</param>
        /// <returns>Возвращает строку для оповищения в чате консоля конкретному игроку</returns>
        public string ExecutionCommand(CommandSender sender)
        {
            string commandName = sender.GetCommandName();

            if (commandName != "" && commandName[0] == '/')
            {
                commandName = commandName.Substring(1);
                if (commandName != "" && _commands.ContainsKey(commandName))
                {
                    // Команда такая имеется, выполняем
                    return _commands[commandName].UseCommand(sender);
                }
            }
            // Команды такой нет, оповещаем
            return ChatStyle.Red + L.S("CommandsGenericNotFound") + " (" + sender.GetMessage() + ")";
        }
    }
}
