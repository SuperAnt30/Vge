using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Util;

namespace Vge.Entity
{
    /// <summary>
    /// Карта объектов прослеживания конкретных сущностей
    /// </summary>
    public class MapEntity<T>
    {
        private readonly Dictionary<int, T> _map = new Dictionary<int, T>();
        private readonly ListMessy<T> _list = new ListMessy<T>();

        /// <summary>
        /// Добавить трек
        /// </summary>
        public void Add(int id, T entity)
        {
            if (!_list.Contains(entity))
            {
                if (_map.ContainsKey(id))
                {
                    _list.Remove(_map[id]);
                    _map.Remove(id);
                }
                _map.Add(id, entity);
                _list.Add(entity);
            }
        }

        /// <summary>
        /// Удалить трек
        /// </summary>
        public void Remove(int id, T entity)
        {
            _map.Remove(id);
            _list.Remove(entity);
        }

        /// <summary>
        /// Проверить наличие объект по id
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsId(int id) => _map.ContainsKey(id);

        /// <summary>
        /// Количество элементов
        /// </summary>
        public int Count => _list.Count;

        /// <summary>
        /// Получить объект по порядковому номеру списка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetAt(int index)
        {
            if (index >= 0 && index < Count) return _list[index];
            return default(T);
        }

        /// <summary>
        /// Получить объект по id сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(int id)
            => _map.ContainsKey(id) ? _map[id] : default(T);

        public override string ToString() => _list.Count.ToString();
    }
}
