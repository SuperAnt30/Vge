using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Util;

namespace Vge.Entity
{
    /// <summary>
    /// Карта объектов прослеживания конкретных сущностей
    /// </summary>
    public class MapEntityTrackerEntry
    {
        private readonly Dictionary<int, EntityTrackerEntry> _map = new Dictionary<int, EntityTrackerEntry>();
        private readonly ListMessy<EntityTrackerEntry> _list = new ListMessy<EntityTrackerEntry>();

        /// <summary>
        /// Добавить трек
        /// </summary>
        public void Add(EntityTrackerEntry entity)
        {
            if (!_list.Contains(entity))
            {
                int id = entity.TrackedEntity.Id;
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
        public void Remove(EntityTrackerEntry entity)
        {
            _map.Remove(entity.TrackedEntity.Id);
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
        public EntityTrackerEntry GetAt(int index)
        {
            if (index >= 0 && index < Count) return _list[index];
            return null;
        }

        /// <summary>
        /// Получить объект по id сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityTrackerEntry Get(int id) 
            => _map.ContainsKey(id) ? _map[id] : null;

        public override string ToString() => _list.Count.ToString();
    }
}
