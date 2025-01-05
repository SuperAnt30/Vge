using Vge.Entity;

namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Пакет инпутов клавиатуры используется для физики на сервере
    /// </summary>
    public struct PacketC0CInput : IPacket
    {
        public byte Id => 0x0C;

        /// <summary>
        /// Перемещение вперёд
        /// </summary>
        public bool Forward;
        /// <summary>
        /// Перемещение назад
        /// </summary>
        public bool Back;
        /// <summary>
        /// Шаг влево
        /// </summary>
        public bool Left;
        /// <summary>
        /// Шаг вправо
        /// </summary>
        public bool Right;
        /// <summary>
        /// Прыжок
        /// </summary>
        public bool Jump;
        /// <summary>
        /// Присесть
        /// </summary>
        public bool Sneak;
        /// <summary>
        /// Ускорение
        /// </summary>
        public bool Sprinting;

        public PacketC0CInput(MovementInput movement)
        {
            Forward = movement.Forward;
            Back = movement.Back;
            Left = movement.Left;
            Right = movement.Right;
            Jump = movement.Jump;
            Sneak = movement.Sneak;
            Sprinting = movement.Sprinting;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Forward = stream.Bool();
            Back = stream.Bool();
            Left = stream.Bool();
            Right = stream.Bool();
            Jump = stream.Bool();
            Sneak = stream.Bool();
            Sprinting = stream.Bool();

        }

        public void WritePacket(WritePacket stream)
        {
            stream.Bool(Forward);
            stream.Bool(Back);
            stream.Bool(Left);
            stream.Bool(Right);
            stream.Bool(Jump);
            stream.Bool(Sneak);
            stream.Bool(Sprinting);
        }
    }
}

