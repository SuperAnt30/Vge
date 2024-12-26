using System;
using Vge.Management;
using Vge.Network;
using Vge.Util;
using Vge.World.Block;

namespace Vge.Command
{
    /// <summary>
    /// Структура отправителя команды
    /// </summary>
    public struct CommandSender
    {
        private string _message;
        private readonly EnumForWhat _forWhat;
        private readonly BlockPos _blockPos;
        private readonly ushort _entityId;
        private PlayerServer _player;

        public CommandSender(string message)
        {
            _message = message;
            _forWhat = EnumForWhat.Null;
            _entityId = 0;
            _blockPos = new BlockPos();
            _player = null;
        }

        public CommandSender(string message, ushort entityId)
        {
            _message = message;
            _forWhat = EnumForWhat.Entity;
            _entityId = entityId;
            _blockPos = new BlockPos();
            _player = null;
        }
        public CommandSender(string message, BlockPos blockPos)
        {
            _message = message;
            _forWhat = EnumForWhat.Entity;
            _entityId = 0;
            _blockPos = blockPos;
            _player = null;
        }

        public CommandSender(string message, MovingObjectPosition movingObject)
        {
            _message = message;
            if (movingObject.IsBlock())
            {
                _forWhat = EnumForWhat.Block;
                _entityId = 0;
                _blockPos = movingObject.BlockPosition;

            }
            else if (movingObject.IsEntity())
            {
                _forWhat = EnumForWhat.Entity;
                _entityId = 0;// movingObject.Entity.Id;
                _blockPos = new BlockPos();
            }
            else
            {
                _forWhat = EnumForWhat.Null;
                _entityId = 0;
                _blockPos = new BlockPos();
            }
            _player = null;
        }

        /// <summary>
        /// Строка сообщения
        /// </summary>
        public string GetMessage() => _message;
        /// <summary>
        /// На что попадает курсор того кто отправлял команду
        /// </summary>
        public EnumForWhat GetForWhat() => _forWhat;
        /// <summary>
        /// Координата блока на который попал курсор
        /// </summary>
        public BlockPos GetBlockPos() => _blockPos;
        /// <summary>
        /// ID сущности на которую попал курсор
        /// </summary>
        public ushort GetEntityId() => _entityId;
        /// <summary>
        /// Получить объект игрока который использует команду
        /// </summary>
        public PlayerServer GetPlayer() => _player;
        /// <summary>
        /// Задать объект игрока который использует команду
        /// </summary>
        public CommandSender SetPlayer(PlayerServer player)
        {
            _player = player;
            return this;
        }

        /// <summary>
        /// Получить название команды в нижнем регистре
        /// </summary>
        public string GetCommandName()
        {
            int index = _message.IndexOf(" ");
            if (index == -1) return _message.ToLower();
            return _message.Substring(0, index).ToLower();
        }
        /// <summary>
        /// Получить массив команд
        /// </summary>
        public string[] GetCommandParams()
        {
            string[] vs = _message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int count = vs.Length;
            if (count < 2) return new string[0];

            string[] vs2 = new string[count - 1];
            for (int i = 1; i < count; i++)
            {
                vs2[i - 1] = vs[i];
            }
            return vs2;
        }

        public void ReadPacket(ReadPacket stream)
        {
            _message = stream.String();
        }
        public void WritePacket(WritePacket stream)
        {
            stream.String(_message);
        }

        public override string ToString() => "[" + _forWhat + "] " + _message;

        /// <summary>
        /// На что попадает луч
        /// </summary>
        public enum EnumForWhat
        {
            Null = 0,
            Block = 1,
            Entity = 2
        }
    }
}
