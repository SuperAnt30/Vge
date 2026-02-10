using Mvk2.World.Block;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.BlockEntity;
using Vge.World.Chunk;
using Vge.World.Сalendar;

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
        public BlockPosLoc[] Blocks { get; private set; }
        /// <summary>
        /// Хитбокс древа
        /// </summary>
        private int _minX, _minY, _minZ, _maxX, _maxY, _maxZ;
        /// <summary>
        /// Пустой ли
        /// </summary>
        private bool _empty = true;
        /// <summary>
        /// Шаги роста
        /// </summary>
        public TypeStep Step { get; private set; } = TypeStep.Young;
        /// <summary>
        /// Длинна корня
        /// </summary>
        public int RootLenght { get; private set; } = 0;
        /// <summary>
        /// Готов ли для урожая
        /// </summary>
        private bool _isFetus;

        /// <summary>
        /// Типа этапа роста дерева
        /// </summary>
        public enum TypeStep
        {
            /// <summary>
            /// Молодое дерево
            /// </summary>
            Young,
            /// <summary>
            /// Взрослое дерево
            /// </summary>
            Mature,
            /// <summary>
            /// Сухое дерево
            /// </summary>
            Dry
        }

        /// <summary>
        /// Вариант состояния дерева
        /// </summary>
        public enum StateVariant
        {
            /// <summary>
            /// Без изменения
            /// </summary>
            Norm,
            /// <summary>
            /// Надо указать листве, что надо сохнуть, Leaves.Met += 512;
            /// </summary>
            Dry,
            /// <summary>
            /// Надо готовить листву для урожая, Leaves.Met += 256;
            /// </summary>
            Fetus,
            /// <summary>
            /// Отменить готовность листвы к урожаю, Leaves.Met -= 256;
            /// </summary>
            Cancel
        }

        #region Методы изменения

        /// <summary>
        /// Задать взрослое дерево, с длинной корня
        /// </summary>
        public void StepMature(int rootLenght)
        {
            Step = TypeStep.Mature;
            RootLenght = rootLenght;
        }

        /// <summary>
        /// Получить массив всех блоков древесины, в локальных координатах
        /// Тут позиция от текущего чанка, где зараждено дерево.
        /// Массив должен быть правельный, Parent всегда идти на увеличения исключение новых веток.
        /// Ветка идёт по очереди без обрывов.
        /// </summary>
        public void SetArrayLocal(ArrayFast<BlockCache> blockCaches)
        {
            int count = blockCaches.Count;
            List<BlockPosLoc> list = new List<BlockPosLoc>();
            for (int i = 0; i < count; i++)
            {
                if (blockCaches[i].ParentIndex > -2)
                {
                    list.Add(new BlockPosLoc(blockCaches[i]));
                }
            }
            Blocks = list.ToArray();

            _UpdateAxis();
        }

        #endregion

        #region Remove

        /// <summary>
        /// Удалить цепочку блоков,
        /// Возвращает true если хоть один блок удалён
        /// </summary>
        public bool RemoveBlock(WorldServer world, ChunkServer chunk, BlockPos blockPos)
        {
            // Список для пополнения блоков удаления по цепочке дерева
            List<PosId> list = new List<PosId>();
            int biasX = (Position.X >> 4) << 4;
            int biasZ = (Position.Z >> 4) << 4;
            int posLoc = blockPos.Y << 12 | (blockPos.Z - biasZ + 16) << 6 | (blockPos.X - biasX + 16);
            int count = Blocks.Length;
            
            bool remove = false;
            int id = 0;
            for (int i = 0; i < count; i++)
            {
                if (!remove)
                {
                    // Ищем
                    if (Blocks[i].EqualsPos(posLoc))
                    {
                        remove = true;
                        list.Add(new PosId(Blocks[i].Id, i, Blocks[i].GetBlockPos(biasX, biasZ)));
                        id = i;
                    }
                }
                else
                {
                    // Найден
                    if (Blocks[i].ParentIndex < id)
                    {
                        break;
                    }
                    else
                    {
                        list.Add(new PosId(Blocks[i].Id, i, Blocks[i].GetBlockPos(biasX, biasZ)));
                    }
                }
            }

            if (list.Count > 0)
            {
                // Удалить кэш блоков надо до удаления их в мире
                int countNew = Blocks.Length - list.Count;
                if (countNew == 0)
                {
                    // Полностью удалили
                    Blocks = new BlockPosLoc[0];
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
                            blocksNew[i] = Blocks[i];
                        }
                        else
                        {
                            posLoc2 = Blocks[i + bias];
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
                    
                    Blocks = blocksNew;
                }

                _UpdateAxis();

                // Удаляем блоки, уже при обращении BlockEntytyTree этих блоков быть не должно у этого дерева
                foreach (PosId pos in list)
                {
                    if (pos.Id == world.GetBlockState(pos.Pos).Id)
                    {
                        world.SetBlockToAir(pos.Pos, 110); // 2 4 8 32 64 без частичек и звука, и отключен OnBreakBlock 
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Удалить все блоки
        /// </summary>
        public void RemoveAll(WorldServer world)
        {
            BlockPosLoc[] blocks = CopyArrayBlocks();

            int biasX = (Position.X >> 4) << 4;
            int biasZ = (Position.Z >> 4) << 4;

            // Полностью удалили
            Blocks = new BlockPosLoc[0];
            _UpdateAxis();

            // Удаляем блоки, уже при обращении BlockEntytyTree этих блоков быть не должно у этого дерева
            BlockPos pos = new BlockPos();
            foreach (BlockPosLoc posLoc in blocks)
            {
                pos.X = posLoc.X + biasX;
                pos.Y = posLoc.Y;
                pos.Z = posLoc.Z + biasZ;
                if (posLoc.Id == world.GetBlockState(pos).Id)
                {
                    world.SetBlockToAir(pos, 110); // 2 4 8 32 64
                }
            }
        }

        /// <summary>
        /// Дерево засохло, надо пометить всю листву на высыхание
        /// </summary>
        public void DriedUp() => Step = TypeStep.Dry;

        /// <summary>
        /// Удалить все блоки листвы и плодов
        /// </summary>
        private void RemoveAllLeaves(WorldServer world)
        {
            int count = Blocks.Length;
            if (count > 0)
            {
                int biasX = (Position.X >> 4) << 4;
                int biasZ = (Position.Z >> 4) << 4;

                BlockPosLoc posLoc;
                BlockPos pos = new BlockPos();
                BlockState state;
                // Пробегаемся по всем блокам траектории дерева, и удаляем рядом листву.
                // Алгоритм упрощён, иметируем удаление блока, он тащит за собой удаление листвы,
                // а потом возвращаем блок
                for (int i = 0; i < count; i++)
                {
                    posLoc = Blocks[i];
                    pos.X = posLoc.X + biasX;
                    pos.Y = posLoc.Y;
                    pos.Z = posLoc.Z + biasZ;
                    state = world.GetBlockState(pos);
                    
                    if (posLoc.Id == state.Id)
                    {
                        
                        world.SetBlockToAir(pos, 66); // 64 блокировка, 2 смена соседа
                        world.SetBlockState(pos, state, 64);

                        // TODO::2026-02-05 сделать вкусный вариант на мире сервера, для проверки боковых. без изменения
                        // А может и не надо, это не так часто бывает!
                        //world._NotifyNeighborsOfStateChange(pos, stateAir.GetBlock());
                    }
                }
            }
            Step = TypeStep.Dry;
        }

        #endregion

        /// <summary>
        /// Проверить состояние дерева, только для Mature
        /// </summary>
        public StateVariant CheckingCondition(WorldServer world)
        {
            // Надо проверить корень
            int rootLenght = _GetRootLenght(Chunk, Position);
            if (rootLenght < RootLenght)
            {
                // Корень короче, предлагаю дерево высушить
                Step = TypeStep.Dry;
                return StateVariant.Dry;
            }
            else
            {
                EnumTimeYear timeYear = world.Settings.Calendar.TimeYear;
                if (timeYear == EnumTimeYear.Summer)
                {
                    // Погода, для роста (лето)
                    if (!_isFetus)
                    {
                        // Тут надо проверка корня, и удобрения
                        _isFetus = true;
                        return StateVariant.Fetus;
                    }
                }
                else if (timeYear == EnumTimeYear.Winter || timeYear == EnumTimeYear.Spring)
                {
                    // Убрать рост (зима, весна)
                    if (_isFetus)
                    {
                        _isFetus = false;
                        return StateVariant.Cancel;
                    }
                }
            }
            return StateVariant.Norm;
        }

        /// <summary>
        /// Меряем фактическую глубину корня
        /// </summary>
        private int _GetRootLenght(ChunkServer chunk, BlockPos blockPos)
        {
            int y = blockPos.Y - 1;
            int xz = (blockPos.Z & 15) << 4 | (blockPos.X & 15);
            int length = 0;
            while (chunk.GetBlockStateNotCheckLight(xz, y).Id == BlocksRegMvk.TreeRoot.IndexBlock)
            {
                length++;
                y--;
            }
            return length;
        }

        /// <summary>
        /// Найти имеется ли блок
        /// Покуда не используется 2026-01-30
        /// </summary>
        public bool FindBlock(BlockPos blockPos)
        {
            int count = Blocks.Length;
            if (count > 0)
            {
                int x = (Position.X >> 4) << 4;
                int z = (Position.Z >> 4) << 4;
                int posLoc = blockPos.Y << 12 | (blockPos.Z - z + 16) << 6 | (blockPos.X - x + 16);
                for (int i = 0; i < count; i++)
                {
                    if (Blocks[i].EqualsPos(posLoc)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Обновление блока листвы
        /// </summary>
        //public void UpdateTickLeaves(WorldServer world, ChunkServer chunk, 
        //    BlockPos blockPosBranch, BlockPos blockPosLeaves, int idFetus)
        //{
        //    if (Step == TypeStep.Dry)
        //    {
        //        RemoveBlock(world, chunk, blockPosBranch);
        //    }
        //    else if (chunk.GetBlockStateNotCheck(blockPosLeaves.OffsetDown()).Id == 0)
        //    {
        //        world.SetBlockState(blockPosLeaves.OffsetDown(), new BlockState(idFetus), 12);
        //    }
        //}

        /// <summary>
        /// Попадает ли блок в AABB древа
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAABB(BlockPos pos) => !_empty && pos.X >= _minX && pos.X <= _maxX
            && pos.Y >= _minY && pos.Y <= _maxY && pos.Z >= _minZ && pos.Z <= _maxZ;

        /// <summary>
        /// Сделать копию блоков
        /// </summary>
        public BlockPosLoc[] CopyArrayBlocks()
        {
            BlockPosLoc[] blocks = new BlockPosLoc[Blocks.Length];
            Array.Copy(Blocks, blocks, blocks.Length);
            return blocks;
        }

        /// <summary>
        /// Получить высоту дерева
        /// </summary>
        public int GetHeight() => _maxY - _minY;

        /// <summary>
        /// Обновить размер хитбокса
        /// </summary>
        private void _UpdateAxis()
        {
            int count = Blocks.Length;
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
                    posLoc = Blocks[i];
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
            + "] Tree:" + Blocks.Length
            + " Root:" + RootLenght + " " + Step;

        /// <summary>
        /// Дополнительная структура, для удаления
        /// </summary>
        private readonly struct PosId
        {
            /// <summary>
            /// Id блока
            /// </summary>
            public readonly int Id;
            /// <summary>
            /// Порядковый индекс в массиве
            /// </summary>
            public readonly int Index;
            /// <summary>
            /// Позиция
            /// </summary>
            public readonly BlockPos Pos;

            public PosId(int index, BlockPos pos)
            {
                Id = -1;
                Index = index;
                Pos = pos;
            }
            public PosId(int id, int index, BlockPos pos)
            {
                Id = id;
                Index = index;
                Pos = pos;
            }

            public override string ToString() => Index + " [" + Pos + "]";
        }
    }
}
