using Vge.Games;

namespace Vge.Command
{
    /// <summary>
    /// Абстрактный объект команды
    /// </summary>
    public abstract class CommandBase
    {
        /// <summary>
        /// Основной сервер
        /// </summary>
        public readonly GameServer Server;

        /// <summary>
        /// Название команды в нижнем регистре
        /// </summary>
        public string Name { get; protected set; } = "";
        /// <summary>
        /// Название команды в упрощёном виде
        /// </summary>
        public string NameMin { get; protected set; } = "";

        public CommandBase(GameServer server) => Server = server;

        /// <summary>
        /// Возвращает true, если отправителю данной команды разрешено использовать эту команду
        /// </summary>
        public virtual string UseCommand(CommandSender sender) => "";
    }
}
