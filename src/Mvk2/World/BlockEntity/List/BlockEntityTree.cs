using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.BlockEntity;
using Vge.World.Chunk;

namespace Mvk2.World.BlockEntity.List
{
    /// <summary>
    /// Блок сущности дерева
    /// </summary>
    public class BlockEntityTree : BlockEntityBase
    {
        /// <summary>
        /// Массив всех блоков дерева
        /// </summary>
        private BlockPosLoc[] _blocks;
        /// <summary>
        /// Хитбокс древа
        /// </summary>
        private int _minX, _minY, _minZ, _maxX, _maxY, _maxZ;
        /// <summary>
        /// Пустой ли
        /// </summary>
        private bool _empty = true;

        /// <summary>
        /// Получить массив всех блоков древесины.
        /// Тут позиция от текущего чанка, где зараждено дерево.
        /// Массив должен быть правельный, Parent всегда идти на увеличения исключение новых веток.
        /// Ветка идёт по очереди без обрывов.
        /// </summary>
        public void SetArray(ArrayFast<BlockCache> blockCaches)
        {
            int count = blockCaches.Count;
            _blocks = new BlockPosLoc[count];
            for (int i = 0; i < count; i++)
            {
                _blocks[i] = new BlockPosLoc(blockCaches[i]);
            }
            _UpdateAxis();
        }

        /// <summary>
        /// Попадает ли блок в AABB древа
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAABB(BlockPos pos) => !_empty && pos.X >= _minX && pos.X <= _maxX
            && pos.Y >= _minY && pos.Y <= _maxY && pos.Z >= _minZ && pos.Z <= _maxZ;

        /// <summary>
        /// Найти имеется ли блок
        /// Покуда не используется 2026-01-30
        /// </summary>
        public bool FindBlock(BlockPos blockPos)
        {
            int count = _blocks.Length;
            if (count > 0)
            {
                int x = (Position.X >> 4) << 4;
                int z = (Position.Z >> 4) << 4;
                int posLoc = blockPos.Y << 12 | (blockPos.Z - z + 16) << 6 | (blockPos.X - x + 16);
                for (int i = 0; i < count; i++)
                {
                    if (_blocks[i].EqualsPos(posLoc)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Удалить цепочку блоков
        /// </summary>
        public void RemoveBlock(WorldServer world, ChunkServer chunk, BlockPos blockPos)
        {
            // Список для пополнения блоков удаления по цепочке дерева
            List<PosId> list = new List<PosId>();
            int biasX = (Position.X >> 4) << 4;
            int biasZ = (Position.Z >> 4) << 4;
            int posLoc = blockPos.Y << 12 | (blockPos.Z - biasZ + 16) << 6 | (blockPos.X - biasX + 16);
            int count = _blocks.Length;

            bool remove = false;
            int id = 0;
            for (int i = 0; i < count; i++)
            {
                if (!remove)
                {
                    // Ищем
                    if (_blocks[i].EqualsPos(posLoc))
                    {
                        remove = true;
                        list.Add(new PosId(i, _blocks[i].GetBlockPos(biasX, biasZ)));
                        id = i;
                    }
                }
                else
                {
                    // Найден
                    if (_blocks[i].ParentIndex < id)
                    {
                        break;
                    }
                    else
                    {
                        list.Add(new PosId(i, _blocks[i].GetBlockPos(biasX, biasZ)));
                    }
                }
            }

            if (list.Count > 0)
            {
                // Удалить кэш блоков надо до удаления их в мире
                int countNew = _blocks.Length - list.Count;
                if (countNew == 0)
                {
                    // Полностью удалили
                    _blocks = new BlockPosLoc[0];
                    // Удаляем BlockEntity
                    chunk.RemoveBlockEntity(blockPos);
                }
                else
                {
                    BlockPosLoc posLoc2;
                    BlockPosLoc[] blocksNew = new BlockPosLoc[countNew];
                    int idBegin = list[0].Index; // 13
                    int bias = list[list.Count - 1].Index - idBegin + 1; // 20 - 13 + 1 = 8
                    for (int i = 0; i < countNew; i++)
                    {
                        if (i < idBegin)
                        {
                            // Начальные блоки остаются как есть
                            blocksNew[i] = _blocks[i];
                        }
                        else
                        {
                            posLoc2 = _blocks[i + bias];
                            if (posLoc2.ParentIndex < idBegin)
                            {
                                blocksNew[i] = posLoc2;
                            }
                            else
                            {
                                blocksNew[i] = new BlockPosLoc(posLoc2, posLoc2.ParentIndex - bias);
                            }
                        }
                    }
                    
                    _blocks = blocksNew;
                }

                _UpdateAxis();

                // Удаляем блоки, уже при обращении BlockEntytyTree этих блоков быть не должно у этого дерева
                foreach (PosId pos in list)
                {
                    world.SetBlockToAir(pos.Pos);
                }
            }
        }

        /// <summary>
        /// Обновить размер хитбокса
        /// </summary>
        private void _UpdateAxis()
        {
            int count = _blocks.Length;
            if (count > 0)
            {
                _empty = false;
                _minX = int.MaxValue;
                _minY = int.MaxValue;
                _minZ = int.MaxValue;
                _maxX = int.MinValue;
                _maxY = int.MinValue;
                _maxZ = int.MinValue;

                BlockPosLoc posLoc;
                for (int i = 0; i < count; i++)
                {
                    posLoc = _blocks[i];
                    if (posLoc.X < _minX) _minX = posLoc.X;
                    if (posLoc.Y < _minY) _minY = posLoc.Y;
                    if (posLoc.Z < _minZ) _minZ = posLoc.Z;
                    if (posLoc.X > _maxX) _maxX = posLoc.X;
                    if (posLoc.Y > _maxY) _maxY = posLoc.Y;
                    if (posLoc.Z > _maxZ) _maxZ = posLoc.Z;
                }
                int biasX = (Position.X >> 4) << 4;
                int biasZ = (Position.Z >> 4) << 4;
                _minX += biasX;
                _maxX += biasX;
                _minZ += biasZ;
                _maxZ += biasZ;
            }
            else
            {
                _empty = true;
            }
        }

        public override string ToString() 
            => Position 
            + " Box[" + (_empty ? "empty" : (_minX + "; " + _minY + "; " + _minZ
            + " -> " + _maxX + "; " + _maxY + "; " + _maxZ))
            + "] Tree:" + _blocks.Length;

        /// <summary>
        /// Дополнительная структура, для удаления
        /// </summary>
        private readonly struct PosId
        {
            public readonly BlockPos Pos;
            public readonly int Index;

            public PosId(int index, BlockPos pos)
            {
                Index = index;
                Pos = pos;
            }

            public override string ToString() => Index + " [" + Pos + "]";
        }
    }
}
