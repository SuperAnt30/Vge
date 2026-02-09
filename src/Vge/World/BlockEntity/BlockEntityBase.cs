using System.Runtime.CompilerServices;
using Vge.NBT;
using Vge.Util;
using Vge.World.Block;
using Vge.World.Chunk;

namespace Vge.World.BlockEntity
{
    /// <summary>
    /// Базовый объект блока сущности с дополнительными данными, которые нужны для визуализации.
    /// Нужен клиенту.
    /// Пример: Табличка, рамка
    /// В первой части аналог TileEntity
    /// </summary>
    public abstract class BlockEntityBase
    {
        /*
        /// <summary>
        /// Позиция этой сущности по оси X
        /// </summary>
        public int PosX { get; protected set; }
        /// <summary>
        /// Позиция этой сущности по оси Y
        /// </summary>
        public int PosY { get; protected set; }
        /// <summary>
        /// Позиция этой сущности по оси Z
        /// </summary>
        public int PosZ { get; protected set; }
        */

        /// <summary>
        /// Индекс тип блока сущности, полученый на сервере из таблицы
        /// </summary>
        public short IndexEntity { get; protected set; }

        /// <summary>
        /// Позиция к какому блоку принадлежит плитка
        /// </summary>
        public BlockPos Position { get; private set; }
        /// <summary>
        /// Структура данных блока
        /// </summary>
        public BlockState Block { get; private set; }
        /// <summary>
        /// Чанк сервера
        /// TODO::2026-02-07 покуда эти объекты только на стороне сервера
        /// </summary>
        public ChunkServer Chunk { get; private set; }

        /// <summary>
        /// Инициализация для сервера
        /// </summary>
        public virtual void InitServer(short index)
        {
            IndexEntity = index;
            //_world = worldServer;
        }

        /// <summary>
        /// Задать позицию блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetBlockPosition(ChunkServer chunk, BlockState blockState, BlockPos pos)
        {
            Chunk = chunk;
            Position = pos;
            Block = blockState;
        }

        /// <summary>
        /// Задать блок по чанку, зная до этого позицию блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitBlock(ChunkServer chunk)
            => Block = chunk.GetBlockState(Position.X & 15, Position.Y, Position.Z & 15);

        /// <summary>
        /// Задать новый такт
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTick(ChunkServer chunk, int timeTick, bool priority = false)
            => chunk.SetBlockTick(Position.X & 15, Position.Y, Position.Z & 15, priority, (uint)timeTick);


        /// <summary>
        /// Обновить блок плитки в такте
        /// </summary>
        public virtual void UpdateTick(WorldServer world, ChunkServer chunk, Rand random) { }

        public virtual void WriteToNBT(TagCompound nbt)
        {
            nbt.SetShort("Id", IndexEntity);
            nbt.SetInt("X", Position.X);
            nbt.SetInt("Y", Position.Y);
            nbt.SetInt("Z", Position.Z);
        }

        public virtual void ReadFromNBT(TagCompound nbt)
        {
            Position = new BlockPos(nbt.GetInt("X"), nbt.GetInt("Y"), nbt.GetInt("Z"));
        }
    }
}
