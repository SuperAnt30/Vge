using Vge.Command;
using Vge.Util;

namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Пакет передачии сообщения или команды на сервер
    /// </summary>
    public struct PacketC14Message : IPacket
    {
        public byte Id => 0x14;

        private CommandSender _sender;

        public PacketC14Message(string message, MovingObjectPosition movingObject)
            => _sender = new CommandSender(message, movingObject);
        
        /// <summary>
        /// Структура отправителя комсанды
        /// </summary>
        public CommandSender GetCommandSender() => _sender;

        public void ReadPacket(ReadPacket stream) => _sender.ReadPacket(stream);
        public void WritePacket(WritePacket stream) => _sender.WritePacket(stream);
    }
}
