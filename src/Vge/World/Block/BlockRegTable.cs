using System;
using Vge.World.Block.List;

namespace Vge.World.Block
{
    /// <summary>
    /// Таблица блоков для регистрации
    /// </summary>
    public class BlockRegTable
    {
        /// <summary>
        /// Количество элементов
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// Массив псевдонимаов блока
        /// </summary>
        private string[] _alias;
        /// <summary>
        /// Массив объектов блока
        /// </summary>
        private BlockBase[] _blocks;

        public BlockRegTable()
        {
            Count = 0;
            _alias = new string[Count];
            _blocks = new BlockBase[Count];
        }

        public BlockBase this[int index] => _blocks[index];

        /// <summary>
        /// Добавить блок
        /// </summary>
        public void Add(string alias, BlockBase block)
        {
            Count++;
            Array.Resize<string>(ref _alias, Count);
            Array.Resize<BlockBase>(ref _blocks, Count);
            _alias[Count - 1] = alias;
            _blocks[Count - 1] = block;
        }

        /// <summary>
        /// Найти индекс значения, если не нашёл вернёт -1
        /// </summary>
        public int Get(string alias)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_alias[i].Equals(alias)) return i;
            }
            return -1;
        }

        /// <summary>
        /// Найти объект блока, если не нашёл вернёт null
        /// </summary>
        public BlockBase GetBlock(string alias)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_alias[i].Equals(alias)) return _blocks[i];
            }
            return null;
        }

        /// <summary>
        /// Получить названия блока
        /// </summary>
        public string GetAlias(int index) => _alias[index];

        /// <summary>
        /// Очистить массивы
        /// </summary>
        public void Clear()
        {
            _blocks = null;
            _alias = null;
            Count = 0;
        }

        /// <summary>
        /// Получить массив названий
        /// </summary>
        public string[] ToArrayAlias()
        {
            string[] result = new string[Count];
            Array.Copy(_alias, result, Count);
            return result;
        }

        /// <summary>
        /// Пересортировать по заданному массиву ключей
        /// </summary>
        public void Sort(string[] alias)
        {
            int count = alias.Length;
            BlockBase[] blocks = new BlockBase[count];
            int i, id;
            for (i = 0; i < count; i++)
            {
                id = Get(alias[i]);
                if (id != -1)
                {
                    blocks[i] = _blocks[id];
                }
                else
                {
                    // TODO::2024-10-21 BlockNull
                    blocks[i] = new BlockAir(true);
                }
            }
            _alias = alias;
            _blocks = blocks;
            Count = count;
        }

        public override string ToString() => Count.ToString();
    }
}
