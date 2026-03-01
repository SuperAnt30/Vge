using Vge.Util;
using Vge.World.Block;
using WinGL.Util;

namespace Vge.Network.Packets.Client
{
    /// <summary>
    /// Отправляем на сервер установка или взаимодействия с блоком
    /// </summary>
    public struct PacketC08PlayerBlockPlacement : IPacket
    {
        public byte Id => 0x08;

        /// <summary>
        /// С какой стороны устанавливаем блок
        /// </summary>
        public Pole Side { get; private set; }
        /// <summary>
        /// Точка куда устанавливаем блок (параметр с RayCast)
        /// значение в пределах 0..1, образно фиксируем пиксел клика на стороне
        /// </summary>
        public Vector3 Facing { get; private set; }

        private BlockPos _blockPos;
        /// <summary>
        /// Позиция блока где устанавливаем блок
        /// </summary>
        public BlockPos GetBlockPos() => _blockPos;

        /// <summary>
        /// Тип действия
        /// </summary>
        public EnumAction Action { get; private set; }

        /// <summary>
        /// Установить блок
        /// </summary>
        public PacketC08PlayerBlockPlacement(BlockPos blockPos, Pole side, Vector3 facing)
        {
            _blockPos = blockPos;
            Side = side;
            Facing = facing;
            Action = EnumAction.PutBlock;
        }

        /// <summary>
        /// Взаимодействие блок, с предметом или без
        /// </summary>
        public PacketC08PlayerBlockPlacement(BlockPos blockPos, bool isItem, bool second)
        {
            _blockPos = blockPos;
            Side = Pole.All;
            Facing = new Vector3();
            Action = isItem 
                ? second ? EnumAction.ItemSecondOnBlock : EnumAction.ItemOnBlock 
                : second ? EnumAction.UseSecondBlock : EnumAction.UseBlock;
        }

        /// <summary>
        /// Взаимодействие предмета без блока
        /// </summary>
        public PacketC08PlayerBlockPlacement(bool second)
        {
            _blockPos = new BlockPos();
            Side = Pole.All;
            Facing = new Vector3();
            Action = second ? EnumAction.UseSecondItem : EnumAction.UseItem;
        }

        public void ReadPacket(ReadPacket stream)
        {
            Action = (EnumAction)stream.Byte();
            if (Action != EnumAction.UseItem && Action != EnumAction.UseSecondItem)
            {
                _blockPos.ReadStream(stream);
                if (Action == EnumAction.PutBlock)
                {
                    Side = (Pole)stream.Byte();
                    Facing = new Vector3(stream.Byte() / 16f, stream.Byte() / 16f, stream.Byte() / 16f);
                }
            }
        }

        public void WritePacket(WritePacket stream)
        {
            stream.Byte((byte)Action);
            if (Action != EnumAction.UseItem && Action != EnumAction.UseSecondItem)
            {
                _blockPos.WriteStream(stream);
                if (Action == EnumAction.PutBlock)
                {
                    stream.Byte((byte)Side);
                    stream.Byte((byte)(Facing.X * 16f));
                    stream.Byte((byte)(Facing.Y * 16f));
                    stream.Byte((byte)(Facing.Z * 16f));
                }
            }
        }

        /// <summary>
        /// Варианты действия
        /// </summary>
        public enum EnumAction
        {
            /// <summary>
            /// Поставить блок
            /// </summary>
            PutBlock = 1,
            /// <summary>
            /// Взаимодействие предмета в правой руке на выбранный блок ЛКМ
            /// </summary>
            ItemOnBlock = 2,
            /// <summary>
            /// Дополнительное взаимодействие предмета в правой руке на выбранный блок ПКМ
            /// </summary>
            ItemSecondOnBlock = 3,
            /// <summary>
            /// Взаимодействие выбранного блока ЛКМ
            /// </summary>
            UseBlock = 4,
            /// <summary>
            /// Дополнительное взаимодействие выбранного блока ПКМ
            /// </summary>
            UseSecondBlock = 5,
            /// <summary>
            /// Взаимодействия предмета, ЛКМ без блока
            /// </summary>
            UseItem = 6,
            /// <summary>
            /// Дополнительное взаимодействия предмета, ПКМ без блока
            /// </summary>
            UseSecondItem = 7
        }
    }
}
