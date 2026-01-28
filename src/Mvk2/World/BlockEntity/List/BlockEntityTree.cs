using Mvk2.Util;
using System.Collections.Generic;
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
        private BlockPos _pos = new BlockPos();

        /// <summary>
        /// Дерево
        /// </summary>
        public TreeNode Tree { get; private set; }

        /// <summary>
        /// Список всех блоков без древа
        /// </summary>
        private List<BlockPosLoc> _blocks = new List<BlockPosLoc>();

        /// <summary>
        /// Получить массив всех блоков древесины
        /// Тут позиция от текущего чанко, где зараждено дерево.
        /// т.е. -15..[0..15]..30 (в чанке старт и в любую сторону до 15) 45, нам надо 6 бит.
        /// y << 12 | z << 6 | x
        /// ---- ---- | ---y yyyy | yyyy zzzz | zzxx xxxx
        /// 4 байта, т.е. int
        /// </summary>
        public void SetArray(TreeNode node, ArrayFast<BlockCache> blockCaches)
        {
            Tree = node;
            int count = blockCaches.Count;
            _blocks = new List<BlockPosLoc>(count);
            for (int i = 0; i < count; i++)
            {
                _blocks.Add(new BlockPosLoc(blockCaches[i]));
            }
        }

        public int Count() => _blocks.Count;

        public BlockPos GetBlockPos(int index)
        {
            BlockPosLoc posLoc = _blocks[index];
            _pos.X = posLoc.GetX();
            _pos.Y = posLoc.GetY();
            _pos.Z = posLoc.GetZ();
            return _pos;
        }

        /// <summary>
        /// Найти имеется ли тут блок
        /// </summary>
        public bool FindBlock(BlockPos blockPos)
        {
            int count = _blocks.Count;
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
        /// Поиск узла родителя
        /// </summary>
        private TreeNode _FindBlockParent(int posLoc, TreeNode parent)
        {
            int count = parent.Children.Count;
            if (count > 0)
            {
                TreeNode node;
                for (int i = 0; i < count; i++)
                {
                    node = parent.Children[i];
                    if (node.PosLoc.EqualsPos(posLoc)) return parent;
                    node = _FindBlockParent(posLoc, node);
                    if (node != null) return node;
                }
            }
            return null;
        }

        /// <summary>
        /// Удалить цепочку блоков
        /// </summary>
        public void RemoveBlock(WorldServer world, ChunkServer chunk, BlockPos blockPos)
        {
            List<BlockPos> list = new List<BlockPos>();
            int biasX = (Position.X >> 4) << 4;
            int biasZ = (Position.Z >> 4) << 4;
            int posLoc = blockPos.Y << 12 | (blockPos.Z - biasZ + 16) << 6 | (blockPos.X - biasX + 16);

            if (Tree.PosLoc.EqualsPos(posLoc))
            {
                // Пень, значит надо удалять всё!
                int count = _blocks.Count;
                for (int i = 0; i < count; i++)
                {
                    list.Add(_blocks[i].GetBlockPos(biasX, biasZ));
                }
                _blocks.Clear();
                // Удаляем 
                chunk.RemoveBlockEntity(blockPos);
                //Tree.
            }
            else
            {
                int count = Tree.Children.Count;
                if (count > 0)
                {
                    // Сначало ищем узел родителя
                    TreeNode node = _FindBlockParent(posLoc, Tree);
                    if (node != null)
                    {
                        // Теперь ищем блок и от него начинаем всё удалять
                        _RemoveFindBlock(posLoc, node, list, biasX, biasZ);
                    }
                }
            }

            // Удаляем блоки
            foreach (BlockPos pos in list)
            {
                world.SetBlockToAir(pos);
            }
        }

        private void _RemoveFindBlock(int posLoc, TreeNode inNode, List<BlockPos> list,
            int biasX, int biasZ)
        {
            int count = inNode.Children.Count;
            if (count > 0)
            {
                TreeNode node;
                bool remove = false;
                for (int i = 0; i < count; i++)
                {
                    node = inNode.Children[i];
                    if (remove)
                    {
                        list.Add(node.PosLoc.GetBlockPos(biasX, biasZ));
                        _RemoveBlock(node, list, biasX, biasZ);
                    }
                    else
                    {
                        if (node.PosLoc.EqualsPos(posLoc))
                        {
                            remove = true;
                            list.Add(node.PosLoc.GetBlockPos(biasX, biasZ));
                            _RemoveBlock(node, list, biasX, biasZ);
                        }
                    }
                }
            }
        }

        private void _RemoveBlock(TreeNode inNode, List<BlockPos> list,
            int biasX, int biasZ)
        {
            int count = inNode.Children.Count;
            if (count > 0)
            {
                TreeNode node;
                for (int i = 0; i < count; i++)
                {
                    node = inNode.Children[i];
                    list.Add(node.PosLoc.GetBlockPos(biasX, biasZ));
                    _RemoveBlock(node, list, biasX, biasZ);
                }
            }
        }

        public int GetBlockId(int index) => _blocks[index].Id;

        public override string ToString() => _pos + " " + _blocks.Count;
    }
}
