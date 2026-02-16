using Vge.Entity.Inventory;

namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Пакет кликов по окну и контролам
    /// </summary>
    public struct PacketC0EClickWindow : IPacket
    {
        public byte Id => 0x0E;

        public byte Action { get; private set; }
        public int Number { get; private set; }
        public bool IsShift { get; private set; }
        public bool IsRight { get; private set; }

        private bool _onlyAction;

        public PacketC0EClickWindow(byte action, int number)
        {
            Action = action;
            _onlyAction = false;
            Number = number;
            IsShift = false;
            IsRight = false;
        }

        public PacketC0EClickWindow(byte action)
        {
            Action = action;
            _onlyAction = true;
            Number = 0;
            IsShift = false;
            IsRight = false;
        }

        public PacketC0EClickWindow(byte action, bool isShift, bool isRight, int number)
        {
            Action = action;
            _onlyAction = false;
            Number = number;
            IsShift = isShift;
            IsRight = isRight;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Action = stream.Byte();
            _onlyAction = stream.Bool();
            if (!_onlyAction)
            {
                IsShift = stream.Bool();
                IsRight = stream.Bool();
                Number = stream.Int();
            }
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Byte(Action);
            stream.Bool(_onlyAction);
            if (!_onlyAction)
            {
                stream.Bool(IsShift);
                stream.Bool(IsRight);
                stream.Int(Number);
            }
        }
    }
}
