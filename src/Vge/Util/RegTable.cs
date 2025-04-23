using System;

namespace Vge.Util
{
    public interface IRegTable
    {
        /// <summary>
        /// Количество элементов
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Найти индекс значения, если не нашёл вернёт -1
        /// </summary>
        int Get(string alias);
        /// <summary>
        /// Получить названия по индексу
        /// </summary>
        string GetAlias(int index);
        /// <summary>
        /// Пересортировать по заданному массиву ключей
        /// </summary>
        void Sort(string[] alias);
    }

    /// <summary>
    /// Таблица для регистрации
    /// </summary>
    public class RegTable<T> : IRegTable
    {
        /// <summary>
        /// Количество элементов
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// Массив псевдонимаов
        /// </summary>
        private string[] _alias;
        /// <summary>
        /// Массив объектов
        /// </summary>
        private T[] _objects;

        public RegTable()
        {
            Count = 0;
            _alias = new string[Count];
            _objects = new T[Count];
        }

        public T this[int index] => _objects[index];

        /// <summary>
        /// Добавить объект
        /// </summary>
        public void Add(string alias, T value)
        {
            Count++;
            Array.Resize<string>(ref _alias, Count);
            Array.Resize<T>(ref _objects, Count);
            _alias[Count - 1] = alias;
            _objects[Count - 1] = value;
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
        /// Найти объект, если не нашёл вернёт null
        /// </summary>
        public T GetObject(string alias)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_alias[i].Equals(alias)) return _objects[i];
            }
            return default(T);
        }

        /// <summary>
        /// Получить названия по индексу
        /// </summary>
        public string GetAlias(int index) => _alias[index];

        /// <summary>
        /// Очистить массивы
        /// </summary>
        public void Clear()
        {
            _objects = null;
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
            T[] objects = new T[count];
            int i, id;
            for (i = 0; i < count; i++)
            {
                id = Get(alias[i]);
                if (id != -1)
                {
                    objects[i] = _objects[id];
                }
                else
                {
                    objects[i] = _CreateNull();
                }
            }
            _alias = alias;
            _objects = objects;
            Count = count;
        }

        /// <summary>
        /// Создание пустого объекта если он был удалён с мира и не смог перезаписать другим новым
        /// </summary>
        protected virtual T _CreateNull() => default(T);

        public override string ToString() => Count.ToString();
    }
}
